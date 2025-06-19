using FOCS.Application.DTOs;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Authorize(Roles = Roles.User + "," + Roles.Admin + "," + Roles.Manager)]
    [Route("user")]
    [ApiController]
    public class UserProfileController : FocsController
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IAuthService _authService;

        public UserProfileController(IUserProfileService userProfileService, IAuthService authService)
        {
            _userProfileService = userProfileService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<UserProfileDTO> GetUserProfileAsync()
        {
            return await _userProfileService.GetUserProfileAsync(UserId);
        }

        [HttpPut]
        public async Task<UserProfileDTO> UpdateUserProfileAsync(UserProfileDTO dto)
        {
            return await _userProfileService.UpdateUserProfileAsync(dto, UserId);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUserProfileAsync()
        {
            var isDeleted = await _userProfileService.DeleteUserProfileAsync(UserId);
            if (!isDeleted)
            {
                return NotFound();
            }
            await _authService.LogoutAsync(UserId);
            return Ok();
        }
    }
}
