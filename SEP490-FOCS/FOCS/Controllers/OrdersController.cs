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
        private readonly IHubContext<OrderHub> _orderHubContext;

        private readonly IOrderService _orderService;

        public OrdersController(IHubContext<OrderHub> hubContext, IOrderService orderService)
        {
            _orderHubContext = hubContext;  
            _orderService = orderService;
        }

        [HttpPost("order/guest")]
        public async Task<DiscountResultDTO> CreateOrderAsGuest([FromBody] CreateOrderRequest request)
        {
            return await _orderService.CreateOrderAsGuestAsync(request, UserId);
        }
    }
}
