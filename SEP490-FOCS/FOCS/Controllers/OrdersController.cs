using FOCS.Application.DTOs;
using FOCS.Common.Constants;
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

        public OrdersController(IHubContext<OrderHub> hubContext)
        {
            _orderHubContext = hubContext;  
        }

        [HttpPost("notify-kitchen")]
        public async Task NotifyKitchenAsync(OrderWrapDTO orderWrapDTO)
        {
            var group = SignalRGroups.Kitchen(orderWrapDTO.StoreId);
            await _orderHubContext.Clients.Group(group).SendAsync(Constants.Method.ReceiveOrderWrapUpdate, orderWrapDTO);
        }

    }
}
