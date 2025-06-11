using FOCS.Application.Services.ApplyStrategy;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
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

        public DiscountContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

            return await strategyInstance.ApplyDiscountAsync(order, couponCode);
        }
    }
}
