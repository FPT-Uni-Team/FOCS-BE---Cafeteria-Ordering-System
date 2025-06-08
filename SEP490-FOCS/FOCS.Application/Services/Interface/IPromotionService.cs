using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IPromotionService
    {
        Task<PromotionDTO> CreatePromotionAsync(PromotionDTO dto, string userId);
        Task<PromotionDTO> GetPromotionAsync(Guid promotionId);
        Task<PagedResult<PromotionDTO>> GetPromotionsByStoreAsync(UrlQueryParameters query, Guid storeId);
        Task<bool> UpdatePromotionAsync(Guid id, PromotionDTO dto, string userId);
        Task<bool> ActivePromotionAsync(Guid id, string userId);
        Task<bool> InactivePromotionAsync(Guid id, string userId);
        Task<bool> DeletePromotionAsync(Guid id, string userId);
        Task IsValidPromotionCouponAsync(string couponCode, string storeId);
    }
}
