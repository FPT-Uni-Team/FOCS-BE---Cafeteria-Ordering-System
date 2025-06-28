using FOCS.Application.DTOs.AdminDTO;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IAdminCouponService
    {
        Task<CouponAdminDTO> CreateCouponAsync(CouponAdminDTO dto, string userId, string storeId);
        Task<bool> UpdateCouponAsync(Guid id, CouponAdminDTO dto, string updatedBy, string storeId);
        Task<bool> DeleteCouponAsync(Guid id, string deletedBy);
        Task<PagedResult<CouponAdminDTO>> GetAllCouponsAsync(UrlQueryParameters query, Guid storeId, string userId);
        Task<PagedResult<CouponAdminDTO>> GetAvailableCouponsAsync(UrlQueryParameters query, Guid storeId, string userId);
        Task<CouponAdminDTO> GetCouponByIdAsync(Guid couponId, string userId);
        Task<List<CouponAdminDTO>> GetCouponsByListIdAsync(List<Guid> couponIds, string storeId, string userId);
        Task<TrackCouponUsageDTO?> TrackCouponUsageAsync(Guid couponId);
        Task<bool> SetCouponStatusAsync(Guid couponId, bool isActive, string userId);
        Task<bool> AssignCouponsToPromotionAsync(List<Guid> couponIds, Guid promotionId, string userId, Guid storeId);
        Task SetCouponConditionAsync(Guid couponId, SetCouponConditionRequest setCouponConditionRequest);
    }
}
