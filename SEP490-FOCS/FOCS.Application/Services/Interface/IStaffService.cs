using FOCS.Application.DTOs;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Constants;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IStaffService
    {
        Task<bool> AddStaffRoleAsync(string role, string userId, string managerId);
        Task<bool> RemoveStaffRoleAsync(string role, string userId, string managerId);
    }
}
