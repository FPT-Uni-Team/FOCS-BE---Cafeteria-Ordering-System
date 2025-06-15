using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FOCS.Controllers
{
    [Route("api")]
    [ApiController]
    public class OrdersController : FocsController
    {
        //SignalR 
        //private readonly IHubContext<OrderHub> _orderHubContext;

        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("order")]
        public async Task<DiscountResultDTO> CreateOrderAsync([FromBody] CreateOrderRequest request)
        {
            var test = Email;
            var user = User;
            return await _orderService.CreateOrderAsync(request, UserId);
        }
    }
}
