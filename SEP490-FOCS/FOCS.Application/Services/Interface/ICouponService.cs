using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface ICouponService
    {
        Task IsValidApplyCouponAsync(string couponCode, Guid storeId);
        Task<PagedResult<CouponAdminDTO>> GetOngoingCouponsAsync(UrlQueryParameters query, string storeId);
    }
}
