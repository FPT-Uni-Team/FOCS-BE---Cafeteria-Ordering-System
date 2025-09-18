using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.NotificationService.Models;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Realtime.Hubs;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly INotifyService _notifyService;
        private readonly IMobileTokenSevice _mobileTokenService;

        private readonly IMapper _mapper;

        private readonly IRepository<WorkshiftSchedule> _workshiftSchedule;
        private readonly IRepository<MenuItemVariant> _variantRepo;

        private readonly ILogger<OrderWrapService> _logger;

        public OrderWrapService(
            ILogger<OrderWrapService> logger, 
            IRepository<MenuItemVariant> variantRepo,
            IRepository<OrderWrap> orderWrapRepo,
            IMapper mapper,
            IMobileTokenSevice mobileTokenService,
            IRealtimeService realtimeService,
            IPublishEndpoint publishEndpoint,
            IRepository<OrderEntity> orderRepo,
            INotifyService notifyService,
            IRepository<WorkshiftSchedule> workshiftScheduleRepo)
        {
            _orderWrapRepo = orderWrapRepo;
            _variantRepo = variantRepo;
            _publishEndpoint = publishEndpoint;
            _realtimeService = realtimeService;
            _orderRepo = orderRepo;
            _mobileTokenService = mobileTokenService;
            _mapper = mapper;
            _logger = logger;
            _notifyService = notifyService;
            _workshiftSchedule = workshiftScheduleRepo;
        }

        public async Task<bool> ChangeStatusProductionOrder(UpdateStatusProductionOrderRequest dto)
        {
            try
            {
                var orderWrap = await _orderWrapRepo.AsQueryable()
                    .Include(x => x.Orders)
                        .ThenInclude(x => x.Table)
                    .FirstOrDefaultAsync(x => x.Id == dto.OrderWrapId);

                if (orderWrap == null) return false;

                orderWrap.OrderWrapStatus = dto.Status;

                var orders = orderWrap.Orders.ToList();

                if (dto.Status == Common.Enums.OrderWrapStatus.Completed)
                {
                    try
                    {
                        foreach (var order in orders)
                        {
                            order.OrderStatus = Common.Enums.OrderStatus.Completed;
                        }

                        _orderRepo.UpdateRange(orders);
                        await _orderRepo.SaveChangesAsync();
                    } catch(Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        return false;
                    }

                    foreach (var order in orders)
                    {
                        if (order.UserId == null) continue;

                        var currentToken = await _mobileTokenService.GetMobileToken((Guid)order.UserId);

                        if (currentToken != null)
                        {
                            var notifyEventForUser = new NotifyEvent
                            {
                                Title = "Your order is complete",
                                Message = "Your order is complete, Please check with staff",
                                storeId = order.StoreId.ToString(),
                                tableId = order.TableId?.ToString(),
                                TargetGroups = new[] { SignalRGroups.User(order.StoreId, (Guid)order.TableId, (Guid)order.UserId) },
                                MobileTokens = new[] { currentToken.Token }
                            };
                            _logger.LogInformation($"[Notify Event For User] Notify userId {order.UserId} for order {notifyEventForUser.ToString()}");

                            await _publishEndpoint.Publish(notifyEventForUser);
                            await _notifyService.AddNotifyAsync(order.UserId.ToString(), Constants.ActionTitle.ReceiveNotify(order.TableId?.ToString()).ToString());
                        }

                        if (order.Table != null)
                        {
                            _logger.LogInformation($"table of order:  {order.TableId}");

                            var now = DateTime.Now.TimeOfDay;

                            var staffIds = await _workshiftSchedule.AsQueryable()
                                .Include(x => x.StaffWorkshiftRegistrations)
                                .Include(z => z.Workshift)
                                .Where(z => z.Workshift.WorkDate == DateTime.Now)
                                .Where(x => x.StartTime < now && x.EndTime > now)
                                .SelectMany(x => x.StaffWorkshiftRegistrations.Select(x => x.StaffId))
                                .ToListAsync();

                            _logger.LogInformation($"staffids:  {staffIds.Count()}: {staffIds.FirstOrDefault()}");

                            if (staffIds != null)
                            {
                                foreach (var staffId in staffIds)
                                {
                                    var currentDeviceToken = await _mobileTokenService.GetMobileToken((Guid)staffId);

                                    if (currentDeviceToken == null) { continue; }

                                    var notifyEventForStaff = new NotifyEvent
                                    {
                                        Title = $"Order in table {order.Table.TableNumber} is complete",
                                        Message = $"Order in table {order.Table.TableNumber} is complete, Please check with user",
                                        storeId = order.StoreId.ToString(),
                                        tableId = order.TableId?.ToString(),
                                        TargetGroups = new[] { SignalRGroups.Cashier(order.StoreId, (Guid)order.TableId) },
                                        MobileTokens = new[] { currentDeviceToken.Token }
                                    };
                                    _logger.LogInformation($"[Notify Event For Staff] Notify staff {staffId} for order {notifyEventForStaff.ToString()}");

                                    await _publishEndpoint.Publish(notifyEventForStaff);
                                    await _notifyService.AddNotifyAsync(staffId.ToString(), Constants.ActionTitle.ReceiveNotify(order.TableId?.ToString()).ToString());
                                }
                            }
                        }
                    }
                }
                else if (dto.Status == Common.Enums.OrderWrapStatus.Processing)
                {
                    try
                    {
                        foreach (var order in orders)
                        {
                            order.OrderStatus = Common.Enums.OrderStatus.Ready;
                        }

                        _orderRepo.UpdateRange(orders);
                        await _orderRepo.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        return false;
                    }

                    foreach (var order in orders)
                    {
                        if (order.UserId == null) continue;

                        var currentToken = await _mobileTokenService.GetMobileToken((Guid)order.UserId);

                        if (currentToken != null)
                        {
                            var notifyEventForUser = new NotifyEvent
                            {
                                Title = "Your order is processing",
                                Message = "Your order is processing, Please wait a minute",
                                storeId = order.StoreId.ToString(),
                                tableId = order.TableId?.ToString(),
                                TargetGroups = new[] { SignalRGroups.User(order.StoreId, (Guid)order.TableId, (Guid)order.UserId) },
                                MobileTokens = new[] { currentToken.Token }
                            };

                            await _publishEndpoint.Publish(notifyEventForUser);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"[ChangeStatusProductionOrder] Error: {ex}");
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
                .OrderBy(x => x.CreatedAt)
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
            var storeGuid = Guid.Parse(storeId); 

            var orderWrap = await _orderWrapRepo.AsQueryable()
                .FirstOrDefaultAsync(x => x.Code == code && x.StoreId == storeGuid);

            var orders = await _orderWrapRepo.AsQueryable()
                .Include(x => x.Orders)
                    .ThenInclude(x => x.OrderDetails)
                        .ThenInclude(x => x.MenuItem)
                .Where(x => x.Code == code && x.StoreId == storeGuid)  
                .SelectMany(x => x.Orders)
                .ToListAsync();

            var orderDto = _mapper.Map<List<OrderDTO>>(orders);

            var orderWrapRes = orderDto
                .SelectMany(order => order.OrderDetails)
                .GroupBy(detail => detail.MenuItemId)
                .Select(group => new SendOrderWrapDTO
                {
                    OrderWrapId = orderWrap.Id,
                    MenuItemId = group.Key,
                    MenuItemName = group.First().MenuItemName,
                    Variants = group
                        .SelectMany(detail => detail.Variants.Select(v => new VariantWrapOrder
                        {
                            VariantId = v.VariantId.ToString(),
                            VariantName = _variantRepo.AsQueryable().FirstOrDefault(x => x.Id == v.VariantId)?.Name,
                            Note = detail.Note
                        }))
                        .ToList()
                })
                .ToList();

            return orderWrapRes;
        }
    }
}
