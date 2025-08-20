using FOCS.Application.Services;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponUsageController : FocsController
    {
        private readonly ICouponUsageService _couponUsageService;

        public CouponUsageController(ICouponUsageService couponUsageService)
        {
            _couponUsageService = couponUsageService;
        }

        [HttpPost("list")]
        public async Task<ActionResult<PagedResult<CouponUsageResponse>>> GetList(UrlQueryParameters urlQueryParameters, [FromHeader(Name = "StoreId")] string storeId)
        {
            var rs = await _couponUsageService.GetList(urlQueryParameters, StoreId);

            return rs != null ? Ok(rs) : NotFound();
        }
    }
}
