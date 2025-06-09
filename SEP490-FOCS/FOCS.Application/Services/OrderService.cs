using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Org.BouncyCastle.Crypto.Modes.Gcm;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly IRepository<MenuItemVariant> _variantRepository;
        private readonly IRepository<Table> _tableRepository;
        private readonly IRepository<FOCS.Order.Infrastucture.Entities.Order> _orderRepository;

        private readonly IPromotionService _promotionService;
        private readonly DiscountContext _discountContext;
        private readonly IStoreSettingService _storeSettingService;

        public OrderService(IRepository<FOCS.Order.Infrastucture.Entities.Order> orderRepository, DiscountContext discountContext, IStoreSettingService storeSettingService, IRepository<Table> tableRepo, IRepository<Store> storeRepository, IRepository<MenuItem> menuRepository, IRepository<MenuItemVariant> variantRepository, IPromotionService promotionService)
        {
            _orderRepository = orderRepository;
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
            var store = await _storeRepository.FindAsync(x => x.Id == order.StoreId);
            ConditionCheck.CheckCondition(store != null, Errors.OrderError.NotFoundStore);

            var tableInStore = await _tableRepository.FindAsync(x => x.StoreId == store.FirstOrDefault().Id && x.Id == order.TableId);
            ConditionCheck.CheckCondition(tableInStore.Count() < 1 || tableInStore.Count() > 1 || tableInStore.FirstOrDefault() != null, Errors.OrderError.TableNotFound);

            foreach(var item in order.Items)
            {
                var currentItem = await _menuItemRepository.FindAsync(x => x.Id == item.MenuItemId);
                ConditionCheck.CheckCondition(currentItem.Any(), Errors.OrderError.MenuItemNotFound);

                if (item.VariantId.HasValue)
                {
                    var currentVariant = await _variantRepository.FindAsync(x => x.Id == item.VariantId);
                    ConditionCheck.CheckCondition(currentVariant.Any(), Errors.OrderError.MenuItemNotFound);
                }
            }

            var storeSettings = await _storeSettingService.GetStoreSettingAsync(order.StoreId); 
            ConditionCheck.CheckCondition(storeSettings != null, Errors.Common.StoreNotFound);

            // Validate promotion and coupon
            await _promotionService.IsValidPromotionCouponAsync(order.CouponCode, userId, order.StoreId);

            // Pricing
            ConditionCheck.CheckCondition(storeSettings.DiscountStrategy.HasValue, Errors.StoreSetting.DiscountStrategyNotConfig);
            var discountResult = await _discountContext.CalculateDiscountAsync(order, order.CouponCode, (DiscountStrategy)storeSettings.DiscountStrategy);

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

        #endregion
    }
}
