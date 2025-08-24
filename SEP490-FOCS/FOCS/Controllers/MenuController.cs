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
    [Route("api/me/menu-item")]
    [ApiController]
    public class MenuController : FocsController
    {
        private readonly IMenuService _menuService;
        private readonly IMenuInsightService _menuInsightService;
        private readonly IMapper _mapper;

        public MenuController(IMenuService menuService, IMapper mapper, IMenuInsightService menuInsightService)
        {
            _menuService = menuService;
            _mapper = mapper;
            _menuInsightService = menuInsightService;
        }

        [HttpPost]
        public async Task<PagedResult<MenuItemDTO>> GetMenuItemByStore([FromBody] UrlQueryParameters urlQueryParameters, [FromHeader] Guid storeId)
        { 
            return await _menuService.GetMenuItemByStore(urlQueryParameters, storeId);
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
