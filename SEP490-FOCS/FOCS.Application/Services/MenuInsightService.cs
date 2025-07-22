using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.Application.Services
{
    public class MenuInsightService : IMenuInsightService
    {

        private readonly IRepository<OrderEntity> _orderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IRepository<Promotion> _promotionRepository;

        public MenuInsightService(IRepository<OrderEntity> orderRepository, IRepository<Promotion> promotionRepository, IRepository<OrderDetail> orderDetailRepository)
        {
            _orderRepository = orderRepository;
            _promotionRepository = promotionRepository;
            _orderDetailRepository = orderDetailRepository;
        }

        public async Task<List<MenuItemInsightResponse>> GetMostOrderedProductsAsync(TimeSpan since, string storeId, int topN = 10)
        {
            var timeSince = DateTime.UtcNow.Subtract(since);
            var orderInStoreInPeriodTime = await _orderRepository.AsQueryable()
                                                                 .Include(x => x.OrderDetails)
                                                                    .ThenInclude(x => x.MenuItem)
                                                                        .ThenInclude(x => x.Images)
                                                                 .Where(x => x.StoreId == Guid.Parse(storeId) && (DateTime)x.CreatedAt >= timeSince)
                                                                 .ToListAsync();

            var allOrderDetails = orderInStoreInPeriodTime
                                    .SelectMany(order => order.OrderDetails)
                                    .ToList();

<<<<<<< HEAD
            return GroupProductInsignt(allOrderDetails, topN);
        }

        public Task<List<MenuItemInsightResponse>> GetProductsBasedOnBestPromotionAsync(string storeId, int topN = 10)
        {
            throw new NotImplementedException();
        }

        public async Task<List<MenuItemInsightResponse>> GetProductOrderNearingWithCurrent(Guid userId, int topN = 1)
        {
            var orders = await _orderRepository.AsQueryable().Include(x => x.OrderDetails)
                                                                .ThenInclude(x => x.MenuItem)
                                                                    .ThenInclude(x => x.Images)
                                                             .Where(x => x.UserId == userId && !x.IsDeleted)
                                                             .OrderByDescending(x => x.CreatedAt)
                                                             .Take(topN)
                                                             .ToListAsync();

            var allOrdersDetail = orders.SelectMany(x => x.OrderDetails).ToList();

            return GroupProductInsignt(allOrdersDetail, topN);
        }

        #region private method
        public List<MenuItemInsightResponse> GroupProductInsignt(List<OrderDetail> orderDetails, int topN = 1)
        {
            var grouped = orderDetails.GroupBy(od => new { od.MenuItemId, od.VariantId })
=======
            var grouped = allOrderDetails.GroupBy(od => new { od.MenuItemId, od.VariantId })
>>>>>>> main
                                                    .Select(g => new {
                                                        MenuItemId = g.Key.MenuItemId,
                                                        VariantId = g.Key.VariantId,
                                                        TotalQuantity = g.Sum(x => x.Quantity),
                                                        MenuItem = g.First().MenuItem,
                                                        Variant = g.First().Variant
                                                    })
                                                    .OrderByDescending(g => g.TotalQuantity)
                                                    .Take(topN)
                                                    .ToList();

<<<<<<< HEAD
            return grouped.Select(g => new MenuItemInsightResponse
=======
            var result = grouped.Select(g => new MenuItemInsightResponse
>>>>>>> main
            {
                MenuItemId = g.MenuItemId,
                Name = g.MenuItem.Name,
                Variants = new List<VariantInsightResponse>
                {
                    new VariantInsightResponse
                    {
                        Variantid = g.VariantId,
                        VariantName = g.Variant?.Name
                    }
                },
                Image = g.MenuItem.Images.FirstOrDefault()?.Url ?? ""
            }).ToList();
<<<<<<< HEAD
        }
        #endregion
=======

            return result;
        }

        public Task<List<MenuItemInsightResponse>> GetProductsBasedOnBestPromotionAsync(string storeId, int topN = 10)
        {
            throw new NotImplementedException();
        }

        public Task<List<MenuItemInsightResponse>> GetSuggestedProductsBasedOnHistoryAsync(Guid userId, int topN = 10)
        {
            throw new NotImplementedException();
        }
>>>>>>> main
    }
}
