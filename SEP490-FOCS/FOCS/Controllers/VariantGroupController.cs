using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/variant-group")]
    [ApiController]
    public class VariantGroupController : FocsController
    {
        private readonly IVariantGroupService _variantGroupService;

        public VariantGroupController(IVariantGroupService variantGroupService)
        {
            _variantGroupService = variantGroupService;
        }

        // GET: /variant-group/menu-item/{menuItemId}
        [HttpGet("menu-item/{menuItemId}")]
        public async Task<IActionResult> GetVariantGroupsByMenuItemAsync(Guid menuItemId)
        {
            var result = await _variantGroupService.GetVariantGroupsByMenuItemAsync(menuItemId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<bool> CreateNewVariantGroup(CreateVariantGroupRequest request)
        {
            return await _variantGroupService.CreateVariantGroup(request, StoreId);
        }

        // POST: /variant-group
        [HttpPost("assign")]
        public async Task<IActionResult> AddMenuItemVariantToGroupAsync([FromBody] AddVariantToGroupRequest request, [FromHeader(Name = "StoreId")] Guid storeId)
        {
            var result = await _variantGroupService.AddMenuItemVariantToGroupAsync(request, storeId);
            return result ? Ok() : BadRequest("Failed to add variant to group.");
        }

        // DELETE: /variant-group/{variantGroupId}
        [HttpDelete("{variantGroupId}")]
        public async Task<IActionResult> RemoveVariantFromGroupAsync(Guid variantGroupId)
        {
            var result = await _variantGroupService.RemoveVariantFromGroupAsync(variantGroupId);
            return result ? Ok() : NotFound("Variant group not found.");
        }

        // PUT: /variant-group/{menuItemId}/group-name/{groupName}
        [HttpPut("{menuItemId}/group-name/{groupName}")]
        public async Task<IActionResult> UpdateGroupSettingsAsync(Guid menuItemId, string groupName, [FromBody] UpdateGroupSettingRequest request)
        {
            var result = await _variantGroupService.UpdateGroupSettingsAsync(menuItemId, groupName, request);
            return result ? Ok() : BadRequest("Failed to update group settings.");
        }

        // GET: /variant-group/menu-item/{menuItemId}/names
        [HttpGet("menu-item/{menuItemId}/names")]
        public async Task<IActionResult> GetGroupNamesByMenuItemAsync(Guid menuItemId)
        {
            var result = await _variantGroupService.GetGroupNamesByMenuItemAsync(menuItemId);
            return Ok(result);
        }

        [HttpGet("variants")]
        public async Task<List<VariantGroupDetailDTO>> GetVariantGroupByStore(UrlQueryParameters urlQueryParameters)
        {
            return await _variantGroupService.GetVariantGroupsByStore(urlQueryParameters, StoreId);
        }
    }
}
