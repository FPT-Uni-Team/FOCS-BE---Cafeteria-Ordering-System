using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IAdminCouponService
    {
        Task<CouponAdminServiceDTO> CreateCouponAsync(CouponAdminServiceDTO dto, string createdBy);
        Task<bool> UpdateCouponAsync(Guid id, CouponAdminServiceDTO dto, string updatedBy);
        Task<bool> DeleteCouponAsync(Guid id, string deletedBy);
        Task<PagedResult<CouponAdminServiceDTO>> GetAllCouponsAsync(UrlQueryParameters query, string storeId);
        Task<int> TrackCouponUsageAsync(Guid couponId);
        Task<bool> SetCouponStatusAsync(Guid couponId, bool isActive, string userId);
        Task<bool> AssignCouponToPromotionAsync(Guid couponId, Guid promotionId, string userId);
    }
}
