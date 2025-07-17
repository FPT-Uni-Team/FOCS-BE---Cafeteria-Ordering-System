using FOCS.Application.Services.Interface;
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services.ApplyStrategy
{
    public class CouponThenPromotionStrategy : IDiscountStrategyService
    {
        private readonly CouponOnlyStrategy _couponOnlyStrategy;
        private readonly PromotionOnlyStrategy _promotionOnlyStrategy;

        public CouponThenPromotionStrategy(CouponOnlyStrategy couponOnlyStrategy, PromotionOnlyStrategy promotionOnlyStrategy)
        {
            _couponOnlyStrategy = couponOnlyStrategy;
            _promotionOnlyStrategy = promotionOnlyStrategy;
        }

        public async Task<DiscountResultDTO> ApplyDiscountAsync(ApplyDiscountOrderRequest order, string? couponCode = null)
        {
            var discountApplyCoupon = await _couponOnlyStrategy.ApplyDiscountAsync(order, couponCode);

            var discountApplyPromotion = await _promotionOnlyStrategy.ApplyDiscountAsync(order, couponCode);

            return new DiscountResultDTO
            {
                AppliedCouponCode = couponCode,
                AppliedPromotions = discountApplyCoupon.AppliedPromotions
                                    .Concat(discountApplyPromotion.AppliedPromotions)
                                    .Distinct()
                                    .ToList(),
                TotalDiscount = discountApplyCoupon.TotalDiscount + discountApplyPromotion.TotalDiscount,
                TotalPrice = discountApplyCoupon.TotalPrice - discountApplyPromotion.TotalDiscount,
                ItemDiscountDetails = discountApplyCoupon.ItemDiscountDetails
                                    .Concat(discountApplyPromotion.ItemDiscountDetails)
                                    .ToList(),
                Messages = discountApplyCoupon.Messages
                                    .Concat(discountApplyPromotion.Messages)
                                    .ToList()
            };

        }
    }
}
