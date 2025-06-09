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
            var promotion = await _promotionRepository.AsQueryable().Include(x => x.Coupons).FirstOrDefaultAsync(x => x.Coupons.Any(x => x.Code == couponCode));
            ConditionCheck.CheckCondition(promotion != null, Errors.PromotionError.PromotionNotFound);

            var result = new DiscountResultDTO();

            var acceptItemIds = promotion.AcceptForItems?.Split(',').Select(Guid.Parse).ToHashSet();

            double totalDiscount = 0;

            foreach (var itemOrder in order.Items)
            {
                var isAccepted = acceptItemIds == null || acceptItemIds.Contains(itemOrder.MenuItemId);

                var pricing = await _pricingService.GetPriceByProduct(itemOrder.MenuItemId, itemOrder.VariantId, order.StoreId);
                var itemPrice = pricing.ProductPrice + pricing.VariantPrice;

                switch (promotion.PromotionType)
                {
                    case PromotionType.Percentage:
                        if (isAccepted)
                            totalDiscount += Math.Round((double)itemPrice * (double)promotion.DiscountValue, 2);
                        break;

                    case PromotionType.FixedAmount:
                        if (isAccepted)
                            totalDiscount += Math.Min((double)itemPrice, (double)promotion.DiscountValue);
                        break;

                    case PromotionType.BuyXGetY:
                        if (isAccepted)
                        {
                            var promotionItemCondition = (await _promotionItemConditionRepo.FindAsync(x => x.PromotionId == promotion.Id)).FirstOrDefault();
                        }
                        break;
                    default:
                        break;
                }
            }

            result.TotalDiscount = (decimal)totalDiscount;
            return result;

        }
    }
}
