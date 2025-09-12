using FOCS.Application.Services.Interface;
using FOCS.Common.Models;

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
                                    .Concat(discountApplyPromotion.AppliedPromotions ?? Enumerable.Empty<string>())
                                    .Distinct()
                                    .ToList(),
                TotalDiscount = discountApplyCoupon.TotalDiscount + discountApplyPromotion.TotalDiscount,
                SubTotal = discountApplyCoupon.SubTotal,
                ItemDiscountDetails = discountApplyCoupon.ItemDiscountDetails
                                    .Concat(discountApplyPromotion.ItemDiscountDetails ?? Enumerable.Empty<DiscountItemDetail>())
                                    .ToList(),
                Messages = discountApplyCoupon.Messages
                                    .Concat(discountApplyPromotion.Messages ?? Enumerable.Empty<string>())
                                    .ToList()
            };

        }
    }
}
