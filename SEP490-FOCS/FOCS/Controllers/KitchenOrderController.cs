using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KitchenOrderController : ControllerBase
    {
        private readonly IOrderWrapService _orderWrapService;

        public KitchenOrderController(IOrderWrapService orderWrapService)
        {
            _orderWrapService = orderWrapService;
        }

        [HttpPut("change-status")]
        public async Task<bool> ChangeStatusProductionOrder([FromBody] UpdateStatusProductionOrderRequest dto)
        {
            return await _orderWrapService.ChangeStatusProductionOrder(dto);
        }
    }
}
