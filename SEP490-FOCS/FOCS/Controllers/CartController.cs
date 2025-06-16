using FOCS.Common.Interfaces;
using FOCS.Common.Models.CartModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : FocsController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("tables/{tableId}/cart/{actorId}")]
        public async Task<IActionResult> AddOrUpdateCartAsync(Guid tableId, string actorId, [FromBody] CartItemRedisModel model)
        {
            await _cartService.AddOrUpdateItemAsync(tableId, actorId ?? UserId, model, StoreId);
            return Ok();
        }
    }
}
