using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;
using FOCS.Common.Models.Payment;

namespace FOCS.Application.Services.Interface
{
    public interface IAdminStoreService
    {
        Task<StoreAdminDTO> CreateStoreAsync(StoreAdminDTO dto, string userId);
        Task<PagedResult<StoreAdminDTO>> GetAllStoresAsync(UrlQueryParameters query, string userId);
        Task<StoreAdminResponse> GetStoreSetting(Guid id); 
        Task<bool> UpdateStoreAsync(Guid id, StoreAdminDTO dto, string userId);
        Task<bool> DeleteStoreAsync(Guid id, string userId);
        Task<bool> UpdateConfigPayment(UpdateConfigPaymentRequest request, string storeId);
        Task<bool> CreatePaymentAsync(CreatePaymentRequest request, string storeId);
    }
}
