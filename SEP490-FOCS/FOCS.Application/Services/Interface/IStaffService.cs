using FOCS.Application.DTOs;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Constants;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IStaffService
    {
        Task<StaffProfileDTO> CreateStaffAsync(RegisterRequest request, string StoreId, string managerId);
        Task<PagedResult<StaffProfileDTO>> GetStaffListAsync(UrlQueryParameters query, string storeId);
        Task<StaffProfileDTO> GetStaffProfileAsync(string staffId, string managerId);
        Task<StaffProfileDTO> UpdateStaffProfileAsync(StaffProfileDTO dto, string staffId, string managerId);
        Task<bool> ChangeStaffPasswordAsync(ChangeStaffPasswordRequest request, string managerId);
        Task<bool> DeleteStaffAsync(string staffId, string managerId);
        Task<bool> AddStaffRoleAsync(string role, string staffId, string managerId);
        Task<bool> RemoveStaffRoleAsync(string role, string staffId, string managerId);
        Task<StaffProfileDTO> CreateManagerAsync(RegisterRequest request, string StoreId, string managerId);
        Task<PagedResult<StaffProfileDTO>> GetManagerListAsync(UrlQueryParameters query, string storeId);
        Task<bool> DeleteManagerAsync(string staffId, string managerId);
    }
}
