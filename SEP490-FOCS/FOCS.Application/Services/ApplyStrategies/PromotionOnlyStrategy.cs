using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Order.Infrastucture.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services.ApplyStrategy
{
    public class PromotionOnlyStrategy : IDiscountStrategyService
    {
        private readonly IRepository<Coupon> _couponRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<PromotionItemCondition> _promotionItemConditionRepo;

        private readonly IMenuService _menuService;
        private readonly IPricingService _pricingService;

        public PromotionOnlyStrategy(IRepository<Coupon> couponRepository, IRepository<PromotionItemCondition> promotionItemConditionRepo, IRepository<Promotion> promotionRepository, IMenuService menuService, IPricingService pricingService)
        {
            _couponRepository = couponRepository;
            _promotionItemConditionRepo = promotionItemConditionRepo;
            _menuService = menuService;
            _pricingService = pricingService;
            _promotionRepository = promotionRepository;
        }

        public async Task<DiscountResultDTO> ApplyDiscountAsync(ApplyDiscountOrderRequest order, string? couponCode = null)
        {
            var promotion = await _promotionRepository.AsQueryable()
                .Include(x => x.Coupons)
                .FirstOrDefaultAsync(x => x.Coupons.Any(c => c.Code == couponCode));

            //ConditionCheck.CheckCondition(promotion != null, Errors.PromotionError.PromotionNotFound);
            if (promotion == null)
            {
                var resultNotApply = new DiscountResultDTO
                {
                    AppliedCouponCode = null,
                    AppliedPromotions = null,
                    Messages = null,
                    TotalDiscount = 0,
                    TotalPrice = 0,
                    ItemDiscountDetails = new List<DiscountItemDetail>()
                };

                var allProducts = await _menuService.GetMenuItemByIds(order.Items.Select(x => x.MenuItemId).ToList() , order.StoreId);

                foreach (var item in order.Items)
                {
                    double totalPrice = 0;
                    foreach (var itemVariant in item.Variants)
                    {
                        var pricing = await _pricingService.GetPriceByProduct(item.MenuItemId, itemVariant.VariantId, order.StoreId);
                        double currentPrice = (double)(pricing.VariantPrice ?? 0);
                        totalPrice += currentPrice * itemVariant.Quantity;
                    }

                    resultNotApply.TotalPrice += (decimal)totalPrice * item.Quantity;

                    resultNotApply.ItemDiscountDetails.Add(new DiscountItemDetail
                    {
                        BuyItemCode = $"{item.MenuItemId}",
                        BuyItemName = $"{allProducts.FirstOrDefault(x => x.Id == item.MenuItemId).Name}",
                        DiscountAmount = 0,
                        Quantity = item.Quantity,
                        Source = CouponConstants.PromotionOnly_NotEligible
                    });
                }

                return resultNotApply;
            }


            var result = new DiscountResultDTO
            {
                ItemDiscountDetails = new List<DiscountItemDetail>(),
                AppliedPromotions = new List<string> { promotion.Title }
            };

            var acceptItemIds = promotion.AcceptForItems;

            double totalDiscount = 0;

            result.TotalPrice = 0;

            foreach (var item in order.Items)
            {
                var currentProductPrice = await _pricingService.GetPriceByProduct(item.MenuItemId, null, order.StoreId);
                decimal totalVariantPrice = 0;

                if (item.Variants != null && item.Variants.Any())
                {
                    foreach (var variant in item.Variants)
                    {
                        var variantPriceInfo = await _pricingService.GetPriceByProduct(item.MenuItemId, variant.VariantId, order.StoreId);
                        totalVariantPrice += (decimal)(variantPriceInfo.VariantPrice * variant.Quantity);
                    }

                    result.TotalPrice += ((decimal)currentProductPrice.ProductPrice + totalVariantPrice) * item.Quantity;
                }
                else
                {
                    decimal itemPrice = (decimal)(currentProductPrice.ProductPrice + (currentProductPrice.VariantPrice ?? 0));
                    result.TotalPrice += itemPrice * item.Quantity;
                }

            }

            if (promotion.PromotionScope == PromotionScope.Item)
            {
                foreach (var itemOrder in order.Items)
                {
                    bool isAccepted = acceptItemIds.Any() || !acceptItemIds.Contains(itemOrder.MenuItemId);
                    if (!isAccepted) continue;

                    double itemPrice = 0;

                    if(itemOrder.Variants != null)
                    {
                        foreach(var itemVariant in itemOrder.Variants)
                        {
                            var pricing = await _pricingService.GetPriceByProduct(itemOrder.MenuItemId, itemVariant.VariantId, order.StoreId);
                            itemPrice = (double)pricing.ProductPrice + (double)pricing.VariantPrice;
                        }
                    }

                    double itemDiscount = 0;

                    switch (promotion.PromotionType)
                    {
                        case PromotionType.Percentage:
                            itemDiscount = ApplyPercentageDiscount(itemPrice, promotion.DiscountValue, promotion.MaxDiscountValue);
                            break;
                        case PromotionType.FixedAmount:
                            itemDiscount = ApplyFixedAmountDiscount(itemPrice, promotion.DiscountValue);
                            break;
                        case PromotionType.BuyXGetY:
                            var buyXGetYDiscounts = await ApplyBuyXGetYDiscount(order, promotion);
                            result.ItemDiscountDetails.AddRange(buyXGetYDiscounts);
                            itemDiscount = buyXGetYDiscounts.Sum(d => (double)d.DiscountAmount);
                            break;
                        default:
                            itemDiscount = 0;
                            break;
                    }

                    if (itemDiscount > 0)
                    {
                        totalDiscount += itemDiscount;

                        result.ItemDiscountDetails.Add(new DiscountItemDetail
                        {
                            DiscountAmount = (decimal)itemDiscount,
                            BuyItemCode = $"{itemOrder.MenuItemId}_{string.Join("_", itemOrder.Variants.Select(x => x.VariantId))}",
                            BuyItemName = itemOrder.MenuItemId.ToString(),
                            Quantity = itemOrder.Quantity,
                            Source = $"Promotion_{promotion.PromotionType}_{promotion.Title}"
                        });
                    }
                }
            } 

            if(promotion.PromotionScope == PromotionScope.Order)
            {
                switch (promotion.PromotionType)
                {
                    case PromotionType.Percentage:
                        totalDiscount = (double)result.TotalPrice * (double)(promotion.DiscountValue / 100);
                        break;
                    case PromotionType.FixedAmount:
                        totalDiscount = (double)promotion.DiscountValue;
                        break;
                    case PromotionType.BuyXGetY:
                        var buyXGetYDiscounts = await ApplyBuyXGetYDiscount(order, promotion);
                        result.ItemDiscountDetails.AddRange(buyXGetYDiscounts);
                        totalDiscount = buyXGetYDiscounts.Sum(d => (double)d.DiscountAmount);
                        break;
                }
            }


            result.TotalDiscount = (decimal)totalDiscount;
            return result;
        }

        private double ApplyPercentageDiscount(double itemPrice, double? discountValue, double? maxDiscountValue)
        {
            if (discountValue == null) return 0;
            var currentDecrease = Math.Round(itemPrice * (double)(discountValue / 100), 2);

            if (maxDiscountValue.HasValue && currentDecrease > maxDiscountValue) currentDecrease = (double)maxDiscountValue;
        
            return currentDecrease;
        }

        private double ApplyFixedAmountDiscount(double itemPrice, double? discountValue)
        {
            if (discountValue == null) return 0;
            return Math.Min(itemPrice, (double)discountValue);
        }

        private async Task<List<DiscountItemDetail>> ApplyBuyXGetYDiscount(ApplyDiscountOrderRequest order, Promotion promotion, string? storeId = null)
        {
            var discountDetails = new List<DiscountItemDetail>();

            var promotionItemCondition = (await _promotionItemConditionRepo.FindAsync(x => x.PromotionId == promotion.Id)).FirstOrDefault();
            if (promotionItemCondition == null) return discountDetails;

            var buyItemId = promotionItemCondition.BuyItemId;
            var getItemId = promotionItemCondition.GetItemId;
            var buyQuantity = promotionItemCondition.BuyQuantity;
            var getQuantity = promotionItemCondition.GetQuantity;

            var boughtItem = order.Items.FirstOrDefault(i => i.MenuItemId == buyItemId);
            if (boughtItem == null || boughtItem.Quantity < buyQuantity)
                return discountDetails;

            int applicableSets = boughtItem.Quantity / buyQuantity;

            var pricing = await _pricingService.GetPriceByProduct(getItemId, null, order.StoreId);
            var freeItemPrice = pricing.ProductPrice;

            var productsFree = await _menuService.GetMenuItemByIds(new List<Guid> { buyItemId, getItemId}, promotion.StoreId);

            double totalDiscount = (double)freeItemPrice * getQuantity * applicableSets;

            discountDetails.Add(new DiscountItemDetail
            {
                DiscountAmount = (decimal)totalDiscount,
                BuyItemCode = getItemId.ToString(),
                BuyItemName = productsFree.Where(x => x.Id == buyItemId).FirstOrDefault().Name,
                GetItemCode = getItemId.ToString(),
                getItemName = productsFree.Where(x => x.Id == getItemId).FirstOrDefault().Name,
                Quantity = getQuantity * applicableSets,
                Source = $"Promotion Buy {buyQuantity} {productsFree.Where(x => x.Id == buyItemId).FirstOrDefault().Name} Get {getQuantity} {productsFree.Where(x => x.Id == getItemId).FirstOrDefault().Name} - {promotion.Title}"
            });

            return discountDetails;
        }

    }
}
