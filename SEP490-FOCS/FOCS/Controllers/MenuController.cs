using AutoMapper;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Order.Infrastucture.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Controllers
{
    [Route("api/menu")]
    [ApiController]
    public class MenuController : FocsController
    {
        private readonly IMenuService _menuService;
        private readonly IMenuItemsVariantGroupService _menuItemsVariantGroupService;
        private readonly IMapper _mapper;

        public MenuController(IMenuService menuService, IMapper mapper, IMenuItemsVariantGroupService menuItemsVariantGroupService)
        {
            _menuService = menuService;
            _mapper = mapper;
            _menuItemsVariantGroupService = menuItemsVariantGroupService;
        }

        [HttpPost("get-menu-item")]
        public async Task<PagedResult<MenuItemDTO>> GetMenuItemByStore([FromBody] UrlQueryParameters urlQueryParameters, [FromQuery] Guid storeId)
        { 
            return await _menuService.GetMenuItemByStore(urlQueryParameters, storeId);
        }

        [HttpPost("get-menu-item-detail")]
        public async Task<MenuItemDTO> GetItemVariant(Guid itemId)
        {
            return await _menuService.GetItemVariant(itemId);
        }

        [HttpGet("menu-items/{menuItemId}/variant-groups")]
        public async Task<IActionResult> GetVariantGroups(Guid menuItemId)
        {
            var result = await _menuItemsVariantGroupService.GetVariantGroupsWithVariants(menuItemId, Guid.Parse(StoreId));
            return Ok(result);
        }

    }
}
