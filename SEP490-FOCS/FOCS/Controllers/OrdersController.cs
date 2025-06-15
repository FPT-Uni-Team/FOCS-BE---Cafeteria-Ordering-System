using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Pkcs;

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
            return await _orderService.CreateOrderAsync(request, UserId);
        }

        [HttpGet("order/{orderId}")]
        [Authorize(Roles = Roles.User)]
        public async Task<OrderDTO> GetOrderDetailAsync(Guid orderId)
        {
            return await _orderService.GetUserOrderDetailAsync(Guid.Parse(UserId), orderId);
        }

        [HttpGet("order-by-code/{code}")]
        [Authorize(Roles = Roles.User)]
        public async Task<OrderDTO> GetOrderDetailByCodeAsync(string code)
        {
            return await _orderService.GetOrderByCodeAsync(code);
        }
    }
}
