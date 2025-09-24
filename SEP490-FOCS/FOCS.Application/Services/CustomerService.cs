using AutoMapper;
using CloudinaryDotNet.Core;
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
    public class CustomerService : ICustomerService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRepository<UserStore> _userStoreRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<Store> _storeRepository;

        public CustomerService(
            UserManager<User> userManager,
            IRepository<UserStore> userStoreRepository,
            IMapper mapper,
            IRepository<Store> storeRepository)
        {
            _userManager = userManager;
            _userStoreRepository = userStoreRepository;
            _mapper = mapper;
            _storeRepository = storeRepository;
        }

        public async Task<PagedResult<UserProfileDTO>> GetCustomerListAsync(UrlQueryParameters query, string storeId)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(storeId, out Guid storeIdGuid),
                                                    Errors.Common.InvalidGuidFormat,
                                                    Errors.FieldName.StoreId);
            var userStores = await _userStoreRepository.FindAsync(us => us.StoreId == storeIdGuid &&
                                                                       us.Status == Common.Enums.UserStoreStatus.Active);
            var userIds = userStores.Select(us => us.UserId.ToString()).ToList();

            var allUsers = _userManager.Users.Where(u => !u.IsDeleted && userIds.Contains(u.Id)).ToList();

            var Customer = new List<UserProfileDTO>();

            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, Roles.Staff) ||
                    await _userManager.IsInRoleAsync(user, Roles.KitchenStaff) ||
                    await _userManager.IsInRoleAsync(user, Roles.Manager) ||
                    await _userManager.IsInRoleAsync(user, Roles.Admin))
                {
                    continue;
                }
                var dto = _mapper.Map<UserProfileDTO>(user);
                Customer.Add(dto);
            }

            var CustomerQuery = Customer.AsQueryable();

            CustomerQuery = ApplyFilters(CustomerQuery, query);
            CustomerQuery = ApplySearch(CustomerQuery, query);
            CustomerQuery = ApplySort(CustomerQuery, query);

            var total = CustomerQuery.Count();
            var items = CustomerQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            return new PagedResult<UserProfileDTO>(items, total, query.Page, query.PageSize);
        }

        public async Task<UserProfileDTO> GetCustomerProfileAsync(string storeId, string customerId, string managerId)
        {
            var CustomerStoreId = await _userStoreRepository.AsQueryable()
                .Where(us => us.UserId.ToString().Equals(customerId) && us.StoreId.ToString().Equals(storeId))
                .FirstOrDefaultAsync();

            var managerStoreId = await _userStoreRepository.AsQueryable()
                .Where(us => us.UserId.ToString().Equals(managerId) && us.StoreId.ToString().Equals(storeId))
                .ToListAsync();

            ConditionCheck.CheckCondition(!CustomerStoreId.Equals(null) && !managerStoreId.Equals(null), Errors.AuthError.UserUnauthor);

            var customer = await _userManager.FindByIdAsync(customerId);
            var result = _mapper.Map<UserProfileDTO>(customer);
            return result;
        }

        public async Task<bool> BlockCustomerAsync(string storeId, string customerId, string managerId)
        {
            var CustomerStoreId = await _userStoreRepository.AsQueryable()
                .Where(us => us.UserId.ToString().Equals(customerId) && us.StoreId.ToString().Equals(storeId))
                .FirstOrDefaultAsync();

            var managerStoreId = await _userStoreRepository.AsQueryable()
                .Where(us => us.UserId.ToString().Equals(managerId) && us.StoreId.ToString().Equals(storeId))
                .ToListAsync();

            ConditionCheck.CheckCondition(!CustomerStoreId.Equals(null) && !managerStoreId.Equals(null), Errors.AuthError.UserUnauthor);

            CustomerStoreId.Status = Common.Enums.UserStoreStatus.Blocked;
            _userStoreRepository.Update(CustomerStoreId);
            await _userStoreRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnblockCustomerAsync(string storeId, string customerId, string managerId)
        {
            var CustomerStoreId = await _userStoreRepository.AsQueryable()
                .Where(us => us.UserId.ToString().Equals(customerId) && us.StoreId.ToString().Equals(storeId))
                .FirstOrDefaultAsync();

            var managerStoreId = await _userStoreRepository.AsQueryable()
                .Where(us => us.UserId.ToString().Equals(managerId) && us.StoreId.ToString().Equals(storeId))
                .ToListAsync();

            ConditionCheck.CheckCondition(!CustomerStoreId.Equals(null) && !managerStoreId.Equals(null), Errors.AuthError.UserUnauthor);

            CustomerStoreId.Status = Common.Enums.UserStoreStatus.Active;
            _userStoreRepository.Update(CustomerStoreId);
            await _userStoreRepository.SaveChangesAsync();

            return true;
        }

        #region Private Helper 

        private static IQueryable<UserProfileDTO> ApplyFilters(IQueryable<UserProfileDTO> query, UrlQueryParameters parameters)
        {
            if (parameters.Filters?.Any() != true) return query;

            foreach (var (key, value) in parameters.Filters)
            {
                query = key.ToLowerInvariant() switch
                {
                    _ => query
                };
            }

            return query;
        }

        private static IQueryable<UserProfileDTO> ApplySearch(IQueryable<UserProfileDTO> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SearchBy) || string.IsNullOrWhiteSpace(parameters.SearchValue))
                return query;

            var searchValue = parameters.SearchValue.ToLowerInvariant();

            return parameters.SearchBy.ToLowerInvariant() switch
            {
                //"email" => query.Where(p => p.Email.ToLower().Contains(searchValue)),
                "first_name" => query.Where(p => p.Firstname.ToLower().Contains(searchValue)),
                "last_name" => query.Where(p => p.Lastname.ToLower().Contains(searchValue)),
                "phone" => query.Where(p => p.PhoneNumber.ToLower().Contains(searchValue)),
                _ => query
            };
        }

        private static IQueryable<UserProfileDTO> ApplySort(IQueryable<UserProfileDTO> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy)) return query.OrderBy(p => p.Firstname);

            var isDescending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            return parameters.SortBy.ToLowerInvariant() switch
            {
                //"email" => isDescending
                //    ? query.OrderByDescending(p => p.Email)
                //    : query.OrderBy(p => p.Email),
                "first_name" => isDescending
                    ? query.OrderByDescending(p => p.Firstname)
                    : query.OrderBy(p => p.Firstname),
                "last_name" => isDescending
                    ? query.OrderByDescending(p => p.Lastname)
                    : query.OrderBy(p => p.Lastname),
                "phone" => isDescending
                    ? query.OrderByDescending(p => p.PhoneNumber)
                    : query.OrderBy(p => p.PhoneNumber),
                _ => query
            };
        }
        #endregion
    }
}