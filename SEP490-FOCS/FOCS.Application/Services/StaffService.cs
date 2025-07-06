using AutoMapper;
using FOCS.Application.DTOs;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Application.Services
{
    public class StaffService : IStaffService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRepository<UserStore> _userStoreRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IRepository<Store> _storeRepository;

        public StaffService(
            UserManager<User> userManager,
            IRepository<UserStore> userStoreRepository,
            IMapper mapper,
            IEmailService emailService,
            IRepository<Store> storeRepository)
        {
            _userManager = userManager;
            _userStoreRepository = userStoreRepository;
            _mapper = mapper;
            _emailService = emailService;
            _storeRepository = storeRepository;
            _userStoreRepository = userStoreRepository;
        }

        public async Task<StaffProfileDTO> CreateStaffAsync(RegisterRequest request, string storeId, string managerId)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(storeId, out Guid storeIdGuid),
                                                    Errors.Common.InvalidGuidFormat,
                                                    Errors.FieldName.StoreId);
            var store = await _storeRepository.GetByIdAsync(storeIdGuid);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound, Errors.FieldName.StoreId);

            var staff = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
                PhoneNumber = request.Phone
            };

            var result = await _userManager.CreateAsync(staff, request.Password);
            ConditionCheck.CheckCondition(result.Succeeded,
                    string.Join("; ", result.Errors.Select(e => e.Description)));

            var newUserStore = new UserStoreDTO
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(staff.Id),
                StoreId = storeIdGuid,
                BlockReason = null,
                JoinDate = DateTime.UtcNow,
                Status = Common.Enums.UserStoreStatus.Active
            };

            await _userStoreRepository.AddAsync(_mapper.Map<UserStore>(newUserStore));
            await _userStoreRepository.SaveChangesAsync();

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(staff);

            await _emailService.SendEmailConfirmationAsync(staff.Email, token);

            return _mapper.Map<StaffProfileDTO>(staff);
        }

        public async Task<PagedResult<StaffProfileDTO>> GetStaffListAsync(UrlQueryParameters query, string storeId)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(storeId, out Guid storeIdGuid),
                                                    Errors.Common.InvalidGuidFormat,
                                                    Errors.FieldName.StoreId);
            var userStores = await _userStoreRepository.FindAsync(us => us.StoreId == storeIdGuid &&
                                                                       us.Status == Common.Enums.UserStoreStatus.Active);
            var userIds = userStores.Select(us => us.UserId.ToString()).ToList();

            var allUsers = _userManager.Users.Where(u => u.IsActive && !u.IsDeleted && userIds.Contains(u.Id)).ToList();

            // Filter out users with "User" role
            var staff = new List<StaffProfileDTO>();
            foreach (var user in allUsers)
            {
                if (!await _userManager.IsInRoleAsync(user, Roles.User))
                {
                    var dto = _mapper.Map<StaffProfileDTO>(user);
                    dto.Roles = await _userManager.GetRolesAsync(user);
                    staff.Add(dto);
                }
            }

            var staffQuery = staff.AsQueryable();

            staffQuery = ApplyFilters(staffQuery, query);
            staffQuery = ApplySearch(staffQuery, query);
            staffQuery = ApplySort(staffQuery, query);

            var total = staffQuery.Count();
            var items = staffQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            return new PagedResult<StaffProfileDTO>(items, total, query.Page, query.PageSize);
        }

        public async Task<StaffProfileDTO> GetStaffProfileAsync(string staffId, string managerId)
        {
            var staff = await ValidatePermissionAsync(staffId, managerId);
            ConditionCheck.CheckCondition(staff != null, Errors.Common.UserNotFound);

            var result = _mapper.Map<StaffProfileDTO>(staff);
            result.Roles = await _userManager.GetRolesAsync(staff);
            return result;
        }

        public async Task<StaffProfileDTO> UpdateStaffProfileAsync(StaffProfileDTO dto, string staffId, string managerId)
        {
            var staff = await ValidatePermissionAsync(staffId, managerId);
            ConditionCheck.CheckCondition(staff != null, Errors.Common.UserNotFound);

            dto.Email = staff.Email;
            _mapper.Map(dto, staff);
            staff.UpdatedAt = DateTime.UtcNow;
            staff.UpdatedBy = staffId;

            await _userManager.UpdateAsync(staff);

            var result = _mapper.Map<StaffProfileDTO>(staff);
            result.Roles = await _userManager.GetRolesAsync(staff);
            return result;
        }

        public async Task<bool> DeleteStaffAsync(string staffId, string managerId)
        {
            var staff = await ValidatePermissionAsync(staffId, managerId);
            ConditionCheck.CheckCondition(staff != null, Errors.Common.UserNotFound);

            staff.IsActive = false;
            staff.IsDeleted = true;
            staff.UpdatedAt = DateTime.UtcNow;
            staff.UpdatedBy = staffId;

            await _userManager.UpdateAsync(staff);
            return true;
        }

        public async Task<bool> AddStaffRoleAsync(string role, string staffId, string managerId)
        {
            var staff = await ValidatePermissionAsync(staffId, managerId);
            var roleToAssign = GetValidRoleAsync(role);
            await _userManager.AddToRoleAsync(staff, roleToAssign);

            return true;
        }

        public async Task<bool> RemoveStaffRoleAsync(string role, string staffId, string managerId)
        {
            var staff = await ValidatePermissionAsync(staffId, managerId);

            var roleToRemove = GetValidRoleAsync(role);
            await _userManager.RemoveFromRoleAsync(staff, roleToRemove);
            return true;
        }

        #region Private Helper Methods

        private async Task<User> ValidatePermissionAsync(string staffId, string managerId)
        {
            var staff = await _userManager.FindByIdAsync(staffId);
            ConditionCheck.CheckCondition(staff != null, Errors.Common.UserNotFound);

            var manager = await _userManager.FindByIdAsync(managerId);
            ConditionCheck.CheckCondition(manager != null, Errors.AuthError.UserUnauthor);

            var roles = await _userManager.GetRolesAsync(staff);
            ConditionCheck.CheckCondition(!roles.Contains(Roles.User), Errors.AuthError.UserUnauthor);
            await ValidateStoreAuthorizationAsync(staffId, managerId);

            return staff;
        }

        private async Task ValidateStoreAuthorizationAsync(string staffId, string managerId)
        {
            var staffStoreId = await _userStoreRepository.AsQueryable()
                .Where(us => us.UserId.ToString().Equals(staffId))
                .FirstOrDefaultAsync();

            var managerStoreId = await _userStoreRepository.AsQueryable()
                .Where(us => us.UserId.ToString().Equals(managerId))
                .ToListAsync();

            ConditionCheck.CheckCondition(
                !staffStoreId.Equals(null) && !managerStoreId.Equals(null) && managerStoreId.Select(x => x.StoreId).Contains(staffStoreId.StoreId),
                Errors.AuthError.UserUnauthor);
        }

        private string GetValidRoleAsync(string role)
        {
            var normalizedRole = role.ToLowerInvariant();

            var validRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    Roles.Staff,
                    Roles.KitchenStaff,
                    Roles.Manager
                };
            ConditionCheck.CheckCondition(validRoles.Contains(normalizedRole), Errors.StaffError.InvalidRole);

            // Use the original role constant for consistency
            return validRoles.First(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        }

        private static IQueryable<StaffProfileDTO> ApplyFilters(IQueryable<StaffProfileDTO> query, UrlQueryParameters parameters)
        {
            if (parameters.Filters?.Any() != true) return query;

            foreach (var (key, value) in parameters.Filters)
            {
                query = key.ToLowerInvariant() switch
                {
                    "role" => query.Where(p => p.Roles.Contains(value)),
                    _ => query
                };
            }

            return query;
        }

        private static IQueryable<StaffProfileDTO> ApplySearch(IQueryable<StaffProfileDTO> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SearchBy) || string.IsNullOrWhiteSpace(parameters.SearchValue))
                return query;

            var searchValue = parameters.SearchValue.ToLowerInvariant();

            return parameters.SearchBy.ToLowerInvariant() switch
            {
                "email" => query.Where(p => p.Email.ToLower().Contains(searchValue)),
                "first_name" => query.Where(p => p.FirstName.ToLower().Contains(searchValue)),
                "last_name" => query.Where(p => p.LastName.ToLower().Contains(searchValue)),
                "phone" => query.Where(p => p.PhoneNumber.ToLower().Contains(searchValue)),
                _ => query
            };
        }

        private static IQueryable<StaffProfileDTO> ApplySort(IQueryable<StaffProfileDTO> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy)) return query.OrderBy(p => p.Email);

            var isDescending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            return parameters.SortBy.ToLowerInvariant() switch
            {
                "email" => isDescending
                    ? query.OrderByDescending(p => p.Email)
                    : query.OrderBy(p => p.Email),
                "first_name" => isDescending
                    ? query.OrderByDescending(p => p.FirstName)
                    : query.OrderBy(p => p.LastName),
                "last_name" => isDescending
                    ? query.OrderByDescending(p => p.LastName)
                    : query.OrderBy(p => p.LastName),
                "phone" => isDescending
                    ? query.OrderByDescending(p => p.PhoneNumber)
                    : query.OrderBy(p => p.PhoneNumber),
                "role" => isDescending
                    ? query.OrderByDescending(p => p.Roles)
                    : query.OrderBy(p => p.Roles),
                _ => query.OrderBy(p => p.Email)
            };
        }
        #endregion
    }
}
