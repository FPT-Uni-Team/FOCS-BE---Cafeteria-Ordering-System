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
        private readonly IMapper _mapper;

        public MenuController(IMenuService menuService, IMapper mapper)
        {
            _menuService = menuService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<PagedResult<MenuItemDTO>> GetMenuItemByStore([FromBody] UrlQueryParameters urlQueryParameters, [FromQuery] Guid storeId)
        { 
            return await _menuService.GetMenuItemByStore(urlQueryParameters, storeId);
        }

        [HttpPost("{itemId}")]
        public async Task<MenuItemDTO> GetItemVariant(Guid itemId)
        {
            return await _menuService.GetItemVariant(itemId);
        }
    }
}
