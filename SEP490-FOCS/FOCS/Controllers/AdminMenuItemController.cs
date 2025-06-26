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
    [Route("api/admin")]
    [ApiController]
    public class AdminMenuItemController : FocsController
    {
        private readonly IAdminMenuItemService _adminMenuItemService;

        public AdminMenuItemController(IAdminMenuItemService menuService)
        {
            _adminMenuItemService = menuService;
        }

        [HttpPost("menu-item")]
        public async Task<IActionResult> CreateMenuItem([FromBody] MenuItemAdminDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _adminMenuItemService.CreateMenuAsync(dto, StoreId);

            return Ok(created);
        }

        [HttpPost("menu-items")]
        public async Task<IActionResult> GetAllMenuItems([FromBody] UrlQueryParameters urlQueryParameters)
        {
            var pagedResult = await _adminMenuItemService.GetAllMenuItemAsync(urlQueryParameters, Guid.Parse(StoreId));
            return Ok(pagedResult);
        }

        [HttpGet("menu-item/{menuItemId}")]
        public async Task<IActionResult> GetMenuItemDetail(Guid menuItemId)
        {
            var menuItems = await _adminMenuItemService.GetMenuItemDetail(menuItemId, StoreId);
            if (menuItems == null)
                return NotFound();

            return Ok(menuItems);
        }

        [HttpPost("menu-item-list")]
        public async Task<IActionResult> GetListMenuItem(List<Guid> menuItemIds)
        {
            var menuItems = await _adminMenuItemService.GetListMenuItemDetail(menuItemIds, StoreId, UserId);
            if (menuItems == null)
                return NotFound();

            return Ok(menuItems);
        }

        [HttpPut("menu-item/{id}")]
        public async Task<IActionResult> UpdateMenuItem(Guid id, [FromBody] MenuItemAdminDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _adminMenuItemService.UpdateMenuAsync(id, dto, UserId);
            if (!updated)
                return NotFound();

            return NoContent(); // 204
        }

        [HttpDelete("menu-item/{id}")]
        public async Task<IActionResult> DeleteMenuItem(Guid id)
        {
            var deleted = await _adminMenuItemService.DeleteMenuAsync(id, UserId);
            if (!deleted)
                return NotFound();

            return NoContent(); // 204
        }

    }
}
