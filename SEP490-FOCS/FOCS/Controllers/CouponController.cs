using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
    [Route("api/admin")]
    [ApiController]
    public class CouponController : FocsController
    {
        private readonly IAdminCouponService _adminCouponService;

        public CouponController(IAdminCouponService adminCouponService)
        {
            _adminCouponService = adminCouponService;
        }

        [Authorize]
        [HttpGet("debug-token")]
        public IActionResult Debug()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("coupon")]
        public async Task<IActionResult> CreateCoupon([FromBody] CouponAdminDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var created = await _adminCouponService.CreateCouponAsync(dto, UserId, StoreId);
            return Ok(created); 
        }

        [HttpPost("{couponId}/set-condition")]
        public async Task<IActionResult> SetConditionForCoupon(Guid couponId, [FromBody] SetCouponConditionRequest setCouponConditionRequest)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            await _adminCouponService.SetCouponConditionAsync(couponId, setCouponConditionRequest);

            return Ok();
        }

        [HttpPost("coupons/{storeId}")]
        public async Task<IActionResult> GetAllCoupons([FromBody] UrlQueryParameters query, Guid storeId)
        {
            var pagedResult = await _adminCouponService.GetAllCouponsAsync(query, storeId, UserId);
            return Ok(pagedResult);
        }

        [HttpPost("coupons/available")]
        public async Task<IActionResult> GetAvailableCoupons([FromBody] UrlQueryParameters query)
        {

            ConditionCheck.CheckCondition(Guid.TryParse(StoreId, out Guid storeIdGuid),
                                                    Errors.Common.InvalidGuidFormat,
                                                    Errors.FieldName.StoreId);
            var pagedResult = await _adminCouponService.GetAvailableCouponsAsync(query, storeIdGuid, UserId);
            return Ok(pagedResult);
        }

        [HttpGet("coupon/{id}")]
        public async Task<IActionResult> GetCoupon(Guid id)
        {
            var coupon = await _adminCouponService.GetCouponByIdAsync(id, UserId);
            if (coupon == null)
                return NotFound();

            return Ok(coupon);
        }

        [HttpPost("coupons/by-ids")]
        public async Task<IActionResult> GetCouponsByListId([FromBody] List<Guid> couponIds)
        {
            if (couponIds == null || !couponIds.Any())
                return NotFound();
            var result = await _adminCouponService.GetCouponsByListIdAsync(couponIds, StoreId, UserId);

            return Ok(result);
        }

        [HttpPut("coupon/{id}")]
        public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] CouponAdminDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var updated = await _adminCouponService.UpdateCouponAsync(id, dto, UserId, StoreId);
            if (!updated)
                return NotFound(AdminCouponConstants.UpdateNotFound);

            return Ok(AdminCouponConstants.UpdateOk);
        }

        [HttpDelete("coupon/{id}")]
        public async Task<IActionResult> DeleteCoupon(Guid id)
        {
            var deleted = await _adminCouponService.DeleteCouponAsync(id, UserId);
            if (!deleted)
                return NotFound(AdminCouponConstants.DeleteNotFound);

            return Ok(AdminCouponConstants.DeleteOk);
        }

        [HttpPost("coupon/{id}/track-usage")]
        public async Task<IActionResult> TrackCouponUsage(Guid id)
        {
            var result = await _adminCouponService.TrackCouponUsageAsync(id);
            if (result == null)
                return NotFound(AdminCouponConstants.TrackNotFound);

            return Ok(result);
        }

        [HttpPut("coupon/{id}/status")]
        public async Task<IActionResult> SetCouponStatus(Guid id, [FromQuery] bool isActive)
        {
            var result = await _adminCouponService.SetCouponStatusAsync(id, isActive, UserId);
            if (!result)
                return NotFound(AdminCouponConstants.CouponStatusNotFound);

            return Ok(string.Format(AdminCouponConstants.CouponStatusOk, isActive ? "enabled" : "disabled"));
        }

        [HttpPut("coupon/{storeId}/assign-promotion")]
        public async Task<IActionResult> AssignCouponToPromotion(Guid storeId, [FromBody] AssignCouponRequest request)
        {
            var result = await _adminCouponService.AssignCouponsToPromotionAsync(
                request.CouponIds, request.PromotionId, UserId, storeId);

            if (!result)
                return NotFound(AdminCouponConstants.CouponsToPromotionNotFound);

            return Ok(AdminCouponConstants.CouponsToPromotionOk);
        }
    }
}
