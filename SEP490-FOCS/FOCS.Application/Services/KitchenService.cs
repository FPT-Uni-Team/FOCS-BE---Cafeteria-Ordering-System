using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class KitchenService : IKitchenService
    {
        private readonly IHubContext<OrderHub> _orderHubContext;
        public async Task SendOrdersToKitchenAsync(List<OrderDTO> pendingOrders)
        {
            //With Store
            var mapOrderWithStore = pendingOrders
                .GroupBy(o => o.StoreId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach(var item in mapOrderWithStore)
            {
                var currentStoreId = item.Key;
                var currentOrders = item.Value;

                var group = SignalRGroups.Kitchen(currentStoreId);
                await _orderHubContext.Clients.Group(group).SendAsync(Constants.Method.ReceiveOrderWrapUpdate, currentOrders);
            }
        }
    }
}
