using Azure.Core;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Modes.Gcm;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly IStoreSettingService _storeSettingService;

        private readonly ILogger<string> _logger;

        public OrderService(IRepository<FOCS.Order.Infrastucture.Entities.Order> orderRepository, 
                            ILogger<string> logger, 
                            IRepository<OrderDetail> orderDetailRepository, 
                            IPricingService pricingService, 
                            IRepository<Coupon> couponRepository, 
                            DiscountContext discountContext, 
                            IStoreSettingService storeSettingService, 
                            IRepository<Table> tableRepo, 
                            IRepository<Store> storeRepository, 
                            IRepository<MenuItem> menuRepository, 
                            IRepository<MenuItemVariant> variantRepository, 
                            IPromotionService promotionService)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _pricingService = pricingService;
            _orderDetailRepository = orderDetailRepository;
            _couponRepository = couponRepository;
            _storeRepository = storeRepository;
            this._discountContext = discountContext;
            _menuItemRepository = menuRepository;
            _storeSettingService = storeSettingService;
            _variantRepository = variantRepository;
            _promotionService = promotionService;
            _tableRepository = tableRepo;
        }

        public async Task<DiscountResultDTO> CreateOrderAsGuestAsync(CreateOrderRequest order, string userId)
        {
            var store = await _storeRepository.GetByIdAsync(order.StoreId);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound);

            var tableInStore = await _tableRepository.FindAsync(x => x.StoreId == store.Id && x.Id == order.TableId);
            ConditionCheck.CheckCondition(tableInStore.Count() < 1 || tableInStore.Count() > 1 || tableInStore.FirstOrDefault() != null, Errors.OrderError.TableNotFound);

            // Validate menu items
            await ValidateMenuItemsAsync(order.Items);

            var storeSettings = await _storeSettingService.GetStoreSettingAsync(order.StoreId); 
            ConditionCheck.CheckCondition(storeSettings != null, Errors.Common.StoreNotFound);

            // Validate promotion and coupon
            await _promotionService.IsValidPromotionCouponAsync(order.CouponCode, userId, order.StoreId);

            // Pricing
            ConditionCheck.CheckCondition(storeSettings.DiscountStrategy.HasValue, Errors.StoreSetting.DiscountStrategyNotConfig);
            var discountResult = await _discountContext.CalculateDiscountAsync(order, order.CouponCode, (DiscountStrategy)storeSettings.DiscountStrategy);
            
            //save order and order detail
            await SaveOrderAsync(order, discountResult, tableInStore.FirstOrDefault(), store, userId);

            return discountResult;
        }

        public Task<DiscountResultDTO> ApplyCouponAsync(Guid userId, string couponCode, Guid storeId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderResultDTO> CreateOrderAsync(Guid userId, CreateOrderDTO dto)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDetailDTO> GetOrderByCodeAsync(string orderCode)
        {
            throw new NotImplementedException();
        }

        public async Task<List<OrderDTO>> GetPendingOrdersAsync()
        {
            //var ordersPending = await _orderRepository.FindAsync(x => x.OrderStatus == Common.Enums.OrderStatus.Pending)
            return null;
        }

        public Task<OrderDetailDTO> GetUserOrderDetailAsync(Guid userId, Guid orderId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrderSummaryDTO>> GetUserOrdersAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task SubmitFeedbackAsGuestAsync(OrderFeedbackDTO dto)
        {
            throw new NotImplementedException();
        }

        public Task SubmitFeedbackAsync(Guid userId, OrderFeedbackDTO dto)
        {
            throw new NotImplementedException();
        }

        public Task<DiscountResultDTO> VerifyCouponAsGuestAsync(string couponCode, Guid storeId)
        {
            throw new NotImplementedException();
        }


        #region private methods
        private async Task<bool> IsValidApplyCoupon(string? couponCode)
        {
            throw new NotImplementedException();
        }

        private async Task SaveOrderAsync(CreateOrderRequest order, DiscountResultDTO discountResult, Table table, Store store, string userId)
        {
            Random randomNum = new Random();

            try
            {
                Guid? couponCurrent = null;
                if (!string.IsNullOrEmpty(discountResult.AppliedCouponCode))
                {
                    var coupon = await _couponRepository.FindAsync(x => x.Code == discountResult.AppliedCouponCode);
                    couponCurrent = coupon.FirstOrDefault()?.Id;
                }

                var orderCreate = new Order.Infrastucture.Entities.Order
                {
                    Id = Guid.NewGuid(),
                    OrderCode = "ORD" + randomNum.Next(1000, 9999),
                    OrderStatus = OrderStatus.Pending,
                    OrderType = order.OrderType,
                    SubTotalAmout = (double)(discountResult.TotalPrice + discountResult.TotalDiscount),
                    TaxAmount = store.CustomTaxRate ?? 0,
                    DiscountAmount = (double)discountResult.TotalDiscount,
                    TotalAmount = (double)((double)discountResult.TotalPrice + store.CustomTaxRate ?? 0),
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
                foreach (var item in discountResult.ItemDiscountDetails)
                {
                    var itemCodes = item.ItemCode.Split("_");
                    var productId = Guid.Parse(itemCodes[0]);
                    var variantId = Guid.Parse(itemCodes[1]);

                    var price = await _pricingService.GetPriceByProduct(productId, variantId, order.StoreId);
                    var unitPrice = (double)(price.ProductPrice + price.VariantPrice);

                    ordersDetailCreate.Add(new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = unitPrice - (double)item.DiscountAmount,
                        Note = "",
                        Variants = item.ItemCode,
                        OrderId = orderCreate.Id
                    });
                }

                await _orderRepository.AddAsync(orderCreate);
                await _orderRepository.SaveChangesAsync();

                await _orderDetailRepository.AddRangeAsync(ordersDetailCreate);
                await _orderDetailRepository.SaveChangesAsync();

                table.IsOccupied = TableStatus.Occupied;
                _tableRepository.Update(table);
                await _tableRepository.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError("Error when saving order: " + ex.Message);
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


        #endregion
    }
}
