using FOCS.Application.DTOs;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IUserProfileService
    {
        Task<UserProfileDTO> GetUserProfileAsync(string userId);
        Task<UserProfileDTO> UpdateUserProfileAsync(UserProfileDTO dto, string userId);
        Task<bool> DeleteUserProfileAsync(string userId);
    }
}
