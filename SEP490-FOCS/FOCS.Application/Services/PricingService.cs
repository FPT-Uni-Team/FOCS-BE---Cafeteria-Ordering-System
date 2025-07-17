using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class PricingService : IPricingService
    {
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly IRepository<MenuItemVariant> _menuItemVariantRepo;

        public PricingService(IRepository<MenuItem> menuItemRepo, IRepository<MenuItemVariant> menuItemVariantRepo)
        {
            _menuItemRepository = menuItemRepo;
            _menuItemVariantRepo = menuItemVariantRepo;
        }

        public async Task<PricingDTO> GetPriceByProduct(Guid productId, Guid? variantId, Guid? storeId)
        {
            var menuItem = (await _menuItemRepository.FindAsync(x => x.Id == productId && x.StoreId == storeId)).FirstOrDefault();
            ConditionCheck.CheckCondition(menuItem != null && menuItem.BasePrice > 0, Errors.Pricing.InvalidPrice);

            double productPrice = (double)menuItem.BasePrice;
            double variantPrice = 0;

            if (variantId.HasValue)
            {
                var variant = (await _menuItemVariantRepo.FindAsync(x => x.Id == variantId.Value)).FirstOrDefault();
                ConditionCheck.CheckCondition(variant != null && variant.Price >= 0, Errors.Pricing.InvalidPrice);
                variantPrice = (double)variant.Price;
            }

            return new PricingDTO
            {
                ProductPrice = productPrice,
                VariantPrice = variantPrice
            };
        }

        public async Task<double> CalculatePriceOfProducts(Dictionary<Guid, Guid?> products, string storeId)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(storeId, out Guid storeGuid), Errors.Common.InvalidGuidFormat);

            double totalPrice = 0;

            foreach (var product in products)
            {
                var price = await GetPriceByProduct(product.Key, product.Value, storeGuid);
                totalPrice += price.ProductPrice + price.VariantPrice ?? 0;
            }

            return totalPrice;
        }

    }
}
