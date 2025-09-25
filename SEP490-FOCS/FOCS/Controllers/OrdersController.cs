using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using Org.BouncyCastle.Pkcs;
using System.Formats.Asn1;

namespace FOCS.Controllers
{
    [Route("api/order")]
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

        [HttpPost("history")]
        //[Authorize(Roles = Roles.User + "," + Roles.Staff)]
        public async Task<PagedResult<OrderDTO>> GetOrders(UrlQueryParameters queryParameters, [FromHeader] string actorId)
        {
            return await _orderService.GetListOrders(queryParameters, StoreId, UserId ?? actorId);
        }

        [HttpPost]
        public async Task<DiscountResultDTO> CreateOrderAsync([FromBody] CreateOrderRequest request, [FromHeader(Name = "actorId")] string actorId)
        {
            return await _orderService.CreateOrderAsync(request, UserId ?? actorId);
        }

        [HttpPost("{actorId}/apply-discount")]
        public async Task<DiscountResultDTO> ApplyDiscountForOrder([FromBody] ApplyDiscountOrderRequest request, string actorId)
        {
            return await _orderService.ApplyDiscountForOrder(request, UserId ?? actorId, StoreId);
        }

        [HttpGet("{orderId}")]
        public async Task<OrderDTO> GetOrderDetailAsync(Guid orderId)   
        {
            return await _orderService.GetUserOrderDetailAsync(Guid.Parse(UserId), orderId);
        }

        [HttpGet("order-by-code/{code}")]
        public async Task<OrderDTO> GetOrderDetailByCodeAsync(long code)
        {
            return await _orderService.GetOrderByCodeAsync(code);
        }

        [HttpPatch("change-status/{code}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager + "," + Roles.Staff + "," + Roles.KitchenStaff)]
        public async Task<bool> ChangeStatusOrder(string code, ChangeOrderStatusRequest request)
        {
            return await _orderService.ChangeStatusOrder(code, request, StoreId);
        }

        [HttpPost("cancel/{id}")]
        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            return await _orderService.CancelOrderAsync(orderId, UserId, StoreId);
        }

        [HttpDelete("{code}")]
        //[Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
        public async Task<bool> DeleteOrderAsync(string code)
        {
            return await _orderService.DeleteOrderAsync(code, UserId, StoreId);
        }

        [HttpPost("pending-in-day")]
        public async Task<IActionResult> GetPendingInDay()
        {
            var rs = await _orderService.GetPendingOrdersInDayAsync();
            return Ok();
        }
    }
}
