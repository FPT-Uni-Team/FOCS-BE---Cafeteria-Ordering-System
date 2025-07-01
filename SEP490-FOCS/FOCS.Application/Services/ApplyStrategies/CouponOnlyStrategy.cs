using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Order.Infrastucture.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services.ApplyStrategy
{
    public class CouponOnlyStrategy : IDiscountStrategyService
    {
        private readonly IRepository<Coupon> _couponRepository;
        private readonly IMenuService _menuService;

        private readonly IPricingService _pricingService;

        public CouponOnlyStrategy(IRepository<Coupon> couponRepository, IMenuService menuService, IPricingService pricingService)
        {
            _couponRepository = couponRepository;
            _menuService = menuService;
            _pricingService = pricingService;
        }

        public async Task<DiscountResultDTO> ApplyDiscountAsync(CreateOrderRequest order, string? couponCode = null)
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

            result.AppliedCouponCode = coupon.Code;

            double totalOrderAmount = 0;
            var pricingDict = new Dictionary<(Guid MenuItemId, Guid? VariantId), double>();

            foreach (var item in order.Items)
            {
                var price = await _pricingService.GetPriceByProduct(item.MenuItemId, item.VariantId, order.StoreId);
                double itemUnitPrice = price.ProductPrice + (price.VariantPrice ?? 0);
                double itemTotalPrice = itemUnitPrice * item.Quantity;

                pricingDict[(item.MenuItemId, item.VariantId)] = itemUnitPrice;
                totalOrderAmount += itemTotalPrice;
                result.TotalPrice += (decimal)itemTotalPrice;
            }

            HashSet<Guid>? acceptedItems = coupon.AcceptForItems?.Select(Guid.Parse).ToHashSet();

            // Case 1: Order level discount
            if (coupon.MinimumOrderAmount.HasValue && !coupon.MinimumItemQuantity.HasValue && totalOrderAmount >= coupon.MinimumOrderAmount)
            {
                double discountAmount = CalculateDiscount(coupon.DiscountType, coupon.Value, totalOrderAmount);
                result.TotalDiscount = (decimal)discountAmount;
                result.TotalPrice -= result.TotalDiscount;

                foreach (var item in order.Items)
                {
                    result.ItemDiscountDetails.Add(new DiscountItemDetail
                    {
                        DiscountAmount = 0,
                        ItemCode = GenerateItemCode(item),
                        ItemName = item.MenuItemId.ToString(),
                        Quantity = item.Quantity,
                        Source = CouponConstants.Coupon_MinimumOrderAmount.ToString()
                    });
                }

                return result;
            }

            // Case 2 & 3: Item-based discounts
            double totalDiscount = 0;
            foreach (var item in order.Items)
            {
                bool isItemAccepted = acceptedItems == null || acceptedItems.Contains(item.MenuItemId);
                if (!isItemAccepted)
                    continue;

                if (coupon.MinimumItemQuantity.HasValue && item.Quantity < coupon.MinimumItemQuantity)
                    continue;

                var key = (item.MenuItemId, item.VariantId);
                if (!pricingDict.TryGetValue(key, out double unitPrice)) continue;

                double itemDiscount = CalculateDiscount(coupon.DiscountType, coupon.Value, unitPrice) * item.Quantity;
                totalDiscount += itemDiscount;

                result.ItemDiscountDetails.Add(new DiscountItemDetail
                {
                    DiscountAmount = (decimal)itemDiscount,
                    ItemCode = GenerateItemCode(item),
                    ItemName = item.MenuItemId.ToString(),
                    Quantity = item.Quantity,
                    Source = $"Coupon_{coupon.DiscountType}"
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
            return item.VariantId != null
                ? $"{item.MenuItemId}_{item.VariantId}"
                : $"{item.MenuItemId}";
        }

    }
}
