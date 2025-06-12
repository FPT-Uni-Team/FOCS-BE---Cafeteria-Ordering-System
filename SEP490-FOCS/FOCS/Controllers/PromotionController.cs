using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
    [Route("api/admin/promotion")]
    [ApiController]
    public class PromotionController : FocsController
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet("{promotionId}")]
        public async Task<IActionResult> GetPromotion(Guid promotionId)
        {
            var result = await _promotionService.GetPromotionAsync(promotionId, UserId);
            return Ok(result);
        }

        [HttpPost("{storeId}")]
        public async Task<IActionResult> GetPromotions([FromBody] UrlQueryParameters query, Guid storeId)
        {
            var result = await _promotionService.GetPromotionsByStoreAsync(query, storeId, UserId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePromotion([FromBody] PromotionDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _promotionService.CreatePromotionAsync(dto, UserId);
            return Ok(created);
        }

        [HttpPut("{promotionId}")]
        public async Task<IActionResult> UpdatePromotion(Guid promotionId, [FromBody] PromotionDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _promotionService.UpdatePromotionAsync(promotionId, dto, UserId);
            return success ? Ok() : NotFound();
        }

        [HttpPatch("active/{promotionId}")]
        public async Task<IActionResult> ActivePromotion(Guid promotionId)
        {
            var success = await _promotionService.ActivatePromotionAsync(promotionId, UserId);
            return success ? Ok() : NotFound();
        }

        [HttpPatch("deactive/{promotionId}")]
        public async Task<IActionResult> DeactivePromotion(Guid promotionId)
        {
            var success = await _promotionService.DeactivatePromotionAsync(promotionId, UserId);
            return success ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromotion(Guid id)
        {
            var success = await _promotionService.DeletePromotionAsync(id, UserId);
            return success ? Ok() : NotFound();
        }
    }
}
