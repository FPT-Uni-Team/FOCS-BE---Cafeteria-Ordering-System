using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class KitchenService : IKitchenService
    {
        private readonly IRealtimeService _realtimeService;

        public KitchenService(IRealtimeService realtimeService)
        {
            _realtimeService = realtimeService;
        }

        public async Task SendOrdersToKitchenAsync(List<OrderDTO> pendingOrders)
        {
            if (pendingOrders == null || pendingOrders.Count <= 0) return;

            // Group orders by StoreId
            var mapOrderWithStore = pendingOrders
                .GroupBy(o => o.StoreId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var item in mapOrderWithStore)
            {
                var currentStoreId = item.Key;
                var currentOrders = item.Value;

                var orderWrap = currentOrders
                    .SelectMany(order => order.OrderDetails)
                    .GroupBy(detail => detail.MenuItemId)
                    .Select(group => new SendOrderWrapDTO
                    {
                        MenuItemId = group.Key,
                        MenuItemName = group.First().MenuItemName,
                        Variants = group.Select(detail => new VariantWrapOrder
                        {
                            VariantId = detail.VariantId,
                            VariantName = detail.VariantName,
                            Note = detail.Note
                        }).ToList()
                    }).ToList();

                var groupName = SignalRGroups.Kitchen(currentStoreId);

                await _realtimeService.SendToGroupAsync<OrderHub, List<SendOrderWrapDTO>>(
                    groupName,
                    Constants.Method.ReceiveOrderWrapUpdate,
                    orderWrap
                );
            }
        }

    }
}
