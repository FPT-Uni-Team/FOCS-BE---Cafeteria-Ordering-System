using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities;

namespace FOCS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly IMenuItemManagementService _menuManagementService;

        public MenuItemController(IMenuItemManagementService menuService)
        {
            _menuManagementService = menuService;
        }

        [HttpPost]
        public async Task<bool> CreateNewMenuItem([FromBody] CreateMenuItemWithVariantRequest createMenuItemWithVariantRequest)
        {
            return await _menuManagementService.CreateNewMenuItemWithVariant(createMenuItemWithVariantRequest);
        }
    }
}
