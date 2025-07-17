using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KitchenOrderController : ControllerBase
    {
        [HttpGet("wrap-orders")]
        public async Task<IActionResult> GetWrapOrdersRealtime()
        {
            return Ok();
        }

        [HttpPut("mark-done")]
        public async Task<IActionResult> MarkDishAsCompleted(/*[FromBody] DishStatusDto dto*/)
        {
            return Ok();
        }
    }
}
