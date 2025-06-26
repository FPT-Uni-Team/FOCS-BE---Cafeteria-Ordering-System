using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IPromotionService
    {
        Task<PromotionDTO> CreatePromotionAsync(PromotionDTO dto, Guid storeId, string userId);
        Task<PromotionDTO> GetPromotionAsync(Guid promotionId, string userId);
        Task<PagedResult<PromotionDTO>> GetPromotionsByStoreAsync(UrlQueryParameters query, Guid storeId, string userId);
        Task<bool> UpdatePromotionAsync(Guid promotionId, PromotionDTO dto, Guid storeId, string userId);
        Task<bool> ActivatePromotionAsync(Guid promotionId, string userId);
        Task<bool> DeactivatePromotionAsync(Guid promotionId, string userId);
        Task<bool> DeletePromotionAsync(Guid promotionId, string userId);

        Task IsValidPromotionCouponAsync(string couponCode, string userId, Guid storeId);
        Task<DiscountResultDTO> ApplyEligiblePromotions(DiscountResultDTO discountStrategyResult);
    }
}
