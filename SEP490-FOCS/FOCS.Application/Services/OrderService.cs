using AutoMapper;
using Azure.Core;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Modes.Gcm;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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

        private readonly IPricingService _pricingService;

        private readonly IPromotionService _promotionService;
        private readonly DiscountContext _discountContext;
        private readonly IRepository<SystemConfiguration> _systemConfig;
        private readonly IStoreSettingService _storeSettingService;

        private readonly IRealtimeService _realtimeService;

        private readonly ILogger<OrderService> _logger;
        private readonly IMapper _mapper;

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
                            IRepository<SystemConfiguration> systemConfig)
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
            _systemConfig = systemConfig;
        }

        public async Task<DiscountResultDTO> CreateOrderAsync(CreateOrderRequest order, string userId)
        {
            var store = await _storeRepository.GetByIdAsync(order.StoreId);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound);

            var tableInStore = await _tableRepository.FindAsync(x => x.StoreId == store.Id && x.Id == order.TableId);
            ConditionCheck.CheckCondition(tableInStore.Count() < 1 || tableInStore.Count() > 1 || tableInStore.FirstOrDefault() != null, Errors.OrderError.TableNotFound);

            // Validate menu items
            await ValidateMenuItemsAsync(order.Items);

            var storeSettings = await _storeSettingService.GetStoreSettingAsync(order.StoreId, userId); 
            ConditionCheck.CheckCondition(storeSettings != null, Errors.Common.StoreNotFound);

            if(order.DiscountResult == null)
            {
                order.DiscountResult = new DiscountResultDTO();

                var dictPrice = order.Items
                   .Distinct()
                   .Select(x => (x.MenuItemId, x.VariantId))
                   .ToDictionary();

                var price = await _pricingService.CalculatePriceOfProducts(dictPrice, store!.Id.ToString());

                order.DiscountResult.TotalPrice = (decimal)price;
            }

            //save order and order detail
            await SaveOrderAsync(order, tableInStore.FirstOrDefault(), store, userId);

            return order.DiscountResult;
        }

        public async Task<DiscountResultDTO> ApplyDiscountForOrder(ApplyDiscountOrderRequest orderRequest, string userId)
        {
            ConditionCheck.CheckCondition(orderRequest.CouponCode != null, Errors.Common.NotFound);

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

        public async Task<OrderDTO> GetOrderByCodeAsync(string orderCode)
        {
            var orderByCode = await _orderRepository.AsQueryable().Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.OrderCode == orderCode);

            return orderCode == null ? new OrderDTO { } : _mapper.Map<OrderDTO>(orderByCode);
        }

        public Task<List<OrderDTO>> GetPendingOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<OrderDTO> GetUserOrderDetailAsync(Guid userId, Guid orderId)
        {
            var orderByUser = await _orderRepository.AsQueryable().Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.UserId == userId && x.Id == orderId);

            ConditionCheck.CheckCondition(orderByUser != null, Errors.Common.NotFound);

            return _mapper.Map<OrderDTO>(orderByUser);
        }

        #region private methods
        private async Task SaveOrderAsync(CreateOrderRequest order, Table table, Store store, string userId)
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

                var orderCreate = new Order.Infrastucture.Entities.Order
                {
                    Id = Guid.NewGuid(),
                    OrderCode = "ORD" + randomNum.Next(1000, 9999),
                    UserId = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
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
                    }
                };

                var ordersDetailCreate = new List<OrderDetail>();
                if(order.DiscountResult.ItemDiscountDetails != null)
                {
                    foreach (var item in order.DiscountResult.ItemDiscountDetails)
                    {
                        var itemCodes = item.ItemCode?.Split("_");
                        if (itemCodes == null || itemCodes.Length == 0)
                            continue;

                        var productId = Guid.Parse(itemCodes[0]);

                        Guid? variantId = null;
                        if (itemCodes.Length > 1 && Guid.TryParse(itemCodes[1], out var parsedVariantId))
                        {
                            variantId = parsedVariantId;
                        }

                        var price = await _pricingService.GetPriceByProduct(productId, variantId, order.StoreId);
                        var unitPrice = (double)(price.ProductPrice + price.VariantPrice)!;

                        ordersDetailCreate.Add(new OrderDetail
                        {
                            Id = Guid.NewGuid(),
                            Quantity = item.Quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = unitPrice - (double)item.DiscountAmount,
                            Note = "",
                            MenuItemId = productId,
                            VariantId = variantId,
                            OrderId = orderCreate.Id
                        });
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
                await _orderRepository.SaveChangesAsync();

                await _orderDetailRepository.AddRangeAsync(ordersDetailCreate);
                await _orderDetailRepository.SaveChangesAsync();

                await _tableRepository.SaveChangesAsync();

                //code order for payment hook
                order.DiscountResult.OrderCode = orderCreate.OrderCode;

                //send notify to casher
                await _realtimeService.SendToGroupAsync<OrderHub, Order.Infrastucture.Entities.Order>(SignalRGroups.Cashier(store.Id, table.Id), Constants.Method.OrderCreated, orderCreate);
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
            var variantIds = items.Where(i => i.VariantId.HasValue).Select(i => i.VariantId.Value).ToList();

            var existingMenuItems = await _menuItemRepository.FindAsync(x => menuItemIds.Contains(x.Id));
            var existingVariants = await _variantRepository.FindAsync(x => variantIds.Contains(x.Id));

            foreach (var item in items)
            {
                ConditionCheck.CheckCondition(existingMenuItems.Any(x => x.Id == item.MenuItemId), Errors.OrderError.MenuItemNotFound);

                if (item.VariantId.HasValue)
                    ConditionCheck.CheckCondition(existingVariants.Any(x => x.Id == item.VariantId), Errors.OrderError.MenuItemNotFound);
            }
        }

        private static IQueryable<Order.Infrastucture.Entities.Order> ApplyFilters(IQueryable<Order.Infrastucture.Entities.Order> query, UrlQueryParameters parameters)
        {
            if (parameters.Filters?.Any() != true) return query;

            foreach (var (key, value) in parameters.Filters)
            {
                query = key.ToLowerInvariant() switch
                {
                  /*  "promotion_type" when Enum.TryParse<PromotionType>(value, true, out var promotionType) =>
                        query.Where(p => p.PromotionType == promotionType),
                    "start_date" => query.Where(p => p.StartDate >= DateTime.Parse(value)),
                    "end_date" => query.Where(p => p.EndDate <= DateTime.Parse(value)),
                    "status" when Enum.TryParse<PromotionStatus>(value, true, out var status) =>
                        status switch
                        {
                            PromotionStatus.Incomming => query.Where(p => p.StartDate > DateTime.UtcNow),
                            PromotionStatus.OnGoing => query.Where(p => p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow),
                            PromotionStatus.Expired => query.Where(p => p.EndDate < DateTime.UtcNow),
                            PromotionStatus.UnAvailable => query.Where(p => p.IsActive == false),
                            _ => query
                        },
                    _ => query*/
                };
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
