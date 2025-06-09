using FOCS.Application.Services.Interface;
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

namespace FOCS.Application.Services
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

        public async Task<DiscountResultDTO> ApplyDiscountAsync(CreateOrderRequest order, string? couponCode = null)
        {
            var promotion = await _promotionRepository.AsQueryable()
                .Include(x => x.Coupons)
                .FirstOrDefaultAsync(x => x.Coupons.Any(c => c.Code == couponCode));

            ConditionCheck.CheckCondition(promotion != null, Errors.PromotionError.PromotionNotFound);

            var result = new DiscountResultDTO
            {
                ItemDiscountDetails = new List<DiscountItemDetail>(),
                AppliedPromotions = new List<string> { promotion.Title }
            };

            var acceptItemIds = promotion.AcceptForItems?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(Guid.Parse)
                .ToHashSet();

            double totalDiscount = 0;

            foreach (var itemOrder in order.Items)
            {
                bool isAccepted = acceptItemIds == null || acceptItemIds.Contains(itemOrder.MenuItemId);
                if (!isAccepted) continue;

                var pricing = await _pricingService.GetPriceByProduct(itemOrder.MenuItemId, itemOrder.VariantId, order.StoreId);
                double itemPrice = (double)pricing.ProductPrice + (double)pricing.VariantPrice;

                double itemDiscount = 0;

                switch (promotion.PromotionType)
                {
                    case PromotionType.Percentage:
                        itemDiscount = ApplyPercentageDiscount(itemPrice, promotion.DiscountValue);
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
                        ItemCode = $"{itemOrder.MenuItemId}_{itemOrder.VariantId}",
                        ItemName = itemOrder.MenuItemId.ToString(),
                        Quantity = itemOrder.Quantity,
                        Source = $"Promotion_{promotion.PromotionType}_{promotion.Title}"
                    });
                }
            }

            result.TotalDiscount = (decimal)totalDiscount;
            return result;
        }

        private double ApplyPercentageDiscount(double itemPrice, double? discountValue)
        {
            if (discountValue == null) return 0;
            return Math.Round(itemPrice * (double)discountValue, 2);
        }

        private double ApplyFixedAmountDiscount(double itemPrice, double? discountValue)
        {
            if (discountValue == null) return 0;
            return Math.Min(itemPrice, (double)discountValue);
        }

        private async Task<List<DiscountItemDetail>> ApplyBuyXGetYDiscount(CreateOrderRequest order, Promotion promotion)
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

            double totalDiscount = (double)freeItemPrice * getQuantity * applicableSets;

            discountDetails.Add(new DiscountItemDetail
            {
                DiscountAmount = (decimal)totalDiscount,
                ItemCode = getItemId.ToString(),
                ItemName = getItemId.ToString(),
                Quantity = getQuantity * applicableSets,
                Source = $"Promotion_Buy{buyQuantity}Get{getQuantity}_{promotion.Title}"
            });

            return discountDetails;
        }

    }
}
