using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.NotificationService.Models;
using FOCS.Order.Infrastucture.Entities;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.Application.Services
{
    public class OrderWrapService : IOrderWrapService
    {
        private readonly IRepository<OrderWrap> _orderWrapRepo;

        private readonly IRepository<OrderEntity> _orderRepo;

        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRealtimeService _realtimeService;

        private readonly IMobileTokenSevice _mobileTokenService;

        public OrderWrapService(IRepository<OrderWrap> orderWrapRepo, IMobileTokenSevice mobileTokenService, IRealtimeService realtimeService, IPublishEndpoint publishEndpoint, IRepository<OrderEntity> orderRepo)
        {
            _orderWrapRepo = orderWrapRepo;
            _publishEndpoint = publishEndpoint;
            _realtimeService = realtimeService;
            _orderRepo = orderRepo;
            _mobileTokenService = mobileTokenService;
        }

        public async Task<bool> ChangeStatusProductionOrder(UpdateStatusProductionOrderRequest dto)
        {
            var orderWrap = await _orderWrapRepo.AsQueryable().Include(x => x.Orders).ThenInclude(x => x.Table).FirstOrDefaultAsync(x => x.Id == dto.OrderWrapId);

            if(orderWrap == null) return false;

            if(dto.Status == Common.Enums.OrderWrapStatus.Completed)
            {
                // split order -> sync success process to user order
                var orders = orderWrap.Orders.ToList();

                orders.ForEach(x => x.OrderStatus = Common.Enums.OrderStatus.Completed);

                _orderRepo.UpdateRange(orders);
                await _orderRepo.SaveChangesAsync();

                foreach(var order in orders)
                {
                    var currentToken = await _mobileTokenService.GetMobileToken((Guid)order.UserId);

                    var notifyEventForUser = new NotifyEvent
                    {
                        Title = "Your order is complete",
                        Message = "Your order is complete, Please check with staff",
                        storeId = order.StoreId.ToString(),
                        tableId = order.TableId.ToString(),
                        TargetGroups = new string[] { SignalRGroups.User(order.StoreId, (Guid)order.TableId, (Guid)order.UserId)},
                        MobileTokens = new string[] { currentToken.Token }
                    };

                    var notifyEventForStaff = new NotifyEvent
                    {
                        Title = $"Order in table {order.Table.TableNumber} is complete",
                        Message = "Order in table {order.Table.TableNumber} is complete, Please check with user",
                        storeId = order.StoreId.ToString(),
                        tableId = order.TableId.ToString(),
                        TargetGroups = new string[] {SignalRGroups.Cashier(order.StoreId, (Guid)order.TableId) }
                    };
                }
            }

            orderWrap.OrderWrapStatus = dto.Status;
            //await _publishEndpoint.Publish(new NotifyEvent
            //{
            //    Title = "Order change process",
            //    Message = "Your order is going to next process",
            //    TargetGroups = SignalRGroups.User(dto.)
            //});
            // update status wrap order -> sync process to user order


            return true;

        }
    }
}
