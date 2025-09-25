using AutoMapper;
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
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.CodeDom;
using System.Linq;
using static FOCS.Common.Exceptions.Errors;

namespace FOCS.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly IRepository<MenuItemVariant> _variantRepository;
        private readonly IRepository<Table> _tableRepository;
        private readonly IRepository<FOCS.Order.Infrastucture.Entities.Order> _orderRepository;
        private readonly IRepository<Feedback> _feedbackRepository;
        private readonly IRepository<Coupon> _couponRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;

        private readonly IMobileTokenSevice _mobileTokenService;

        private readonly IPricingService _pricingService;

        private readonly INotifyService _notifyService;

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
                            ICouponUsageService couponUsageService,
                            INotifyService notifyService,
                            IRepository<Feedback> feedbackRepository)
        {
            _notifyService = notifyService;
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
            _feedbackRepository = feedbackRepository;
        }

        public async Task<DiscountResultDTO> CreateOrderAsync(CreateOrderRequest order, string userId)
        {
            var store = await _storeRepository.GetByIdAsync(order.StoreId);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound);

            Table? table = null;

            //if (order.OrderType == OrderType.DineIn)
            //{
            table = await _tableRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == order.TableId && x.StoreId == order.StoreId);
            ConditionCheck.CheckCondition(table != null, Errors.OrderError.TableNotFound);
            //}

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
            if (orderRequest.CouponCode == null)
            {
                var rs = new DiscountResultDTO();

                double totalOrderAmount = 0;
                var pricingDict = new Dictionary<(Guid MenuItemId, Guid? VariantId), double>();
                foreach (var item in orderRequest.Items)
                {
                    var basePrice = await _pricingService.GetPriceByProduct(item.MenuItemId, null, orderRequest.StoreId);
                    double totalVariantPrice = 0;

                    if (item.Variants != null && item.Variants.Count > 0)
                    {
                        foreach (var variant in item.Variants)
                        {
                            var variantPrice = await _pricingService.GetPriceByProduct(item.MenuItemId, variant.VariantId, orderRequest.StoreId);
                            double variantTotal = (double)(variantPrice.VariantPrice * variant.Quantity);
                            totalVariantPrice += variantTotal;

                            pricingDict[(item.MenuItemId, variant.VariantId)] = (double)basePrice.ProductPrice + (double)variantPrice.VariantPrice;
                        }

                        rs.SubTotal += (decimal)((double)basePrice.ProductPrice + totalVariantPrice) * item.Quantity;
                    }
                    else
                    {
                        double itemUnitPrice = (double)basePrice.ProductPrice + (double)(basePrice.VariantPrice ?? 0);
                        rs.SubTotal += (decimal)(itemUnitPrice * item.Quantity);

                        pricingDict[(item.MenuItemId, null)] = itemUnitPrice;
                    }
                }

                var s = await _storeRepository.GetByIdAsync(orderRequest.StoreId);
                var tr = (decimal)(s?.CustomTaxRate ?? 0);

                if (orderRequest.IsUseLoyatyPoint.HasValue && orderRequest.IsUseLoyatyPoint == true)
                {
                    if (orderRequest.Point.HasValue && orderRequest.Point > 0)
                    {
                        var user = await _userManager.FindByIdAsync(userId);

                        //if guest
                        if (user != null)
                        {
                            ConditionCheck.CheckCondition(user!.FOCSPoint < orderRequest.Point, Errors.OrderError.NotEnoughPoint);

                            rs.IsUsePoint = true;
                            rs.Point = orderRequest.Point;

                            var spendingRate = (await _storeSettingService.GetStoreSettingAsync(Guid.Parse(storeId)))!.SpendingRate ?? 0;

                            var discountAmountBasedOnPoint = ((decimal)orderRequest.Point * (decimal)spendingRate) * 1000;

                            if (discountAmountBasedOnPoint > rs.TotalPrice)
                            {
                                discountAmountBasedOnPoint = rs.TotalPrice;
                                rs.Point = (int)(discountAmountBasedOnPoint / (spendingRate * 1000));
                            }

                            rs.TotalDiscount += discountAmountBasedOnPoint;
                            rs.TotalPrice = rs.TotalPrice -= discountAmountBasedOnPoint < 0 ? 0 : rs.TotalPrice -= discountAmountBasedOnPoint;

                        }
                        else
                        {
                            rs.IsUsePoint = false;
                            rs.Point = 0;
                        }
                    }
                }

                rs.TaxAmount = Math.Max(Math.Round((rs.SubTotal - rs.TotalDiscount) * tr), 0);
                rs.TotalPrice = Math.Max(rs.SubTotal - rs.TotalDiscount + rs.TaxAmount, 0);
                return rs;
            }

            await _promotionService.IsValidPromotionCouponAsync(orderRequest.CouponCode!, userId.ToString(), orderRequest.StoreId);

            var storeSettings = await _storeSettingService.GetStoreSettingAsync(orderRequest.StoreId);

            ConditionCheck.CheckCondition(storeSettings != null, Errors.Common.NotFound);
            ConditionCheck.CheckCondition(!storeSettings!.DiscountStrategy.Equals(null), Errors.StoreSetting.DiscountStrategyNotConfig);

            var discountResult = await _discountContext.CalculateDiscountAsync(orderRequest, orderRequest.CouponCode, storeSettings.DiscountStrategy, userId);

            var store = await _storeRepository.GetByIdAsync(orderRequest.StoreId);
            var taxRate = (decimal)(store?.CustomTaxRate ?? 0);

            discountResult.TaxAmount = Math.Round((discountResult.SubTotal - discountResult.TotalDiscount) * taxRate);
            discountResult.TotalPrice = discountResult.SubTotal - discountResult.TotalDiscount + discountResult.TaxAmount;

            return discountResult;
        }

        public async Task<PagedResult<OrderDTO>> GetListOrders(UrlQueryParameters queryParameters, string storeId, string userId)
        {
            var ordersQuery = _orderRepository.AsQueryable()
                .Include(x => x.OrderDetails).ThenInclude(x => x.MenuItem)
                .Where(x => x.StoreId == Guid.Parse(storeId)
                         && x.UserId == Guid.Parse(userId)
                         && !x.IsDeleted);

            ordersQuery = ApplyFilters(ordersQuery, queryParameters);
            ordersQuery = ApplySearch(ordersQuery, queryParameters);
            ordersQuery = ApplySort(ordersQuery, queryParameters);

            var total = await ordersQuery.CountAsync();

            var items = await ordersQuery
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var allVariantIds = items
                .SelectMany(o => o.OrderDetails)
                .Where(d => d.Variants != null)
                .SelectMany(d => d.Variants)
                .Distinct()
                .ToList();

            var variants = await _variantRepository.AsQueryable()
                .Where(x => allVariantIds.Contains(x.Id))
                .ToListAsync();

            var variantDict = variants.ToDictionary(x => x.Id, x => x);

            var orderList = _mapper.Map<List<OrderDTO>>(items);

            orderList.ForEach(x => x.IsFeedback = (_feedbackRepository.AsQueryable().Any(f => f.OrderId == x.Id)));

            foreach (var order in orderList)
            {
                foreach (var od in order.OrderDetails)
                {
                    var itemVariants = new List<OrderDetailVariantDTO>();

                    if (od.Variants != null)
                    {
                        foreach (var variantId in od.Variants)
                        {
                            if (variantDict.TryGetValue(variantId.VariantId, out var variant))
                            {
                                itemVariants.Add(new OrderDetailVariantDTO
                                {
                                    VariantId = variant.Id,
                                    VariantName = variant.Name
                                });
                            }
                        }
                    }

                    od.Variants = itemVariants;
                }
            }

            return new PagedResult<OrderDTO>(orderList, total, queryParameters.Page, queryParameters.PageSize);
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
                                        .Where(od => od.Variants != null)
                                        .SelectMany(od => od.Variants)
                                        .Distinct()
                                        .ToList();

            var variants = await _variantRepository.AsQueryable()
                                                   .Where(x => variantIds.Contains(x.Id))
                                                   .ToListAsync();

            var variantDict = variants.ToDictionary(x => x.Id);

            var rs = _mapper.Map<OrderDTO>(orderByCode);

            foreach (var od in rs.OrderDetails)
            {
                var itemVariants = new List<OrderDetailVariantDTO>();

                if (od.Variants != null)
                {
                    foreach (var variantId in od.Variants)
                    {
                        if (variantDict.TryGetValue(variantId.VariantId, out var variant))
                        {
                            itemVariants.Add(new OrderDetailVariantDTO
                            {
                                VariantId = variant.Id,
                                VariantName = variant.Name
                            });
                        }
                    }
                }

                od.Variants = itemVariants;
            }

            return rs;
        }


        public async Task<bool> ChangeStatusOrder(string code, ChangeOrderStatusRequest request, string storeId)
        {
            try
            {
                var order = await _orderRepository.AsQueryable().FirstOrDefaultAsync(x => x.OrderCode.ToString() == code && !x.IsDeleted && x.StoreId == Guid.Parse(storeId));

                ConditionCheck.CheckCondition(order != null, Errors.Common.NotFound);

                order.OrderStatus = request.OrderStatus;

                if (order.OrderStatus == OrderStatus.Confirmed)
                {
                    order.PaymentStatus = PaymentStatus.Paid;

                    await MarkAsPaid(order.OrderCode, storeId);
                }

                order.UpdatedAt = DateTime.UtcNow;

                _orderRepository.Update(order);
                await _orderRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<OrderDTO>> GetPendingOrdersInDayAsync()
        {
            var timeSince = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));

            var ordersPending = await _orderRepository.AsQueryable()
                                                      .Include(x => x.OrderDetails)
                                                      .Where(x => x.OrderStatus == OrderStatus.Confirmed
                                                              && !x.IsDeleted
                                                              && x.PaymentStatus == PaymentStatus.Paid
                                                              && x.CreatedAt >= timeSince)
                                                      .ToListAsync();

            if (!ordersPending.Any())
                return new List<OrderDTO>();

            var allVariantIds = ordersPending
                .SelectMany(o => o.OrderDetails)
                .Where(d => d.Variants != null)
                .SelectMany(d => d.Variants)
                .Distinct()
                .ToList();

            var variants = await _variantRepository.AsQueryable()
                                                   .Where(x => allVariantIds.Contains(x.Id))
                                                   .ToListAsync();

            var variantDict = variants.ToDictionary(x => x.Id, x => x);

            var mappingOrders = _mapper.Map<List<OrderDTO>>(ordersPending);

            foreach (var od in mappingOrders)
            {
                foreach (var detail in od.OrderDetails)
                {
                    var itemVariants = new List<OrderDetailVariantDTO>();

                    if (detail.Variants != null)
                    {
                        foreach (var variantId in detail.Variants)
                        {
                            if (variantDict.TryGetValue(variantId.VariantId, out var foundVariant))
                            {
                                itemVariants.Add(new OrderDetailVariantDTO
                                {
                                    VariantId = foundVariant.Id,
                                    VariantName = foundVariant.Name
                                });
                            }
                        }
                    }

                    detail.Variants = itemVariants;
                }
            }

            ordersPending.ForEach(x => x.OrderStatus = OrderStatus.Ready);
            _orderRepository.UpdateRange(ordersPending);
            await _orderRepository.SaveChangesAsync();

            foreach (var dto in mappingOrders)
            {
                foreach (var detail in dto.OrderDetails)
                {
                    var menuItem = await _menuItemRepository.GetByIdAsync(detail.MenuItemId);
                    detail.MenuItemName = menuItem?.Name;
                }
            }

            return mappingOrders;
        }

        public async Task<OrderDTO> GetUserOrderDetailAsync(Guid userId, Guid orderId)
        {
            var orderByUser = await _orderRepository.AsQueryable()
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == orderId);

            ConditionCheck.CheckCondition(orderByUser != null, Errors.Common.NotFound);

            var allVariantIds = orderByUser.OrderDetails
                .Where(d => d.Variants != null)
                .SelectMany(d => d.Variants)
                .Distinct()
                .ToList();

            var variants = await _variantRepository.AsQueryable()
                .Where(x => allVariantIds.Contains(x.Id))
                .ToListAsync();

            var variantDict = variants.ToDictionary(x => x.Id, x => x);

            var rs = _mapper.Map<OrderDTO>(orderByUser);

            foreach (var od in rs.OrderDetails)
            {
                var itemVariants = new List<OrderDetailVariantDTO>();

                if (od.Variants != null)
                {
                    foreach (var variantId in od.Variants)
                    {
                        if (variantDict.TryGetValue(variantId.VariantId, out var variant))
                        {
                            itemVariants.Add(new OrderDetailVariantDTO
                            {
                                VariantId = variant.Id,
                                VariantName = variant.Name
                            });
                        }
                    }
                }

                od.Variants = itemVariants;
            }

            return rs;
        }

        public async Task MarkAsPaid(long orderCode, string storeId)
        {
            var order = await _orderRepository.AsQueryable().Include(x => x.Table).Include(x => x.Coupon).FirstOrDefaultAsync(x => x.OrderCode == orderCode);

            //update coupon, promotion usage
            var storeSetting = await _storeSettingService.GetStoreSettingAsync(Guid.Parse(storeId));

            if (order != null)
            {
                var user = await _userManager.FindByIdAsync(order.UserId.ToString());

                if (user != null)
                {
                    var spendingRate = storeSetting.SpendingRate ?? 0;
                    var systemConfigEarningRate = (await _systemConfig.AsQueryable().FirstOrDefaultAsync())!.EarningRate;

                    if (user!.FOCSPoint <= 0)
                    {
                        user!.FOCSPoint += (int)(order.TotalAmount * systemConfigEarningRate) / 1000;
                    }
                    else
                    {
                        user!.FOCSPoint -= order.PointUsed ?? 0;
                        if (user!.FOCSPoint < 0)
                            user!.FOCSPoint = 0;

                        user!.FOCSPoint += (int)(order.TotalAmount * systemConfigEarningRate) / 1000;
                    }

                    //user.FOCSPoint += (int)(order.TotalAmount * spendingRate);

                    await _userManager.UpdateAsync(user);
                }
            }

            ConditionCheck.CheckCondition(order != null, Errors.OrderError.OrderNotFound);

            if (storeSetting.DiscountStrategy == DiscountStrategy.CouponThenPromotion)
            {
                try
                {
                    var currentCoupon = await _couponRepository.AsQueryable().Include(x => x.Promotion).FirstOrDefaultAsync(x => x.Code == orderCode.ToString() && !x.IsDeleted);
                    if (currentCoupon != null)
                    {
                        currentCoupon.CountUsed++;
                        var isAdded = await _couponUsageService.SaveCouponUsage(currentCoupon.Code, order.UserId, order.Id);

                        if (isAdded)
                        {
                            if (currentCoupon.Promotion != null)
                            {
                                currentCoupon.Promotion.CountUsed++;
                            }
                        }

                        _couponRepository.Update(currentCoupon);
                        await _couponRepository.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    return;
                }
            }
            else
            {
                if (order != null && order.Coupon != null)
                {
                    var currentCoupon = await _couponRepository.AsQueryable().FirstOrDefaultAsync(x => x.Code == order.Coupon.Code.ToString() && !x.IsDeleted);
                    if (currentCoupon != null)
                    {
                        currentCoupon.CountUsed++;
                        var isAdded = await _couponUsageService.SaveCouponUsage(currentCoupon.Code, order.UserId, order.Id);

                        _couponRepository.Update(currentCoupon);
                        await _couponRepository.SaveChangesAsync();
                    }
                }
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.OrderStatus = OrderStatus.Confirmed;

            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();

            _logger.LogInformation("SAVE order success - trigger success");

            if (order.TableId != null)
            {
                var notifyEvent = new NotifyEvent
                {
                    Title = Constants.ActionTitle.PaymentSuccess(order.Table.TableNumber),
                    Message = Constants.ActionTitle.PaymentSuccess(order.Table.TableNumber),
                    TargetGroups = new[] { SignalRGroups.Cashier(order.StoreId, (Guid)order.TableId) },
                    storeId = order.StoreId.ToString(),
                    tableId = order.TableId.ToString()
                };

                await _publishEndpoint.Publish(notifyEvent);

                await _notifyService.AddNotifyAsync(order.StoreId.ToString(), Constants.ActionTitle.PaymentSuccess(order.Table.TableNumber));

                await _realtimeService.SendToGroupAsync<NotifyHub, NotifyEvent>(SignalRGroups.Cashier(order.StoreId, (Guid)order.TableId), Constants.Method.NewNotify, notifyEvent);
            }
            else
            {
                _logger.LogInformation("Not found table in Order Handle payment success");
            }
        }

        #region private methods
        private async Task SaveOrderAsync(CreateOrderRequest order, Table? table, Store store, string userId)
        {
            Random randomNum = new Random();

            try
            {
                Guid? couponCurrent = null;
                IEnumerable<Coupon> coupon = null;
                if (!string.IsNullOrEmpty(order.DiscountResult.AppliedCouponCode))
                {
                    //coupon = await _couponRepository.FindAsync(x => x.Code == order.DiscountResult.AppliedCouponCode);
                    coupon = await _couponRepository.AsQueryable().Include(x => x.Promotion).Where(x => x.Code == order.DiscountResult.AppliedCouponCode).ToListAsync();
                    couponCurrent = coupon.FirstOrDefault()?.Id;
                }

                //remaining time for order
                int remainingTimeOrder = 0;
                {
                    var dictProduct = order.Items
                         .GroupBy(item => item.MenuItemId)
                         .ToDictionary(
                             g => g.Key,
                             g => g.SelectMany(x => x.Variants).Select(v => v.VariantId).ToList()
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
                    SubTotalAmout = (double)order.DiscountResult.SubTotal,
                    TaxAmount = (double)order.DiscountResult.TaxAmount,
                    DiscountAmount = (double)order.DiscountResult.TotalDiscount,
                    TotalAmount = (double)order.DiscountResult.TotalPrice,
                    CustomerNote = order.Note ?? "",
                    StoreId = order.StoreId,
                    PointUsed = order.DiscountResult.Point,
                    CouponId = couponCurrent,
                    PaymentStatus = order.PaymentType switch
                    {
                        PaymentType.CASH => PaymentStatus.Unpaid,
                        PaymentType.BANK_TRANSFER or PaymentType.ONLINE_PAYMENT => PaymentStatus.Waiting,
                        _ => PaymentStatus.Unpaid
                    },
                    CreatedBy = userId,
                    TableId = order.OrderType == OrderType.DineIn ? order.TableId : null,
                    RemainingTime = TimeSpan.FromMinutes(remainingTimeOrder),
                    SourceDiscount = order.DiscountResult.Messages ?? new List<string>()
                };

                var ordersDetailCreate = new List<OrderDetail>();

                if (order.DiscountResult?.ItemDiscountDetails != null && order.DiscountResult.ItemDiscountDetails.Any() && coupon?.FirstOrDefault()?.Promotion?.PromotionType != PromotionType.BuyXGetY)
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

                        var variants = await _variantRepository.AsQueryable().Where(x => variantIds.Contains(x.Id)).ToListAsync();

                        ordersDetailCreate.AddRange(variantIds.Select(x => new OrderDetail
                        {
                            Id = Guid.NewGuid(),
                            Quantity = item.Quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = unitPrice - (double)item.DiscountAmount,
                            Note = order.Note ?? "",
                            MenuItemId = productId,
                            Variants = variants.Select(x => x.Id).Distinct().ToList(),
                            OrderId = orderCreate.Id
                        }).ToList());
                    }
                }
                else
                {
                    foreach (var item in order.Items)
                    {
                        double price = 0;
                        var currentProductPrice = await _pricingService.GetPriceByProduct(item.MenuItemId, null, order.StoreId);
                        double unitPrice = currentProductPrice.ProductPrice;
                        if (item.Variants != null)
                        {
                            foreach (var itemVariant in item.Variants)
                            {
                                var currentPrice = await _pricingService.GetPriceByProduct(item.MenuItemId, itemVariant.VariantId, order.StoreId);
                                unitPrice += currentPrice.VariantPrice ?? 0;
                            }

                            var variants = await _variantRepository.AsQueryable().Where(x => item.Variants.Select(x => x.VariantId).Contains(x.Id)).ToListAsync();

                            ordersDetailCreate.Add(new OrderDetail
                            {
                                Id = Guid.NewGuid(),
                                Quantity = item.Quantity,
                                UnitPrice = unitPrice,
                                TotalPrice = unitPrice * item.Quantity,
                                Note = item.Note ?? "",
                                MenuItemId = item.MenuItemId,
                                Variants = variants.Select(x => x.Id).Distinct().ToList(),
                                OrderId = orderCreate.Id
                            });
                        }
                        else
                        {
                            ordersDetailCreate.Add(new OrderDetail
                            {
                                Id = Guid.NewGuid(),
                                Quantity = item.Quantity,
                                UnitPrice = unitPrice,
                                TotalPrice = unitPrice * item.Quantity,
                                Note = item.Note ?? "",
                                MenuItemId = item.MenuItemId,
                                Variants = new List<Guid>(),
                                OrderId = orderCreate.Id
                            });
                        }
                    }
                }


                //if (order.DiscountResult.IsUsePoint.HasValue && order.DiscountResult.IsUsePoint == true)
                //{
                //    var user = await _userManager.FindByIdAsync(userId);
                //    ConditionCheck.CheckCondition(user != null, Errors.Common.NotFound);

                //    var systemConfigEarningRate = (await _systemConfig.AsQueryable().FirstOrDefaultAsync())!.EarningRate;

                //    user!.FOCSPoint -= order.DiscountResult.Point;
                //    user!.FOCSPoint += (int)order.DiscountResult.TotalPrice / (int)systemConfigEarningRate;

                //    await _userManager.UpdateAsync(user);
                //}

                table.Status = TableStatus.Occupied;
                _tableRepository.Update(table);

                await _orderRepository.AddAsync(orderCreate);

                await _orderDetailRepository.AddRangeAsync(ordersDetailCreate);
                await _tableRepository.SaveChangesAsync();

                //code order for payment hook
                order.DiscountResult.OrderCode = orderCreate.OrderCode;

                var tokenDeviceMobile = await _mobileTokenService.GetMobileToken(Guid.Parse(userId));

                if (tokenDeviceMobile.Token != null)
                {
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
                    await _notifyService.AddNotifyAsync(order.StoreId.ToString(), Constants.ActionTitle.NewOrderAtTable(table.TableNumber));
                }

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

        public async Task<bool> DeleteOrderAsync(string code, string userId, string storeId)
        {
            try
            {
                var order = await _orderRepository.AsQueryable().FirstOrDefaultAsync(x => x.OrderCode == long.Parse(code));

                ConditionCheck.CheckCondition(order != null, Errors.Common.NotFound);

                var orderDetails = _orderDetailRepository.AsQueryable().Where(x => x.OrderId == order.Id).ToList();

                if (orderDetails.Any() && orderDetails != null)
                {
                    _orderDetailRepository.RemoveRange(orderDetails);
                }

                _orderRepository.Remove(order);
                await _orderRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
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
            }
            catch (Exception ex)
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
                if (item.Variants != null)
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
