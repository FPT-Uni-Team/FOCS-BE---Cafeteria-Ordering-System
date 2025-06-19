using FOCS.Application.Services.ApplyStrategy;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class DiscountContext
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly IStoreSettingService _storeSettingService;
        private readonly IPromotionService _promotionService;
        public DiscountContext(IServiceProvider serviceProvider, IStoreSettingService storeSettingService, IPromotionService promotionService)
        {
            _serviceProvider = serviceProvider;
            _storeSettingService = storeSettingService;
            _promotionService = promotionService;
        }

        public async Task<DiscountResultDTO> CalculateDiscountAsync(CreateOrderRequest order, string? couponCode, DiscountStrategy discountStrategy)
        {
            IDiscountStrategyService strategyInstance = discountStrategy switch
            {
                DiscountStrategy.CouponOnly => _serviceProvider.GetRequiredService<CouponOnlyStrategy>(),
                DiscountStrategy.PromotionOnly => _serviceProvider.GetRequiredService<PromotionOnlyStrategy>(),
                DiscountStrategy.CouponThenPromotion => _serviceProvider.GetRequiredService<CouponThenPromotionStrategy>(),
                DiscountStrategy.MaxDiscountOnly => _serviceProvider.GetRequiredService<MaxDiscountOnlyStrategy>(),
                _ => throw new NotImplementedException()
            };

            var storeSetting = await _storeSettingService.GetStoreSettingAsync(order.StoreId);

            if (storeSetting.AllowCombinePromotionAndCoupon)
            {
                var discountStrategyResult = await strategyInstance.ApplyDiscountAsync(order, couponCode);
                return await _promotionService.ApplyEligiblePromotions(discountStrategyResult);
            }

            return await strategyInstance.ApplyDiscountAsync(order, couponCode);
        }
    }
}
