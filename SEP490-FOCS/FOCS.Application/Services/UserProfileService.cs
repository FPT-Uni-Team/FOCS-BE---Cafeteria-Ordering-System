using AutoMapper;
using FOCS.Application.DTOs;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
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
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public UserProfileService(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<UserProfileDTO> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound);

            return _mapper.Map<UserProfileDTO>(user);
        }

        public async Task<UserProfileDTO> UpdateUserProfileAsync(UserProfileDTO dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound);

            dto.Email = user.Email; 
            _mapper.Map(dto, user);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = userId;

            await _userManager.UpdateAsync(user);
            return _mapper.Map<UserProfileDTO>(user);
        }

        public async Task<bool> DeleteUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound);

            user.IsActive = false;
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = userId;

            await _userManager.UpdateAsync(user);
            return true;
        }
    }
}
