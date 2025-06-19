using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FOCS.Application.Services
{
    public class StaffService : IStaffService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRepository<UserStore> _userStoreRepository;

        public StaffService(UserManager<User> userManager, IRepository<UserStore> userStoreRepository)
        {
            _userManager = userManager;
            _userStoreRepository = userStoreRepository;
        }

        public async Task<bool> AddStaffRoleAsync(string role, string userId, string managerId)
        {
            var user = await ValidatePermissionAsync(userId, managerId);
            var roleToAssign = GetValidRoleAsync(role);
            await _userManager.AddToRoleAsync(user, roleToAssign);

            return true;
        }


        private async Task<User> ValidatePermissionAsync(string userId, string managerId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var manager = await _userManager.FindByIdAsync(managerId);

            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound);
            ConditionCheck.CheckCondition(manager != null, Errors.AuthError.UserUnauthor);

            var roles = await _userManager.GetRolesAsync(user);
            ConditionCheck.CheckCondition(roles.Contains(Roles.User), Errors.AuthError.UserUnauthor);
            await ValidateStoreAuthorizationAsync(userId, managerId);

            return user;
        }

        private async Task ValidateStoreAuthorizationAsync(string userId, string managerId)
        {
            var userStoreId = await _userStoreRepository.AsQueryable()
                .Where(us => us.UserId.ToString().Equals(userId))
                .FirstOrDefaultAsync();

            var managerStoreId = await _userStoreRepository.AsQueryable()
                .Where(us => us.UserId.ToString().Equals(managerId))
                .FirstOrDefaultAsync();

            ConditionCheck.CheckCondition(
                !userStoreId.Equals(null) && !managerStoreId.Equals(null) && userStoreId.StoreId.Equals(managerStoreId.StoreId),
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

        public async Task<bool> RemoveStaffRoleAsync(string role, string userId, string managerId)
        {
            var user = await ValidatePermissionAsync(userId, managerId);

            var roleToRemove = GetValidRoleAsync(role);
            await _userManager.RemoveFromRoleAsync(user, roleToRemove);
            return true;
        }
    }
}
