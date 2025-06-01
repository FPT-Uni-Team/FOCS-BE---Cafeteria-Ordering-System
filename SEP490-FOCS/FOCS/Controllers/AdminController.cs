using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : FocsController
    {
        private readonly IMenuManagementService _adminService;

        public AdminController(IMenuManagementService menuService)
        {
            _adminService = menuService;
        }

        [HttpPost("menu-item")]
        public async Task<IActionResult> CreateMenuItem([FromBody] MenuItemAdminServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _adminService.CreateMenuAsync(dto, UserId);

            return Ok(created);
        }

        [HttpPost("menu-items")]
        public async Task<IActionResult> GetAllMenuItems([FromBody] UrlQueryParameters urlQueryParameters, [FromQuery] Guid storeId)
        {
            var pagedResult = await _adminService.GetAllMenuItemAsync(urlQueryParameters, storeId);
            return Ok(pagedResult);
        }

        [HttpGet("menu-item/{menuItemId}")]
        public async Task<IActionResult> GetMenuItemDetail(Guid menuItemId)
        {
            var menuItems = await _adminService.GetMenuItemDetail(menuItemId, StoreId);
            if (menuItems == null)
                return NotFound();

            return Ok(menuItems);
        }

        [HttpPut("menu-item/{id}")]
        public async Task<IActionResult> UpdateMenuItem(Guid id, [FromBody] MenuItemAdminServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _adminService.UpdateMenuAsync(id, dto, UserId);
            if (!updated)
                return NotFound();

            return NoContent(); // 204
        }

        [HttpDelete("menu-item/{id}")]
        public async Task<IActionResult> DeleteMenuItem(Guid id)
        {
            var deleted = await _adminService.DeleteMenuAsync(id, UserId);
            if (!deleted)
                return NotFound();

            return NoContent(); // 204
        }
    }
}
