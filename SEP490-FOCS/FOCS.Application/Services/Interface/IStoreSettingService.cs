using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IStoreSettingService
    {
        Task<StoreSettingDTO> GetStoreSettingAsync(Guid storeId);
        Task<bool> UpdateStoreSettingAsync(Guid storeId, StoreSettingDTO dto, string userId);
        Task<bool> ResetStoreSettingAsync(Guid storeId, string userId);
    }
}
