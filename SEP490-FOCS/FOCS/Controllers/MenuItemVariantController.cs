using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/menu-item-variant")]
    [ApiController]
    public class MenuItemVariantController : FocsController
    {
        private readonly IMenuItemVariantService _menuItemVariantService;

        public MenuItemVariantController(IMenuItemVariantService menuItemVariantService)
        {
            _menuItemVariantService = menuItemVariantService;
        }

        // POST: /menu-item-variant
        [HttpPost]
        public async Task<IActionResult> CreateMenuItemVariant([FromBody] MenuItemVariantDTO request, [FromHeader(Name = "StoreId")] Guid storeId)
        {
            var result = await _menuItemVariantService.CreateMenuItemVariant(request, storeId);
            return Ok(result);
        }

        // GET: /menu-item-variant
        [HttpGet]
        public async Task<IActionResult> ListVariants([FromQuery] UrlQueryParameters queryParameters, [FromHeader(Name = "StoreId")] Guid storeId)
        {
            var result = await _menuItemVariantService.ListVariants(queryParameters, storeId);
            return Ok(result);
        }

        // POST: /menu-item-variant/by-ids
        [HttpPost("by-ids")]
        public async Task<IActionResult> ListVariantsWithIds([FromBody] List<Guid> ids, [FromHeader(Name = "StoreId")] Guid storeId)
        {
            var result = await _menuItemVariantService.ListVariantsWithIds(ids, storeId);
            return Ok(result);
        }

        // DELETE: /menu-item-variant/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveMenuItemVariant(Guid id, [FromHeader(Name = "StoreId")] Guid storeId)
        {
            var result = await _menuItemVariantService.RemoveMenuItemVariant(id, storeId);
            return result ? Ok() : NotFound("Variant not found or already deleted.");
        }

        // PUT: /menu-item-variant/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuItemVariant(Guid id, [FromBody] MenuItemVariantDTO request, [FromHeader(Name = "StoreId")] Guid storeId)
        {
            var result = await _menuItemVariantService.UpdateMenuItemVariant(id, request, storeId);
            return result ? Ok() : BadRequest("Failed to update menu item variant.");
        }

        // GET: /menu-item-variant/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVariantDetail(Guid id, [FromHeader(Name = "StoreId")] Guid storeId)
        {
            var result = await _menuItemVariantService.GetVariantDetail(id, storeId);
            return result != null ? Ok(result) : NotFound("Variant not found.");
        }

        [HttpPost("assign-to-variant-group")]
        public async Task<bool> AssignVariantsToVariantGroup([FromBody] AssignVariantsToGroupRequest request)
        {
            return await _menuItemVariantService.AssignVariantGroupToVariants(request.VariantIds, request.VariantGroupId);
        }

        [HttpGet("list-by-store")]
        public async Task<List<VariantDTO>> ListVariantsByStore()
        {
            return await _menuItemVariantService.ListVariantByStore(StoreId);
        }
    }
}
