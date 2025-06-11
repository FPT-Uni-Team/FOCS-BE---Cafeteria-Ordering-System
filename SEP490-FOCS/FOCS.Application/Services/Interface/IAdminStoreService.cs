using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IAdminStoreService
    {
        Task<StoreAdminDTO> CreateStoreAsync(StoreAdminDTO dto, string userId);
        Task<PagedResult<StoreAdminDTO>> GetAllStoresAsync(UrlQueryParameters query, string userId);
        Task<bool> UpdateStoreAsync(Guid id, StoreAdminDTO dto, string userId);
        Task<bool> DeleteStoreAsync(Guid id, string userId);
    }
}
