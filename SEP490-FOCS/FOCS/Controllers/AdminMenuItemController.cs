using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace FOCS.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
    [Route("api/admin/menu-item")]
    [ApiController]
    public class AdminMenuItemController : FocsController
    {
        private readonly IAdminMenuItemService _menuService;
        private readonly IMenuItemManagementService _menuManagementService;
        private readonly IMenuItemsVariantGroupService _menuItemsVariantGroupService;


        public AdminMenuItemController(
            IAdminMenuItemService menuService,
            IMenuItemsVariantGroupService menuItemsVariantGroupService,
            IMenuItemManagementService imageService)
        {
            _menuService = menuService;
            _menuManagementService = imageService;
            _menuItemsVariantGroupService = menuItemsVariantGroupService;
        }

        #region CRUD MENU ITEM

        [HttpPost("without-variant")]
        public async Task<IActionResult> Create([FromBody] MenuItemAdminDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _menuService.CreateMenuAsync(dto, StoreId);
            return Ok(created);
        }

        [HttpPost]
        public async Task<bool> Create([FromBody] CreateMenuItemWithVariantRequest createMenuItemWithVariantRequest)
        {
            return await _menuManagementService.CreateNewMenuItemWithVariant(createMenuItemWithVariantRequest);
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList([FromBody] UrlQueryParameters query)
        {
            var result = await _menuService.GetAllMenuItemAsync(query, Guid.Parse(StoreId));
            return Ok(result);
        }

        [HttpGet("{menuItemId}")]
        public async Task<IActionResult> GetDetail(Guid menuItemId)
        {
            var item = await _menuService.GetMenuItemDetail(menuItemId, StoreId);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> GetByIds([FromBody] List<Guid> ids)
        {
            var items = await _menuService.GetListMenuItemDetail(ids, StoreId, UserId);
            return items == null ? NotFound() : Ok(items);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MenuItemAdminDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _menuService.UpdateMenuAsync(id, dto, UserId);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _menuService.DeleteMenuAsync(id, UserId);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("{id}/variant-groups")]
        public async Task<IActionResult> GetVariantGroups(Guid id)
        {
            var result = await _menuItemsVariantGroupService.GetVariantGroupsWithVariants(id, Guid.Parse(StoreId));
            return Ok(result);
        }

        [HttpPost("{id}/variant-group/variants")]
        public async Task<bool> AddVariantGroupAndVariantForProduct([FromBody] AddVariantGroupAndVariant request, Guid id, [FromHeader(Name = "StoreId")] string storeId)
        {
            return await _menuManagementService.AddVariantGroupAndVariant(request, id, storeId);
        }

        [HttpDelete("{id}/product-variants")]
        public async Task<bool> RemoveProductVariant(RemoveProductVariantFromProduct request, Guid id, [FromHeader(Name = "StoreId")] string storeId)
        {
            return await _menuManagementService.RemoveVariantGroupAndVariantFromProduct(request, id, storeId);
        }

        [HttpDelete("{id}/variant-groups")]
        public async Task<bool> RemoveVariantGroups(RemoveVariantGroupFromProduct request, Guid id, [FromHeader(Name = "StoreId")] string storeId)
        {
            return await _menuManagementService.RemoveVariantGroupsFromProduct(request, id, storeId);
        }

        #endregion

        #region IMAGE MANAGEMENT

        [HttpGet("{menuItemId}/images")]
        public async Task<IActionResult> GetImages(Guid menuItemId)
        {
            var images = await _menuManagementService.GetImagesOfProduct(menuItemId, StoreId);
            return Ok(images);
        }

        [HttpPost("{menuItemId}/images/upload")]
        public async Task<IActionResult> UploadImages(
            Guid menuItemId,
            [FromForm] List<IFormFile> files,
            [FromForm] List<bool> isMain)
        {
            var success = await _menuManagementService.UploadImagesAsync(files, isMain, menuItemId.ToString(), StoreId);
            return Ok(success);
        }

        [HttpPut("images/update")]
        public async Task<IActionResult> UpdateImages(
            [FromForm] List<string> urls,
            [FromForm] List<IFormFile> files,
            [FromForm] List<bool> isMain)
        {
            var success = await _menuManagementService.UpdateImagesAsync(urls, files, isMain, StoreId);
            return Ok(success);
        }

        [HttpDelete("images/delete")]
        public async Task<IActionResult> DeleteImages([FromBody] List<string> urls)
        {
            var success = await _menuManagementService.RemoveImageAsync(urls);
            return Ok(success);
        }

        #endregion
    }
}
