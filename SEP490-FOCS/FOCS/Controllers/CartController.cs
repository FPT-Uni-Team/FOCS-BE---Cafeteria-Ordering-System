using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Models.CartModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class CartController : FocsController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("tables/{tableId}/cart/{actorId?}")]
        public async Task<IActionResult> AddOrUpdateCartAsync(Guid tableId, string? actorId, [FromBody] CartItemRedisModel model)
        {
            await _cartService.AddOrUpdateItemAsync(tableId, actorId ?? UserId, model, StoreId);
            return Ok();
        }

        [HttpPost("clear/table/{tableId}/cart/{actorId}")]
        public async Task ClearCartAsync(Guid tableId, string actorId)
        {
            await _cartService.ClearCartAsync(tableId, StoreId, actorId);
        }

        [HttpPost("get/table/{tableId}/cart/{actorId}")]
        public async Task<List<CartItemRedisModel>> GetCartAsync(Guid tableId, string actorId)
        {
            return await _cartService.GetCartAsync(tableId, StoreId, actorId);
        }

        [HttpDelete("table/{tableId}/cart/{actorId}")]
        public async Task RemoveItemCartAsync(Guid tableId, string actorId, [FromBody] RemoveItemCartRequest request)
        {
            await _cartService.RemoveItemAsync(tableId, actorId, StoreId, request.MenuItemId, request.VariantId, request.Quantity);
        }

    }
}
