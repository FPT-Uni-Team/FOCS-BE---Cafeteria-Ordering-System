using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IStoreManagementService
    {
        Task<StoreAdminServiceDTO> CreateStoreAsync(StoreAdminServiceDTO dto, string userId);
        Task<PagedResult<StoreAdminServiceDTO>> GetAllStoresAsync(UrlQueryParameters query);
        Task<bool> UpdateStoreAsync(Guid id, StoreAdminServiceDTO dto, string userId);
        Task<bool> DeleteStoreAsync(Guid id, string userId);
    }
}
