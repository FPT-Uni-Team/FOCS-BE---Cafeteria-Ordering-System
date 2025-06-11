using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Route("api/admin")]
    [ApiController]
    public class AdminCouponController : FocsController
    {
        private readonly IAdminCouponService _adminCouponService;

        public AdminCouponController(IAdminCouponService adminCouponService)
        {
            _adminCouponService = adminCouponService;
        }

        [HttpPost("coupon")]
        public async Task<IActionResult> CreateCoupon([FromBody] CouponAdminDTO dto, [FromQuery] string couponType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _adminCouponService.CreateCouponAsync(dto, couponType, UserId);
            return Ok(created);
        }

        [HttpPost("coupons")]
        public async Task<IActionResult> GetAllCoupons([FromBody] UrlQueryParameters query)
        {
            var pagedResult = await _adminCouponService.GetAllCouponsAsync(query, UserId);
            return Ok(pagedResult);
        }

        [HttpPut("coupon/{id}")]
        public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] CouponAdminDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _adminCouponService.UpdateCouponAsync(id, dto, UserId);
            if (!updated)
                return NotFound(AdminCoupon.UpdateNotFound);

            return Ok(AdminCoupon.UpdateOk);
        }

        [HttpDelete("coupon/{id}")]
        public async Task<IActionResult> DeleteCoupon(Guid id)
        {
            var deleted = await _adminCouponService.DeleteCouponAsync(id, UserId);
            if (!deleted)
                return NotFound(AdminCoupon.DeleteNotFound);

            return Ok(AdminCoupon.DeleteOk);
        }

        [HttpPost("coupon/{id}/track-usage")]
        public async Task<IActionResult> TrackCouponUsage(Guid id)
        {
            var result = await _adminCouponService.TrackCouponUsageAsync(id);
            if (result <= 0)
                return NotFound(AdminCoupon.TrackNotFound);

            return Ok(result);
        }

        [HttpPut("coupon/{id}/status")]
        public async Task<IActionResult> SetCouponStatus(Guid id, [FromQuery] bool isActive)
        {
            var result = await _adminCouponService.SetCouponStatusAsync(id, isActive, UserId);
            if (!result)
                return NotFound(AdminCoupon.CouponStatusNotFound);

            return Ok(string.Format(AdminCoupon.CouponStatusOk, isActive ? "enabled" : "disabled"));
        }

        [HttpPut("coupon/{storeId}/assign-promotion")]
        public async Task<IActionResult> AssignCouponToPromotion(List<Guid> couponIds, Guid promotionId, Guid storeId)
        {
            var result = await _adminCouponService.AssignCouponsToPromotionAsync(couponIds, promotionId, UserId, storeId);
            if (!result)
                return NotFound(AdminCoupon.CouponsToPromotionNotFound);

            return Ok(AdminCoupon.CouponsToPromotionOk);
        }

    }
}
