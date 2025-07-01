using FOCS.Application.Services;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("MenuItemCategory")]
    [Authorize(Roles = Roles.Manager +","+ Roles.Admin)]
    [ApiController]
    public class MenuItemCategoryController : FocsController
    {
        private readonly IMenuItemCategoryService _menuItemCategoryService;

        public MenuItemCategoryController(IMenuItemCategoryService menuItemCategoryService)
        {
            _menuItemCategoryService = menuItemCategoryService;
        }

        [HttpPost("assign-to-category/{cateId}")]
        public async Task<bool> AssignMenuItemsToCategory(Guid cateId, [FromBody] List<Guid> menuItemIds)
        {
            return await _menuItemCategoryService.AssignMenuItemsToCategory(cateId, menuItemIds, Guid.Parse(StoreId));
        }

        [HttpGet("{cateId}")]
        public async Task<CategoryMenuItemDetailResponse> GetCategoryWithMenuItems(Guid cateId)
        {
            return await _menuItemCategoryService.GetCategoryWithMenuItemDetail(cateId, Guid.Parse(StoreId));
        }

        [HttpPost("menu-item/{menuItemId}/categories")]
        public async Task<List<MenuCategoryDTO>> ListCategoriesWithMenuItem(Guid menuItemId)
        {
            return await _menuItemCategoryService.ListCategoriesWithMenuItem(menuItemId, StoreId);
        }

        [HttpDelete("{cateId}")]
        public async Task<bool> RemoveMenuItemFromCategory(Guid cateId,[FromBody] List<Guid> menuItemIds)
        {
            return await _menuItemCategoryService.RemoveMenuItemFromCategory(cateId, menuItemIds);
        }

        [HttpPost("assign-to-menu-item/{menuItemId}")]
        public async Task AssignCategoriesToMenuItem([FromBody] List<Guid> categoryIds, Guid menuItemId)
        {
            await _menuItemCategoryService.AssignCategoriesToMenuItem(categoryIds, menuItemId, StoreId);
        }

        [HttpPost("list")]
        public async Task<PagedResult<CategoryMenuItemDetailResponse>> ListCategoriesWithMenuItems([FromBody] UrlQueryParameters urlQueryParameters)
        {
            return await _menuItemCategoryService.ListCategoriesWithMenuItems(urlQueryParameters, Guid.Parse(StoreId));
        }
    }
}
