using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
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
        public async Task<IActionResult> CreateCoupon([FromBody] CouponAdminServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _adminCouponService.CreateCouponAsync(dto, UserId);
            return Ok(created);
        }

        [HttpPost("coupons")]
        public async Task<IActionResult> GetAllCoupons([FromBody] UrlQueryParameters query)
        {
            var pagedResult = await _adminCouponService.GetAllCouponsAsync(query, StoreId);
            return Ok(pagedResult);
        }

        [HttpPut("coupon/{id}")]
        public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] CouponAdminServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _adminCouponService.UpdateCouponAsync(id, dto, UserId);
            if (!updated)
                return NotFound();

            return Ok();
        }

        [HttpDelete("coupon/{id}")]
        public async Task<IActionResult> DeleteCoupon(Guid id)
        {
            var deleted = await _adminCouponService.DeleteCouponAsync(id, UserId);
            if (!deleted)
                return NotFound();

            return Ok();
        }

        [HttpPost("coupon/{id}/track-usage")]
        public async Task<IActionResult> TrackCouponUsage(Guid id)
        {
            var result = await _adminCouponService.TrackCouponUsageAsync(id);
            if (result <= 0)
                return NotFound("Coupon cannot be used anymore.");

            return Ok(result);
        }

        [HttpPut("coupon/{id}/status")]
        public async Task<IActionResult> SetCouponStatus(Guid id, [FromQuery] bool isActive)
        {
            // Lấy userId từ token hoặc context
            var userId = User?.Identity?.Name ?? "system";

            var result = await _adminCouponService.SetCouponStatusAsync(id, isActive, userId);
            if (!result)
                return NotFound("Coupon not found or has been deleted.");

            return Ok($"Coupon has been {(isActive ? "enabled" : "disabled")} successfully.");
        }

        [HttpPut("coupon/{id}/assign-promotion")]
        public async Task<IActionResult> AssignCouponToPromotion(Guid id, [FromQuery] Guid promotionId)
        {
            var userId = User?.Identity?.Name ?? "system";

            var result = await _adminCouponService.AssignCouponToPromotionAsync(id, promotionId, userId);
            if (!result)
                return NotFound();

            return Ok("Coupon assigned to promotion successfully.");
        }

    }
}
