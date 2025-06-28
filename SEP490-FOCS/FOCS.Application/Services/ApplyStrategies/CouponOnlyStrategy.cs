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

            // Case 1: Order level discount
            if (coupon.MinimumOrderAmount.HasValue && totalOrderAmount >= coupon.MinimumOrderAmount)
            {
                double discountAmount = coupon.DiscountType switch
                {
                    DiscountType.Percent => Math.Round(totalOrderAmount * coupon.Value / 100, 2),
                    DiscountType.FixedAmount => Math.Min(totalOrderAmount, coupon.Value),
                    _ => 0
                };

                result.TotalDiscount = (decimal)discountAmount;
                result.TotalPrice -= result.TotalDiscount;

                foreach (var item in order.Items)
                {
                    var itemCode = item.VariantId != null
                        ? $"{item.MenuItemId}_{item.VariantId}"
                        : $"{item.MenuItemId}";

                    result.ItemDiscountDetails.Add(new DiscountItemDetail
                    {
                        DiscountAmount = 0,
                        ItemCode = itemCode,
                        ItemName = $"{item.MenuItemId}", 
                        Quantity = item.Quantity,
                        Source = CouponConstants.Coupon_MinimumOrderAmount.ToString()
                    });
                }

                return result;
            }

            // Case 2: Discount specific items
            HashSet<Guid>? acceptItemIds = null;
            if (coupon.AcceptForItems != null)
            {
                acceptItemIds = coupon.AcceptForItems
                    .Select(Guid.Parse)
                    .ToHashSet();
            }

            double totalDiscount = 0;
            foreach (var item in order.Items)
            {
                bool isItemAccepted = acceptItemIds == null || acceptItemIds.Contains(item.MenuItemId);
                if (!isItemAccepted)
                    continue;

                if (coupon.MinimumItemQuantity.HasValue && item.Quantity < coupon.MinimumItemQuantity)
                    continue;

                var key = (item.MenuItemId, item.VariantId);
                var itemUnitPrice = pricingDict[key];
                double itemDiscountPerUnit = coupon.DiscountType switch
                {
                    DiscountType.Percent => Math.Round(itemUnitPrice * coupon.Value / 100, 2),
                    DiscountType.FixedAmount => Math.Min(itemUnitPrice, coupon.Value),
                    _ => 0
                };

                double totalItemDiscount = itemDiscountPerUnit * item.Quantity;
                totalDiscount += totalItemDiscount;

                result.ItemDiscountDetails.Add(new DiscountItemDetail
                {
                    DiscountAmount = (decimal)totalItemDiscount,
                    ItemCode = $"{item.MenuItemId}_{item.VariantId}",
                    ItemName = item.MenuItemId.ToString(),
                    Quantity = item.Quantity,
                    Source = $"Coupon_{coupon.DiscountType}"
                });
            }

            result.TotalDiscount = (decimal)totalDiscount;
            result.TotalPrice -= result.TotalDiscount;
            return result;
        }

    }
}
