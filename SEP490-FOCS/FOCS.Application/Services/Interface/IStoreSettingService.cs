using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IStoreSettingService
    {
        Task<StoreSettingDTO> GetStoreSettingAsync(Guid storeId, string userId);
        Task<bool> UpdateStoreSettingAsync(Guid storeId, StoreSettingDTO dto, string userId);
        Task<StoreSettingDTO> CreateStoreSettingAsync(Guid storeId, StoreSettingDTO dto, string userId);
    }
}
