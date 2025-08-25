using AutoMapper;
using FOCS.Application.Services;
using FOCS.Application.Services.Interface;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Order.Infrastucture.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Controllers
{
    [Route("api/me/menu-item")]
    [ApiController]
    public class MenuController : FocsController
    {
        private readonly IMenuService _menuService;
        private readonly IAdminMenuItemService _adminMenuService;
        private readonly IMenuInsightService _menuInsightService;
        private readonly IMenuItemsVariantGroupService _menuItemsVariantGroupService;
        private readonly IMenuItemCategoryService _menuItemCategoryService;
        private readonly IMapper _mapper;

        public MenuController(IMenuService menuService, IMapper mapper, 
            IMenuInsightService menuInsightService,
            IAdminMenuItemService adminMenuItemService, 
            IMenuItemCategoryService menuItemCategoryService, 
            IMenuItemsVariantGroupService menuItemsVariantGroupService)
        {
            _menuService = menuService;
            _mapper = mapper;
            _menuInsightService = menuInsightService;
            _adminMenuService = adminMenuItemService;
            _menuItemCategoryService = menuItemCategoryService;
            _menuItemsVariantGroupService = menuItemsVariantGroupService;
        }

        [HttpPost]
        public async Task<PagedResult<MenuItemDTO>> GetMenuItemByStore([FromBody] UrlQueryParameters urlQueryParameters, [FromHeader] Guid storeId)
        { 
            return await _menuService.GetMenuItemByStore(urlQueryParameters, storeId);
        }

        [HttpGet("{menuItemId}")]
        public async Task<IActionResult> GetDetail(Guid menuItemId)
        {
            var item = await _adminMenuService.GetMenuItemDetail(menuItemId, StoreId);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet("{id}/variant-groups")]
        public async Task<IActionResult> GetVariantGroups(Guid id)
        {
            var result = await _menuItemsVariantGroupService.GetVariantGroupsWithVariants(id, Guid.Parse(StoreId));
            return Ok(result);
        }

        [HttpPost("{menuItemId}/categories")]
        public async Task<List<MenuCategoryDTO>> ListCategoriesWithMenuItem(Guid menuItemId)
        {
            return await _menuItemCategoryService.ListCategoriesWithMenuItem(menuItemId, StoreId);
        }

        [HttpPost("ids")]
        public async Task<List<MenuItemDTO>> GetMenuItemByStore(List<Guid> ids, [FromHeader] Guid storeId)
        {
            return await _menuService.GetMenuItemByIds(ids, storeId);
        }

        [HttpPost("most-order")]
        public async Task<List<MenuItemInsightResponse>> GetProductsMostOrder()
        {
            return await _menuInsightService.GetMostOrderedProductsAsync(TimeSpan.FromDays(7), StoreId);
        }

        [HttpPost("based-on-history")]
        public async Task<List<MenuItemInsightResponse>> GetProductsBasedOnHistory([FromHeader] string actorId)
        {
            return await _menuInsightService.GetProductOrderNearingWithCurrentOfUser(Guid.Parse(UserId ?? actorId), 5);
        }

        [HttpPost("{itemId}")]
        public async Task<MenuItemDTO> GetItemVariant(Guid itemId)
        {
            return await _menuService.GetItemVariant(itemId);
        }
    }
}
