using AutoMapper;
using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.Application.Services
{
    public class KitchenService : IKitchenService
    {
        private readonly IRealtimeService _realtimeService;
        private readonly IRepository<OrderEntity> _orderRepo;

        private readonly IRepository<OrderWrap> _orderWrapRepository;

        private readonly IMobileTokenSevice _mobileTokenService;

        public KitchenService(IRealtimeService realtimeService, IRepository<OrderEntity> orderRepo, IRepository<OrderWrap> orderWrapRepository, IMobileTokenSevice mobileTokenService)
        {
            _realtimeService = realtimeService;
            _orderRepo = orderRepo;
            _orderWrapRepository = orderWrapRepository;
            _mobileTokenService = mobileTokenService;
        }

        public async Task SendOrdersToKitchenAsync(List<OrderDTO> pendingOrders)
        {
            if (pendingOrders == null || pendingOrders.Count <= 0) return;

            // Group orders by StoreId
            var mapOrderWithStore = pendingOrders
                .GroupBy(o => o.StoreId)    
                .ToDictionary(g => g.Key, g => g.ToList());

            Random random = new Random();

            foreach (var item in mapOrderWithStore)
            {
                var currentStoreId = item.Key;
                var currentOrders = item.Value;

                var orderWrapId = Guid.NewGuid();

                var orderWrap = currentOrders
                    .SelectMany(order => order.OrderDetails)
                    .GroupBy(detail => detail.MenuItemId)
                    .Select(group => new SendOrderWrapDTO
                    {
                        OrderWrapId = orderWrapId,
                        MenuItemId = group.Key,
                        MenuItemName = group.First().MenuItemName,
                        Variants = group.Select(detail => new VariantWrapOrder
                        {
                            VariantId = string.Join(",", detail.Variants.Select(x => x.VariantId)),
                            VariantName = string.Join(" - ", detail.Variants.Select(x => x.VariantName)),
                            Note = detail.Note
                        }).ToList()
                    }).ToList();

                var orders = await _orderRepo.AsQueryable().Where(x => pendingOrders.Select(y => y.Id).Contains(x.Id)).ToListAsync();

                var orderWrapModel = new OrderWrap
                {
                    Id = orderWrapId,
                    Code = random.Next(99999).ToString(),
                    OrderWrapStatus = Common.Enums.OrderWrapStatus.Created,
                    StoreId = currentStoreId,
                    Orders = orders
                };

                await _orderWrapRepository.AddAsync(orderWrapModel);
                await _orderWrapRepository.SaveChangesAsync();

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
