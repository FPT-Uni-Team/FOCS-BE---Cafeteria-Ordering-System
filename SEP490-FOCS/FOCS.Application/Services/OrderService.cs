using AutoMapper;
using Azure.Core;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Models.CartModels;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.NotificationService.Models;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Realtime.Hubs;
using MailKit;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Modes.Gcm;
using Org.BouncyCastle.Utilities.Collections;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static MassTransit.ValidationResultExtensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace FOCS.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly IRepository<MenuItemVariant> _variantRepository;
        private readonly IRepository<Table> _tableRepository;
        private readonly IRepository<FOCS.Order.Infrastucture.Entities.Order> _orderRepository;
        private readonly IRepository<Coupon> _couponRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;

        private readonly IMobileTokenSevice _mobileTokenService;

        private readonly IPricingService _pricingService;

        private readonly IPromotionService _promotionService;
        private readonly DiscountContext _discountContext;
        private readonly IRepository<SystemConfiguration> _systemConfig;
        private readonly IStoreSettingService _storeSettingService;

        private readonly ICouponUsageService _couponUsageService;
        private readonly IRealtimeService _realtimeService;

        private readonly ILogger<OrderService> _logger;
        private readonly IMapper _mapper;

        private readonly IPublishEndpoint _publishEndpoint;

        private readonly UserManager<User> _userManager;

        public OrderService(IRepository<FOCS.Order.Infrastucture.Entities.Order> orderRepository, 
                            ILogger<OrderService> logger, 
                            IRepository<OrderDetail> orderDetailRepository, 
                            IPricingService pricingService, 
                            IRepository<Coupon> couponRepository, 
                            DiscountContext discountContext, 
                            IStoreSettingService storeSettingService, 
                            IRepository<Table> tableRepo, 
                            IRepository<Store> storeRepository, 
                            IRepository<MenuItem> menuRepository, 
                            IRepository<MenuItemVariant> variantRepository, 
                            IPromotionService promotionService,
                            IMapper mapper,
                            IRealtimeService realtimeService,
                            UserManager<User> userManager,
                            IRepository<SystemConfiguration> systemConfig,
                            IPublishEndpoint publishEndpoint,
                            IMobileTokenSevice mobileTokenService,
                            ICouponUsageService couponUsageService)
        {
            _orderRepository = orderRepository;
            _realtimeService = realtimeService;
            _logger = logger;
            _pricingService = pricingService;
            _orderDetailRepository = orderDetailRepository;
            _couponRepository = couponRepository;
            _storeRepository = storeRepository;
            _discountContext = discountContext;
            _menuItemRepository = menuRepository;
            _storeSettingService = storeSettingService;
            _variantRepository = variantRepository;
            _promotionService = promotionService;
            _userManager = userManager;
            _tableRepository = tableRepo;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
            _systemConfig = systemConfig;
            _couponUsageService = couponUsageService;
            _mobileTokenService = mobileTokenService;
        }

        public async Task<DiscountResultDTO> CreateOrderAsync(CreateOrderRequest order, string userId)
        {
            var store = await _storeRepository.GetByIdAsync(order.StoreId);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound);

            Table? table = null;

            if(order.OrderType == OrderType.DineIn)
            {
                table = await _tableRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == order.TableId && x.StoreId == order.StoreId);
                ConditionCheck.CheckCondition(table != null, Errors.OrderError.TableNotFound);
            }

            // Validate menu items
            await ValidateMenuItemsAsync(order.Items);

            var storeSettings = await _storeSettingService.GetStoreSettingAsync(order.StoreId, userId); 
            ConditionCheck.CheckCondition(storeSettings != null, Errors.Common.StoreNotFound);

            //save order and order detail
            await SaveOrderAsync(order, table, store, userId);

            return order.DiscountResult;
        }

        public async Task<DiscountResultDTO> ApplyDiscountForOrder(ApplyDiscountOrderRequest orderRequest, string userId, string storeId)
        {
            if(orderRequest.CouponCode == null)
            {
                var rs = new DiscountResultDTO();

                double totalOrderAmount = 0;
                var pricingDict = new Dictionary<(Guid MenuItemId, Guid? VariantId), double>();
                foreach (var item in orderRequest.Items)
                {
                    if (item.Variants != null && item.Variants.Count > 0)
                    {
                        foreach (var itemVariant in item.Variants)
                        {
                            var price = await _pricingService.GetPriceByProduct(item.MenuItemId, itemVariant.VariantId, Guid.Parse(storeId));
                            double itemUnitPrice = price.ProductPrice + (price.VariantPrice ?? 0);
                            double itemTotalPrice = itemUnitPrice * itemVariant.Quantity;

                            pricingDict[(item.MenuItemId, itemVariant.VariantId)] = itemUnitPrice;
                            totalOrderAmount += itemTotalPrice;
                            rs.TotalPrice += (decimal)itemTotalPrice;
                        }
                    }
                    else
                    {
                        var price = await _pricingService.GetPriceByProduct(item.MenuItemId, null, Guid.Parse(storeId));
                        double itemUnitPrice = price.ProductPrice + (price.VariantPrice ?? 0);
                        double itemTotalPrice = itemUnitPrice * item.Quantity;

                        pricingDict[(item.MenuItemId, null)] = itemUnitPrice;
                        totalOrderAmount += itemTotalPrice;
                        rs.TotalPrice += (decimal)itemTotalPrice;
                    }
                }

                return rs;
            }

            await _promotionService.IsValidPromotionCouponAsync(orderRequest.CouponCode!, userId.ToString(), orderRequest.StoreId);

            var storeSettings = await _storeSettingService.GetStoreSettingAsync(orderRequest.StoreId);

            ConditionCheck.CheckCondition(storeSettings != null, Errors.Common.NotFound);
            ConditionCheck.CheckCondition(!storeSettings!.DiscountStrategy.Equals(null), Errors.StoreSetting.DiscountStrategyNotConfig);

            return await _discountContext.CalculateDiscountAsync(orderRequest, orderRequest.CouponCode, storeSettings.DiscountStrategy, userId);
        }

        public async Task<PagedResult<OrderDTO>> GetListOrders(UrlQueryParameters queryParameters, string storeId, string userId)
        {
            var ordersQuery = _orderRepository.AsQueryable().Where(x => x.StoreId == Guid.Parse(storeId) && x.UserId == Guid.Parse(userId) && !x.IsDeleted);

            ordersQuery = ApplyFilters(ordersQuery, queryParameters);
            ordersQuery = ApplySearch(ordersQuery, queryParameters);
            ordersQuery = ApplySort(ordersQuery, queryParameters);

            var total = await ordersQuery.CountAsync();
            var items = await ordersQuery
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
            .ToListAsync();

            var mapped = _mapper.Map<List<OrderDTO>>(items);
            return new PagedResult<OrderDTO>(mapped, total, queryParameters.Page, queryParameters.PageSize);
        }

        public async Task<OrderDTO> GetOrderByCodeAsync(long orderCode)
        {
            var orderByCode = await _orderRepository.AsQueryable()
                                                    .Include(x => x.OrderDetails)
                                                        .ThenInclude(od => od.MenuItem)
                                                    .FirstOrDefaultAsync(x => x.OrderCode == orderCode);

            if (orderByCode == null)
                return new OrderDTO();

            var variantIds = orderByCode.OrderDetails
                                  .Where(od => od.VariantId.HasValue)
                                  .Select(od => od.VariantId.Value)
                                  .Distinct()
                                  .ToList();

            var variants = await _variantRepository.AsQueryable()
                                                   .Where(x => variantIds.Contains(x.Id))
                                                   .ToListAsync();

            var variantDict = variants.ToDictionary(x => x.Id);

            foreach (var item in orderByCode.OrderDetails)
            {
                if (variantDict.TryGetValue((Guid)item.VariantId, out var variant))
                {
                    item.Variant = variant;
                }
            }

            return _mapper.Map<OrderDTO>(orderByCode);
        }

        public async Task<bool> ChangeStatusOrder(string code, ChangeOrderStatusRequest request, string storeId)
        {
            try
            {
                var order = await _orderRepository.AsQueryable().FirstOrDefaultAsync(x => x.OrderCode.ToString() == code && !x.IsDeleted && x.StoreId == Guid.Parse(storeId));

                ConditionCheck.CheckCondition(order != null, Errors.Common.NotFound);

                order.OrderStatus = request.OrderStatus;

                if(order.OrderStatus == OrderStatus.Confirmed)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                }

                order.UpdatedAt = DateTime.UtcNow;

                _orderRepository.Update(order);
                await _orderRepository.SaveChangesAsync();

                return true;
            } catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<List<OrderDTO>> GetPendingOrdersInDayAsync()
        {
            var timeSince = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));

            var ordersPending = await _orderRepository.AsQueryable()
                                                      .Include(x => x.OrderDetails)
                                                      .ThenInclude(x => x.Variant)
                                                      .Where(x => x.OrderStatus == OrderStatus.Pending
                                                              && !x.IsDeleted
                                                              && x.PaymentStatus == PaymentStatus.Paid
                                                              && x.CreatedAt >= timeSince)
                                                      .ToListAsync();

            var mappingOrders = _mapper.Map<List<OrderDTO>>(ordersPending);

            if(ordersPending != null)
            {
                ordersPending.ForEach(x => x.OrderStatus = OrderStatus.Confirmed);

                _orderRepository.UpdateRange(ordersPending);
                await _orderRepository.SaveChangesAsync();
            }

            foreach (var dto in mappingOrders)
            {
                var original = ordersPending.FirstOrDefault(x => x.Id == dto.Id);
                if (original != null && original.OrderDetails.Any())
                {
                    var firstMenuItemId = original.OrderDetails.First().MenuItemId;

                    var menuItem = await _menuItemRepository.GetByIdAsync(firstMenuItemId);
                    var menuItemName = menuItem?.Name;

                    dto.OrderDetails.ForEach(detail =>
                    {
                        detail.MenuItemName = menuItemName;
                    });
                }
            }

            return mappingOrders;
        }

        public async Task<OrderDTO> GetUserOrderDetailAsync(Guid userId, Guid orderId)
        {
            var orderByUser = await _orderRepository.AsQueryable().Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.UserId == userId && x.Id == orderId);

            ConditionCheck.CheckCondition(orderByUser != null, Errors.Common.NotFound);

            return _mapper.Map<OrderDTO>(orderByUser);
        }

        public async Task MarkAsPaid(long orderCode, string storeId)
        {
            var order = await _orderRepository.AsQueryable().Include(x => x.Table).Include(x => x.Coupon).FirstOrDefaultAsync(x => x.OrderCode == orderCode);

            //update coupon, promotion usage
            var storeSetting = await _storeSettingService.GetStoreSettingAsync(Guid.Parse(storeId));

            if(storeSetting.DiscountStrategy == DiscountStrategy.CouponThenPromotion)
            {
                try
                {
                    var currentCoupon = await _couponRepository.AsQueryable().Include(x => x.Promotion).FirstOrDefaultAsync(x => x.Code == orderCode.ToString());
                    currentCoupon.CountUsed++;
                    var isAdded = await _couponUsageService.SaveCouponUsage(currentCoupon.Code, order.UserId, order.Id);

                    if (isAdded)
                    {
                        currentCoupon.Promotion.CountUsed++;
                    }

                    _couponRepository.Update(currentCoupon);
                    await _couponRepository.SaveChangesAsync();
                } catch(Exception ex)
                {
                    return;
                }
            } else
            {
                var currentCoupon = await _couponRepository.AsQueryable().FirstOrDefaultAsync(x => x.Code == order.Coupon.Code.ToString());
                currentCoupon.CountUsed++;
                var isAdded = await _couponUsageService.SaveCouponUsage(currentCoupon.Code, order.UserId, order.Id);

                _couponRepository.Update(currentCoupon);
                await _couponRepository.SaveChangesAsync();
            }

            order.PaymentStatus = PaymentStatus.Paid;

            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();

            var notifyEvent = new NotifyEvent
            {
                Title = Constants.ActionTitle.PaymentSuccess(order.Table.TableNumber),
                Message = Constants.ActionTitle.PaymentSuccess(order.Table.TableNumber),
                TargetGroups = new[] { SignalRGroups.Cashier(order.StoreId, (Guid)order.TableId) },
                storeId = order.StoreId.ToString(),
                tableId = order.TableId.ToString()
            };

            await _realtimeService.SendToGroupAsync<NotifyHub, NotifyEvent>(SignalRGroups.Cashier(order.StoreId, (Guid)order.TableId), Constants.Method.NewNotify, notifyEvent);
        }

        #region private methods
        private async Task SaveOrderAsync(CreateOrderRequest order, Table? table, Store store, string userId)
        {
            Random randomNum = new Random();

            try
            {
                Guid? couponCurrent = null;
                if (!string.IsNullOrEmpty(order.DiscountResult.AppliedCouponCode))
                {
                    var coupon = await _couponRepository.FindAsync(x => x.Code == order.DiscountResult.AppliedCouponCode);
                    couponCurrent = coupon.FirstOrDefault()?.Id;
                }

                //remaining time for order
                int remainingTimeOrder = 0;
                {
                    var dictProduct = order.Items.ToDictionary(
                            item => item.MenuItemId,
                            item => item.Variants.Select(x => x.VariantId) ?? new List<Guid>()
                        );

                    var variantGroupItems = await _menuItemRepository.AsQueryable()
                                                                    .Where(x => dictProduct.Keys.Contains(x.Id))
                                                                    .Include(x => x.MenuItemVariantGroups)
                                                                        .ThenInclude(y => y.MenuItemVariantGroupItems)
                                                                    .Select(z => z.MenuItemVariantGroups
                                                                        .Select(v => v.MenuItemVariantGroupItems)
                                                                        .ToList())
                                                                    .ToListAsync();

                    var currnetRemainingTimeOrder = variantGroupItems
                            .SelectMany(listLevel2 => listLevel2) 
                            .SelectMany(listLevel3 => listLevel3) 
                            .Where(item => item.IsActive && item.IsAvailable)
                            .Sum(item => item.PrepPerTime * item.QuantityPerTime);

                    remainingTimeOrder = (await _orderRepository.AsQueryable()
                        .Where(x => x.PaymentStatus == PaymentStatus.Paid && x.OrderStatus == OrderStatus.Confirmed)
                        .SumAsync(x => (int?)x.RemainingTime.Value.Minutes ?? 0)) + currnetRemainingTimeOrder;
                }

                var orderCreate = new Order.Infrastucture.Entities.Order
                {
                    Id = Guid.NewGuid(),
                    OrderCode = randomNum.Next(1000, 9999),
                    UserId = Guid.Parse(userId),
                    OrderStatus = OrderStatus.Pending,
                    OrderType = order.OrderType,
                    SubTotalAmout = (double)(order.DiscountResult.TotalPrice + order.DiscountResult.TotalDiscount),
                    TaxAmount = store.CustomTaxRate ?? 0,
                    DiscountAmount = (double)order.DiscountResult.TotalDiscount,
                    TotalAmount = (double)((double)order.DiscountResult.TotalPrice + store.CustomTaxRate ?? 0),
                    CustomerNote = order.Note ?? "",
                    StoreId = order.StoreId,
                    CouponId = couponCurrent,
                    PaymentStatus = order.PaymentType switch
                    {
                        PaymentType.CASH => PaymentStatus.Unpaid,
                        PaymentType.BANK_TRANSFER or PaymentType.ONLINE_PAYMENT => PaymentStatus.Waiting,
                        _ => PaymentStatus.Unpaid
                    },
                    CreatedBy = userId,
                    TableId = order.OrderType == OrderType.DineIn ? order.TableId : null,
                    RemainingTime = TimeSpan.FromMinutes(remainingTimeOrder)
                };

                var ordersDetailCreate = new List<OrderDetail>();
                if(order.DiscountResult.ItemDiscountDetails != null)
                {
                    foreach (var item in order.DiscountResult.ItemDiscountDetails)
                    {
                        var itemCodes = item.BuyItemCode?.Split("_");
                        if (itemCodes == null || itemCodes.Length == 0)
                            continue;

                        var productId = Guid.Parse(itemCodes[0]);

                        var variantIds = new List<Guid>();
                        if (itemCodes.Length > 1)
                        {
                            var variantIdStrings = itemCodes[1].Split(",", StringSplitOptions.RemoveEmptyEntries);
                            foreach (var variantIdStr in variantIdStrings)
                            {
                                if (Guid.TryParse(variantIdStr, out var vId))
                                {
                                    variantIds.Add(vId);
                                }
                            }
                        }

                        var price = await _pricingService.GetPriceByProduct(productId, null, order.StoreId);
                        double totalVariantPrice = 0;
                        foreach (var vId in variantIds)
                        {
                            var variantPrice = await _pricingService.GetPriceByProduct(productId, vId, order.StoreId);
                            totalVariantPrice += variantPrice.VariantPrice ?? 0;
                        }

                        double unitPrice = (double)(price.ProductPrice + totalVariantPrice);

                        ordersDetailCreate.AddRange(variantIds.Select(x => new OrderDetail
                        {
                            Id = Guid.NewGuid(),
                            Quantity = item.Quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = unitPrice - (double)item.DiscountAmount,
                            Note = "",
                            MenuItemId = productId,
                            VariantId = x,
                            OrderId = orderCreate.Id
                        }).ToList());
                    }

                }

                if (order.DiscountResult.IsUsePoint.HasValue && order.DiscountResult.IsUsePoint == true)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    ConditionCheck.CheckCondition(user != null, Errors.Common.NotFound);

                    var systemConfigEarningRate = (await _systemConfig.AsQueryable().FirstOrDefaultAsync())!.EarningRate;

                    user!.FOCSPoint -= order.DiscountResult.Point;
                    user!.FOCSPoint += (int)order.DiscountResult.TotalPrice / (int)systemConfigEarningRate;

                    await _userManager.UpdateAsync(user);
                }

                table.Status = TableStatus.Occupied;
                _tableRepository.Update(table);

                await _orderRepository.AddAsync(orderCreate);

                await _orderDetailRepository.AddRangeAsync(ordersDetailCreate);
                await _tableRepository.SaveChangesAsync();

                //code order for payment hook
                order.DiscountResult.OrderCode = orderCreate.OrderCode;

                var tokenDeviceMobile = await _mobileTokenService.GetMobileToken(Guid.Parse(userId));

                //send notify to casher
                var notifyEventModel = new NotifyEvent
                {
                    Title = Constants.ActionTitle.NewOrderd,
                    Message = Constants.ActionTitle.NewOrderAtTable(table.TableNumber),
                    TargetGroups = new[] { SignalRGroups.Cashier(store.Id, table.Id) },
                    MobileTokens = new[] { tokenDeviceMobile.Token }, 
                    storeId = store.Id.ToString(),
                    tableId = table.Id.ToString()
                };
                
                await _publishEndpoint.Publish(notifyEventModel);

                var orderDataExchangeRealtime = order.Items
                        .SelectMany(item => item.Variants.Select(variant => new OrderRedisModel
                        {
                            MenuItemId = item.MenuItemId,
                            VariantId = variant.VariantId,
                            Quantity = item.Quantity,
                            Note = item.Note
                        }))
                        .ToList();

                await _realtimeService.SendToGroupAsync<OrderHub, List<OrderRedisModel>>(
                    SignalRGroups.User(store.Id, table.Id, Guid.Parse(userId)),
                    Constants.Method.OrderCreated,
                    orderDataExchangeRealtime
                );

            }
            catch (Exception ex)
            {
                _logger.LogError("Error when saving order: " + ex.Message);
            }
        }

        public async Task<bool> DeleteOrderAsync(Guid orderId, string userId, string storeId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);

                ConditionCheck.CheckCondition(order != null, Errors.Common.NotFound);

                var orderDetails = _orderDetailRepository.AsQueryable().Where(x => x.OrderId == order.Id).ToList();

                if(orderDetails.Any() && orderDetails != null)
                {
                    _orderDetailRepository.RemoveRange(orderDetails);
                }

                _orderRepository.Remove(order);
                await _orderRepository.SaveChangesAsync();

                return true;
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, string userId, string storeId)
        {
            try
            {

                var order = await _orderRepository.GetByIdAsync(orderId);

                ConditionCheck.CheckCondition(order != null, Errors.Common.NotFound);

                order!.OrderStatus = OrderStatus.Canceled;
                order!.UpdatedAt = DateTime.UtcNow;

                _orderRepository.Update(order);
                await _orderRepository.SaveChangesAsync();

                return true;
            } catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        private async Task ValidateMenuItemsAsync(IEnumerable<OrderItemDTO> items)
        {
            var menuItemIds = items.Select(i => i.MenuItemId).ToList();
            var variantIds = items
                .SelectMany(i => i.Variants ?? new List<CartVariantRedisModel>())
                .Select(v => v.VariantId)
                .Distinct()
                .ToList();

            var existingMenuItems = await _menuItemRepository.FindAsync(x => menuItemIds.Contains(x.Id));
            var existingVariants = await _variantRepository.FindAsync(x => variantIds.Contains(x.Id));

            foreach (var item in items)
            {
                ConditionCheck.CheckCondition(existingMenuItems.Any(x => x.Id == item.MenuItemId), Errors.OrderError.MenuItemNotFound);
                if(item.Variants != null)
                {
                    foreach (var itemVariant in item.Variants)
                    {
                        if (itemVariant.VariantId != null)
                            ConditionCheck.CheckCondition(existingVariants.Any(x => x.Id == itemVariant.VariantId), Errors.OrderError.MenuItemNotFound);
                    }
                }
            }
        }

        private static IQueryable<Order.Infrastucture.Entities.Order> ApplyFilters(
    IQueryable<Order.Infrastucture.Entities.Order> query,
    UrlQueryParameters parameters)
        {
            if (parameters.Filters?.Any() != true)
                return query;

            foreach (var (key, value) in parameters.Filters)
            {
                if (key.Equals("status", StringComparison.OrdinalIgnoreCase))
                {
                    var statusValues = value.Split('-')
                                            .Select(v => v.Trim())
                                            .ToList();

                    var statuses = new List<OrderStatus>();
                    foreach (var sv in statusValues)
                    {
                        if (Enum.TryParse<OrderStatus>(sv, true, out var parsedStatus))
                        {
                            statuses.Add(parsedStatus);
                        }
                    }

                    if (statuses.Any())
                    {
                        query = query.Where(o => statuses.Contains(o.OrderStatus));
                    }
                }
            }

            return query;
        }

        private static IQueryable<Order.Infrastucture.Entities.Order> ApplySearch(IQueryable<Order.Infrastucture.Entities.Order> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SearchBy) || string.IsNullOrWhiteSpace(parameters.SearchValue))
                return query;

            var searchValue = parameters.SearchValue.ToLowerInvariant();

            return parameters.SearchBy.ToLowerInvariant() switch
            {
                //"title" => query.Where(p => p.Title.ToLower().Contains(searchValue)),
                //_ => query
            };
        }

        private static IQueryable<Order.Infrastucture.Entities.Order> ApplySort(IQueryable<Order.Infrastucture.Entities.Order> query, UrlQueryParameters parameters)
        {
            //if (string.IsNullOrWhiteSpace(parameters.SortBy)) return query.OrderBy(p => p.StartDate);

            //var isDescending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            //return parameters.SortBy.ToLowerInvariant() switch
            //{
            //    "title" => isDescending
            //        ? query.OrderByDescending(p => p.Title)
            //        : query.OrderBy(p => p.Title),
            //    "end_date" => isDescending
            //        ? query.OrderByDescending(p => p.EndDate)
            //        : query.OrderBy(p => p.EndDate),
            //    "start_date" => isDescending
            //        ? query.OrderByDescending(p => p.StartDate)
            //        : query.OrderBy(p => p.StartDate),
            //    "promotion_type" => isDescending
            //        ? query.OrderByDescending(p => p.PromotionType)
            //        : query.OrderBy(p => p.PromotionType),
            //    "discount_value" => isDescending
            //        ? query.OrderByDescending(p => p.DiscountValue)
            //        : query.OrderBy(p => p.DiscountValue),
            //    _ => query.OrderBy(p => p.StartDate)
            //};

            return query;
        }


        #endregion
    }
}
