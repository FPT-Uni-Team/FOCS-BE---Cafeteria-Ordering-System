using FOCS.Application.Services.Interface;
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

namespace FOCS.Application.Services
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
                ItemDiscountDetails = new List<DiscountItemDetail>(),
                AppliedPromotions = new List<string>(),
                TotalPrice = 0
            };

            if (string.IsNullOrEmpty(couponCode))
                return result;

            var coupon = (await _couponRepository.FindAsync(x => x.Code == couponCode && x.StoreId == order.StoreId))
                ?.FirstOrDefault();

            if (coupon == null)
                return result;

            result.AppliedCouponCode = coupon.Code;

            double totalOrderAmount = 0;
            var pricingDict = new Dictionary<(Guid MenuItemId, Guid? VariantId), (double? ProductPrice, double? VariantPrice)>();

            foreach (var itemOrder in order.Items)
            {
                var pricing = await _pricingService.GetPriceByProduct(itemOrder.MenuItemId, itemOrder.VariantId, order.StoreId);
                pricingDict[(itemOrder.MenuItemId, itemOrder.VariantId)] = (pricing.ProductPrice, pricing.VariantPrice);

                var itemPrice = pricing.ProductPrice + pricing.VariantPrice;
                totalOrderAmount += (double)itemPrice * itemOrder.Quantity;
                result.TotalPrice += (decimal)(itemPrice * itemOrder.Quantity);
            }

            // CASE 1: Discount on total order
            if (coupon.MinimumOrderAmount.HasValue && totalOrderAmount >= coupon.MinimumOrderAmount)
            {
                double discountAmount = 0;

                switch (coupon.DiscountType)
                {
                    case DiscountType.Percent:
                        discountAmount = Math.Round(totalOrderAmount * coupon.Value / 100, 2);
                        break;
                    case DiscountType.FixedAmount:
                        discountAmount = Math.Min(totalOrderAmount, coupon.Value);
                        break;
                }

                result.TotalDiscount = (decimal)discountAmount;
                result.TotalPrice -= result.TotalDiscount;

                result.ItemDiscountDetails.Add(new DiscountItemDetail
                {
                    DiscountAmount = (decimal)discountAmount,
                    ItemCode = coupon.Id.ToString(),
                    ItemName = "Entire Order",
                    Quantity = 1,
                    Source = "Coupon_OrderLevel"
                });

                return result;
            }

            // CASE 2: Discount on specific items
            if (!string.IsNullOrEmpty(coupon.AcceptForItems))
            {
                var acceptItemIds = coupon.AcceptForItems
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Guid.Parse)
                    .ToHashSet();

                double totalDiscount = 0;

                foreach (var itemOrder in order.Items)
                {
                    if (!acceptItemIds.Contains(itemOrder.MenuItemId))
                        continue;

                    if (coupon.MinimumItemQuantity.HasValue && itemOrder.Quantity < coupon.MinimumItemQuantity)
                        continue;

                    var (productPrice, variantPrice) = pricingDict[(itemOrder.MenuItemId, itemOrder.VariantId)];
                    var itemPrice = (double)(productPrice + variantPrice);

                    double itemDiscount = 0;
                    string source = "";

                    switch (coupon.DiscountType)
                    {
                        case DiscountType.Percent:
                            itemDiscount = Math.Round(itemPrice * coupon.Value / 100, 2);
                            source = "Coupon_Percent";
                            break;
                        case DiscountType.FixedAmount:
                            itemDiscount = Math.Min(itemPrice, coupon.Value);
                            source = "Coupon_FixedAmount";
                            break;
                    }

                    totalDiscount += itemDiscount;

                    result.ItemDiscountDetails.Add(new DiscountItemDetail
                    {
                        DiscountAmount = (decimal)itemDiscount,
                        ItemCode = $"{itemOrder.MenuItemId}_{itemOrder.VariantId}",
                        ItemName = itemOrder.MenuItemId.ToString(),
                        Quantity = itemOrder.Quantity,
                        Source = source
                    });
                }

                result.TotalDiscount = (decimal)totalDiscount;
                result.TotalPrice -= result.TotalDiscount;
                return result;
            }

            // DEFAULT: Apply coupon to all items
            {
                double totalDiscount = 0;

                foreach (var itemOrder in order.Items)
                {
                    var (productPrice, variantPrice) = pricingDict[(itemOrder.MenuItemId, itemOrder.VariantId)];
                    var itemPrice = (double)(productPrice + variantPrice);

                    double itemDiscount = 0;
                    string source = "";

                    switch (coupon.DiscountType)
                    {
                        case DiscountType.Percent:
                            itemDiscount = Math.Round(itemPrice * coupon.Value / 100, 2);
                            source = "Coupon_Percent";
                            break;
                        case DiscountType.FixedAmount:
                            itemDiscount = Math.Min(itemPrice, coupon.Value);
                            source = "Coupon_FixedAmount";
                            break;
                    }

                    totalDiscount += itemDiscount;

                    result.ItemDiscountDetails.Add(new DiscountItemDetail
                    {
                        DiscountAmount = (decimal)itemDiscount,
                        ItemCode = $"{itemOrder.MenuItemId}_{itemOrder.VariantId}",
                        ItemName = itemOrder.MenuItemId.ToString(),
                        Quantity = itemOrder.Quantity,
                        Source = source
                    });
                }

                result.TotalDiscount = (decimal)totalDiscount;
                result.TotalPrice -= result.TotalDiscount;
                return result;
            }
        }
    }
}
