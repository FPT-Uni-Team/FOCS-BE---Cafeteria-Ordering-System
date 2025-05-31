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

        [HttpPost("menu")]
        public async Task<IActionResult> CreateMenu([FromBody] MenuItemAdminServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _adminService.CreateMenuAsync(dto, UserId);

            return Ok(created);
        }

        [HttpPost("menus")]
        public async Task<IActionResult> GetAllMenus([FromBody] UrlQueryParameters urlQueryParameters, [FromQuery] Guid storeId)
        {
            var pagedResult = await _adminService.GetAllMenuItemAsync(urlQueryParameters, storeId);
            return Ok(pagedResult);
        }

        [HttpGet("menu/{id}")]
        public async Task<IActionResult> GetMenuDetail(Guid id)
        {
            var menu = await _adminService.GetMenuDetailAsync(id);
            if (menu == null)
                return NotFound();

            return Ok(menu);
        }

        [HttpPut("menu/{id}")]
        public async Task<IActionResult> UpdateMenu(Guid id, [FromBody] MenuItemAdminServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _adminService.UpdateMenuAsync(id, dto, UserId);
            if (!updated)
                return NotFound();

            return NoContent(); // 204
        }

        [HttpDelete("menu/{id}")]
        public async Task<IActionResult> DeleteMenu(Guid id)
        {
            var deleted = await _adminService.DeleteMenuAsync(id, UserId);
            if (!deleted)
                return NotFound();

            return NoContent(); // 204
        }
    }
}
