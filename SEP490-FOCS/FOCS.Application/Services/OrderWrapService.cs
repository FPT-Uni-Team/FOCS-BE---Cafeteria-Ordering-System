using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
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
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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

        private readonly IMapper _mapper;

        public OrderWrapService(IRepository<OrderWrap> orderWrapRepo, IMapper mapper, IMobileTokenSevice mobileTokenService, IRealtimeService realtimeService, IPublishEndpoint publishEndpoint, IRepository<OrderEntity> orderRepo)
        {
            _orderWrapRepo = orderWrapRepo;
            _publishEndpoint = publishEndpoint;
            _realtimeService = realtimeService;
            _orderRepo = orderRepo;
            _mobileTokenService = mobileTokenService;
            _mapper = mapper;
        }

        public async Task<bool> ChangeStatusProductionOrder(UpdateStatusProductionOrderRequest dto)
        {
            try
            {
                var orderWrap = await _orderWrapRepo.AsQueryable().Include(x => x.Orders).ThenInclude(x => x.Table).FirstOrDefaultAsync(x => x.Id == dto.OrderWrapId);

                if (orderWrap == null) return false;

                if (dto.Status == Common.Enums.OrderWrapStatus.Completed)
                {
                    // split order -> sync success process to user order
                    var orders = orderWrap.Orders.ToList();

                    orders.ForEach(x => x.OrderStatus = Common.Enums.OrderStatus.Completed);

                    _orderRepo.UpdateRange(orders);
                    await _orderRepo.SaveChangesAsync();

                    foreach (var order in orders)
                    {
                        var currentToken = await _mobileTokenService.GetMobileToken((Guid)order.UserId);

                        var notifyEventForUser = new NotifyEvent
                        {
                            Title = "Your order is complete",
                            Message = "Your order is complete, Please check with staff",
                            storeId = order.StoreId.ToString(),
                            tableId = order.TableId.ToString(),
                            TargetGroups = new string[] { SignalRGroups.User(order.StoreId, (Guid)order.TableId, (Guid)order.UserId) },
                            MobileTokens = new string[] { currentToken.Token }
                        };

                        var notifyEventForStaff = new NotifyEvent
                        {
                            Title = $"Order in table {order.Table.TableNumber} is complete",
                            Message = $"Order in table {order.Table.TableNumber} is complete, Please check with user",
                            storeId = order.StoreId.ToString(),
                            tableId = order.TableId.ToString(),
                            TargetGroups = new string[] { SignalRGroups.Cashier(order.StoreId, (Guid)order.TableId) }
                        };

                        await _publishEndpoint.Publish(notifyEventForStaff);
                        await _publishEndpoint.Publish(notifyEventForUser);
                    }
                }
                else if (dto.Status == Common.Enums.OrderWrapStatus.Processing)
                {
                    var orders = orderWrap.Orders.ToList();

                    orders.ForEach(x => x.OrderStatus = Common.Enums.OrderStatus.Confirmed);

                    _orderRepo.UpdateRange(orders);
                    await _orderRepo.SaveChangesAsync();

                    foreach (var order in orders)
                    {
                        var currentToken = await _mobileTokenService.GetMobileToken((Guid)order.UserId);

                        var notifyEventForUser = new NotifyEvent
                        {
                            Title = "Your order is processing",
                            Message = "Your order is processing, Please wait a minute",
                            storeId = order.StoreId.ToString(),
                            tableId = order.TableId.ToString(),
                            TargetGroups = new string[] { SignalRGroups.User(order.StoreId, (Guid)order.TableId, (Guid)order.UserId) },
                            MobileTokens = new string[] { currentToken.Token }
                        };

                        await _publishEndpoint.Publish(notifyEventForUser);
                    }
                }
                return true;
            } catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<PagedResult<OrderWrapResponse>> GetListOrderWraps(UrlQueryParameters query, string storeId)
        {
            var ordersWrapQuery = _orderWrapRepo.AsQueryable().Include(x => x.Orders).Where(x => x.StoreId == Guid.Parse(storeId));

            //promotionQuery = ApplyFilters(promotionQuery, query);
            //promotionQuery = ApplySearch(promotionQuery, query);
            //promotionQuery = ApplySort(promotionQuery, query);

            var total = await ordersWrapQuery.CountAsync();
            var items = await ordersWrapQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
            .ToListAsync();

            var rs = ordersWrapQuery.Select(x => new OrderWrapResponse
            {
                Code = x.Code,
                Status = x.OrderWrapStatus,
                Orders = x.Orders.Select(z => new OrderKithcenResponse
                {
                    Code = z.OrderCode.ToString(),
                    Amount = z.TotalAmount
                }).ToList()
            }).ToList();

            return new PagedResult<OrderWrapResponse>(rs, total, query.Page, query.PageSize);
        }

        public async Task<List<SendOrderWrapDTO>> GetOrderWrapDetail(string code, string storeId)
        {
            var orderWrap = await _orderWrapRepo.AsQueryable().FirstOrDefaultAsync(x => x.Code == code && x.StoreId == Guid.Parse(storeId));
            var orders = await _orderWrapRepo.AsQueryable().Include(x => x.Orders).Where(x => x.Code == code && x.StoreId == Guid.Parse(storeId)).SelectMany(x => x.Orders).ToListAsync();

            var orderDto = _mapper.Map<List<OrderDTO>>(orders);

            var orderWrapRes = orderDto
                    .SelectMany(order => order.OrderDetails)
                    .GroupBy(detail => detail.MenuItemId)
                    .Select(group => new SendOrderWrapDTO
                    {
                        OrderWrapId = orderWrap.Id,
                        MenuItemId = group.Key,
                        MenuItemName = group.First().MenuItemName,
                        Variants = group.Select(detail => new VariantWrapOrder
                        {
                            VariantId = detail.VariantId,
                            VariantName = detail.VariantName,
                            Note = detail.Note
                        }).ToList()
                    }).ToList();

            return orderWrapRes;
        }
    }
}
