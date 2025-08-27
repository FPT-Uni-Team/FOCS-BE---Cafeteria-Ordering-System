using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Order.Infrastucture.Interfaces;

namespace FOCS.Application.Services.ApplyStrategy
{
    public class CouponOnlyStrategy : IDiscountStrategyService
    {
        private readonly IRepository<Coupon> _couponRepository;
        private readonly IMenuService _menuService;

        private readonly IPricingService _pricingService;
        private readonly IRepository<CouponUsage> _couponUsageRepo;

        public CouponOnlyStrategy(IRepository<Coupon> couponRepository, IRepository<CouponUsage> couponUsageRepo, IMenuService menuService, IPricingService pricingService)
        {
            _couponRepository = couponRepository;
            _menuService = menuService;
            _pricingService = pricingService;
            _couponUsageRepo = couponUsageRepo;
        }

        public async Task<DiscountResultDTO> ApplyDiscountAsync(ApplyDiscountOrderRequest order, string? couponCode = null)
        {
            var result = new DiscountResultDTO
            {
                ItemDiscountDetails = new(),
                AppliedPromotions = new(),
                TotalPrice = 0
            };

            if (string.IsNullOrEmpty(couponCode))
                return result;

            var coupon = (await _couponRepository.FindAsync(x => x.Code == couponCode && x.StoreId == order.StoreId))?.FirstOrDefault();
            if (coupon == null)
                return result;

            if (coupon.MinimumItemQuantity.HasValue && order.Items.Sum(i => i.Quantity) < coupon.MinimumItemQuantity.Value)
                return result;

            result.AppliedCouponCode = coupon.Code;

            double totalOrderAmount = 0;
            var pricingDict = new Dictionary<Guid, double>();

            foreach (var item in order.Items)
            {
                var basePrice = await _pricingService.GetPriceByProduct(item.MenuItemId, null, order.StoreId);
                double totalVariantPrice = 0;

                if (item.Variants != null && item.Variants.Count > 0)
                {
                    foreach (var variant in item.Variants)
                    {
                        var variantPrice = await _pricingService.GetPriceByProduct(item.MenuItemId, variant.VariantId, order.StoreId);
                        totalVariantPrice += (double)variantPrice.VariantPrice * variant.Quantity;
                    }
                }

                pricingDict[item.MenuItemId] = ((double)basePrice.ProductPrice + totalVariantPrice) * item.Quantity;
                result.TotalPrice += (decimal)pricingDict[item.MenuItemId];
                totalOrderAmount += pricingDict[item.MenuItemId];
            }

            if (coupon.MinimumOrderAmount.HasValue && (double)result.TotalPrice < coupon.MinimumOrderAmount.Value)
                return result;

            HashSet<Guid>? acceptedItems = coupon.AcceptForItems?.Select(Guid.Parse).ToHashSet();

            if (acceptedItems == null || acceptedItems?.Count == 0)
            {
                result.TotalDiscount = (decimal)CalculateDiscount(coupon.DiscountType, coupon.Value, (double)result.TotalPrice);
                result.TotalPrice -= result.TotalDiscount;
                return result;
            }

            double totalDiscount = 0;
            foreach (var item in order.Items)
            {
                if (!acceptedItems.Contains(item.MenuItemId))
                    continue;

                if (!pricingDict.TryGetValue(item.MenuItemId, out double unitPrice)) continue;
                double itemDiscount = CalculateDiscount(coupon.DiscountType, coupon.Value, unitPrice)  * item.Quantity;
                totalDiscount += itemDiscount;

                result.ItemDiscountDetails.Add(new DiscountItemDetail
                {
                    DiscountAmount = (decimal)itemDiscount,
                    BuyItemCode = GenerateItemCode(item),
                    BuyItemName = item.MenuItemId.ToString(),
                    Quantity = item.Quantity,
                    Source = $"Coupon {coupon.DiscountType}"
                });
            }

            result.TotalDiscount = (decimal)totalDiscount;
            result.TotalPrice -= result.TotalDiscount;

            return result;
        }


        private static double CalculateDiscount(DiscountType type, double value, double baseAmount)
        {
            return type switch
            {
                DiscountType.Percent => Math.Round(baseAmount * value / 100, 2),
                DiscountType.FixedAmount => Math.Min(baseAmount, value),
                _ => 0
            };
        }

        private static string GenerateItemCode(OrderItemDTO item)
        {
            return item.Variants != null
                ? $"{item.MenuItemId}_{string.Join("_", item.Variants.Select(x => x.VariantId))}"
                : $"{item.MenuItemId}";
        }

    }
}
