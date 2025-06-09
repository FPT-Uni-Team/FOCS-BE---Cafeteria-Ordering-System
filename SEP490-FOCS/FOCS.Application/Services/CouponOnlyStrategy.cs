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
            var coupon = (await _couponRepository.FindAsync(x => x.Code == couponCode && x.StoreId == order.StoreId))?.FirstOrDefault();
            var result = new DiscountResultDTO();

            if (coupon == null)
                return result;

            var acceptItemIds = coupon.AcceptForItems?.Split(',').Select(Guid.Parse).ToHashSet();

            double totalDiscount = 0;

            foreach (var itemOrder in order.Items)
            {
                var isAccepted = acceptItemIds == null || acceptItemIds.Contains(itemOrder.MenuItemId);

                var pricing = await _pricingService.GetPriceByProduct(itemOrder.MenuItemId, itemOrder.VariantId, order.StoreId);
                var itemPrice = pricing.ProductPrice + pricing.VariantPrice;

                switch (coupon.DiscountType)
                {
                    case DiscountType.Percent:
                        if (isAccepted)
                            totalDiscount += Math.Round((double)itemPrice * coupon.Value, 2);
                        break;

                    case DiscountType.FixedAmount:
                        if (isAccepted)
                            totalDiscount += Math.Min((double)itemPrice, coupon.Value);
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
