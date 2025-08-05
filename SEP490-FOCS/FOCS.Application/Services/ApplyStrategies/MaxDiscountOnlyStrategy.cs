using FOCS.Application.Services.Interface;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services.ApplyStrategy
{
    public class MaxDiscountOnlyStrategy : IDiscountStrategyService
    {
        private readonly IRepository<Coupon> _couponRepository;
        private readonly IRepository<Promotion> _promotionRepository;

        private readonly CouponOnlyStrategy _couponOnlyStrategy;
        private readonly PromotionOnlyStrategy _promotionOnlyStrategy;

        private readonly IPricingService _pricingService;

        public MaxDiscountOnlyStrategy(IRepository<Coupon> couponRepository, PromotionOnlyStrategy promotionOnlyStrategy, CouponOnlyStrategy couponOnlyStrategy, IPricingService pricingService, IRepository<Promotion> promotionRepository)
        {
            _couponRepository = couponRepository;
            _promotionRepository = promotionRepository;
            _couponOnlyStrategy = couponOnlyStrategy;
            _promotionOnlyStrategy = promotionOnlyStrategy;
            _pricingService = pricingService;
        }

        public async Task<DiscountResultDTO> ApplyDiscountAsync(ApplyDiscountOrderRequest order, string? couponCode = null)
        {
            var discountApplyCoupon = await _couponOnlyStrategy.ApplyDiscountAsync(order, couponCode);
            var discountApplyPromotion = await _promotionOnlyStrategy.ApplyDiscountAsync(order, couponCode);

            if (discountApplyCoupon.TotalDiscount >= discountApplyPromotion.TotalDiscount)
            {
                discountApplyCoupon.Messages.Add("MaxDiscountStrategy: Coupon discount is higher or equal, applied.");
                return discountApplyCoupon;
            }
            else
            {
                discountApplyPromotion.Messages.Add("MaxDiscountStrategy: Promotion discount is higher, applied.");
                return discountApplyPromotion;
            }
        }

    }
}
