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
        private readonly IMenuManagementService _adminMenuItemService;
        private readonly IBrandManagementService _adminBrandService;
        private readonly IStoreManagementService _adminStoreService;

        public AdminController(IMenuManagementService menuService, IBrandManagementService adminBrand, IStoreManagementService storeService)
        {
            _adminMenuItemService = menuService;
            _adminBrandService = adminBrand;
            _adminStoreService = storeService;
        }

        [HttpPost("menu-item")]
        public async Task<IActionResult> CreateMenuItem([FromBody] MenuItemAdminServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _adminMenuItemService.CreateMenuAsync(dto, UserId);

            return Ok(created);
        }

        [HttpPost("menu-items")]
        public async Task<IActionResult> GetAllMenuItems([FromBody] UrlQueryParameters urlQueryParameters, [FromQuery] Guid storeId)
        {
            var pagedResult = await _adminMenuItemService.GetAllMenuItemAsync(urlQueryParameters, storeId);
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

        [HttpPut("menu-item/{id}")]
        public async Task<IActionResult> UpdateMenuItem(Guid id, [FromBody] MenuItemAdminServiceDTO dto)
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

        [HttpPost("brand")]
        public async Task<IActionResult> CreateBrand([FromBody] BrandAdminServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _adminBrandService.CreateBrandAsync(dto, UserId);
            return Ok(created);
        }

        [HttpPost("brands")]
        public async Task<IActionResult> GetAllBrands([FromBody] UrlQueryParameters query)
        {
            var pagedResult = await _adminBrandService.GetAllBrandsAsync(query);
            return Ok(pagedResult);
        }

        [HttpPut("brand/{id}")]
        public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] BrandAdminServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _adminBrandService.UpdateBrandAsync(id, dto, UserId);
            if (!updated)
                return NotFound();

            return Ok();
        }

        [HttpDelete("brand/{id}")]
        public async Task<IActionResult> DeleteBrand(Guid id)
        {
            var deleted = await _adminBrandService.DeleteBrandAsync(id, UserId);
            if (!deleted)
                return NotFound();

            return Ok();
        }

        [HttpPost("store")]
        public async Task<IActionResult> CreateStore([FromBody] StoreAdminServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _adminStoreService.CreateStoreAsync(dto, UserId);
            return Ok(created);
        }

        [HttpPost("stores")]
        public async Task<IActionResult> GetStores([FromBody] UrlQueryParameters query)
        {
            var result = await _adminStoreService.GetAllStoresAsync(query);
            return Ok(result);
        }

        [HttpPut("store/{id}")]
        public async Task<IActionResult> UpdateStore(Guid id, [FromBody] StoreAdminServiceDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _adminStoreService.UpdateStoreAsync(id, dto, UserId);
            return success ? Ok() : NotFound();
        }

        [HttpDelete("store/{id}")]
        public async Task<IActionResult> DeleteStore(Guid id)
        {
            var success = await _adminStoreService.DeleteStoreAsync(id, UserId);
            return success ? Ok() : NotFound();
        }
    }
}
