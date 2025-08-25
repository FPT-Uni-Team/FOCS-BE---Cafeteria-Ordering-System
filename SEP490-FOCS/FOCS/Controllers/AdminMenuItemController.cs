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
        public async Task<Guid> Create([FromBody] CreateMenuItemWithVariantRequest createMenuItemWithVariantRequest)
        {
            return await _menuManagementService.CreateNewMenuItemWithVariant(createMenuItemWithVariantRequest);
        }

        [HttpPost("list")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager + "," + Roles.User + "," + Roles.Staff + "," + Roles.KitchenStaff)]
        public async Task<IActionResult> GetList([FromBody] UrlQueryParameters query)
        {
            var result = await _menuService.GetAllMenuItemAsync(query, Guid.Parse(StoreId));
            return Ok(result);
        }

        [HttpGet("{menuItemId}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager + "," + Roles.User + "," + Roles.Staff + "," + Roles.KitchenStaff)]
        public async Task<IActionResult> GetDetail(Guid menuItemId)
        {
            var item = await _menuService.GetMenuItemDetail(menuItemId, StoreId);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost("bulk")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager + "," + Roles.User + "," + Roles.Staff + "," + Roles.KitchenStaff)]
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

        [HttpPut("active/{menuItemId}")]
        public async Task<bool> ActivateMenuItemAsync(Guid menuItemId)
        {
            return await _menuService.ActivateMenuItemAsync(menuItemId, UserId);
        }

        [HttpPut("deactive/{menuItemId}")]
        public async Task<bool> DeactivateMenuItemAsync(Guid menuItemId)
        {
            return await _menuService.DeactivateMenuItemAsync(menuItemId, UserId);
        }

        [HttpPut("enable/{menuItemId}")]
        public async Task<bool> EnableMenuItemAsync(Guid menuItemId)
        {
            return await _menuService.EnableMenuItemAsync(menuItemId, UserId);
        }

        [HttpPut("disable/{menuItemId}")]
        public async Task<bool> DisableMenuItemAsync(Guid menuItemId)
        {
            return await _menuService.DisableMenuItemAsync(menuItemId, UserId);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _menuService.DeleteMenuAsync(id, UserId);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("{id}/variant-groups")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager + "," + Roles.User + "," + Roles.Staff + "," + Roles.KitchenStaff)]
        public async Task<IActionResult> GetVariantGroups(Guid id)
        {
            var result = await _menuItemsVariantGroupService.GetVariantGroupsWithVariants(id, Guid.Parse(StoreId));
            return Ok(result);
        }

        [HttpPost("{id}/variant-group/variants")]
        public async Task<bool> AddVariantGroupAndVariantForProduct([FromBody] AddVariantGroupsAndVariants request, Guid id, [FromHeader(Name = "StoreId")] string storeId)
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
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager + "," + Roles.User + "," + Roles.Staff + "," + Roles.KitchenStaff)]
        public async Task<IActionResult> GetImages(Guid menuItemId)
        {
            var images = await _menuManagementService.GetImagesOfProduct(menuItemId, StoreId);
            return Ok(images);
        }

        [HttpPost("sync-images")]
        public async Task<IActionResult> SyncMenuItemImages([FromForm] List<IFormFile> files,
                                                            [FromForm] string metadata, // JSON string
                                                            [FromForm] Guid menuItemId,
                                                            [FromHeader(Name = "StoreId")] string storeId)
        {
            var rs = await _menuManagementService.SyncMenuItemImages(files, metadata, menuItemId, storeId);
            return Ok(rs);
        }
        #endregion
    }
}
