using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Models.Dashboard;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FOCS.Common.Models.Statistics;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepository<OrderEntity> _orderRepository;
        private readonly IRepository<MenuItem> _menuItemRepository;

        private readonly IRepository<OrderWrap> _orderWrapRepository;

        public DashboardService(IRepository<OrderEntity> orderRepository, IRepository<MenuItem> menuItemRepository, IRepository<OrderWrap> orderWrapRepository)
        {
            _orderRepository = orderRepository;
            _menuItemRepository = menuItemRepository;
            _orderWrapRepository = orderWrapRepository;
        }

        public async Task<OrderReportDayResponse> GetOrderStatsAsync(string storeId, bool? today = null)
        {
            var guidStoreId = Guid.Parse(storeId);

            IQueryable<OrderEntity> reportOrders = _orderRepository.AsQueryable()
                .Where(x => x.StoreId == guidStoreId);

            if (today == true)
            {
                var startOfToday = DateTime.Today;
                reportOrders = reportOrders.Where(x => x.CreatedAt >= startOfToday);
            }

            var pendingOrders = await reportOrders.CountAsync(x => x.OrderStatus == Common.Enums.OrderStatus.Pending);
            var canceledOrders = await reportOrders.CountAsync(x => x.OrderStatus == Common.Enums.OrderStatus.Canceled);
            var completedOrders = await reportOrders.CountAsync(x => x.OrderStatus == Common.Enums.OrderStatus.Completed);
            var inProgressOrders = await reportOrders.CountAsync(x => x.OrderStatus == Common.Enums.OrderStatus.Confirmed 
                                                                        || x.OrderStatus == Common.Enums.OrderStatus.Ready);

            var completedOrderTimes = await reportOrders
                .Where(x => x.OrderStatus == Common.Enums.OrderStatus.Completed && x.RemainingTime.HasValue)
                .Select(x => x.RemainingTime.Value.TotalMinutes)
                .ToListAsync();

            var averageCompleteTime = completedOrderTimes.Any()
                ? completedOrderTimes.Average()
                : 0; 

            return new OrderReportDayResponse
            {
                PendingOrders = pendingOrders,
                CanceledOrders = canceledOrders,
                CompletedOrders = completedOrders,
                InProgressOrders = inProgressOrders,
                AvetageCompleteTime = averageCompleteTime
            };
        }


        public async Task<OverviewDayResponse> GetOverviewAsync(string storeId, bool? today = null)
        {
            var guidStoreId = Guid.Parse(storeId);

            IQueryable<OrderEntity> reportOrders = _orderRepository.AsQueryable()
               .Where(x => x.StoreId == guidStoreId)
               .Include(x => x.OrderDetails)
                   .ThenInclude(od => od.MenuItem);

            if (today == true)
            {
                var startOfToday = DateTime.Today;
                reportOrders = reportOrders.Where(x => x.CreatedAt >= startOfToday);
            }

            var totalOrders = await reportOrders.CountAsync();

            var activeTables = await reportOrders.CountAsync(x => x.OrderType == Common.Enums.OrderType.DineIn);

            //var revenue = await reportOrders
            //    .SelectMany(x => x.OrderDetails, (order, od) => new { od.Quantity, od.UnitPrice })
            //    .SumAsync(od => (double)(od.Quantity * od.UnitPrice));

            var revenue = await reportOrders
                            .Select(x => x.TotalAmount)
                            .SumAsync();

            var itemSales = await reportOrders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => new { od.MenuItemId, od.MenuItem.Name })
                .Select(g => new
                {
                    ItemName = g.Key.Name,
                    TotalQuantity = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToListAsync();

            var bestSellingItems = itemSales.Take(5).ToList();
            var worstSellingItem = itemSales.LastOrDefault();

            return new OverviewDayResponse
            {
                TotalOrders = totalOrders,
                ActiveTables = activeTables,
                TotalRevenueToday = revenue,
                BestSellingItem = bestSellingItems.Select(x => new BestSellingItemResponse
                {
                    ItemName = x.ItemName,
                    Quantity = x.TotalQuantity
                }).ToList()
            };
        }



        public async Task<ProdOrderReportResponse> GetKitchenStatsAsync(string storeId, bool? today = null)
        {
            var guidStoreId = Guid.Parse(storeId);

            IQueryable<OrderWrap> reportOrderWraps = _orderWrapRepository.AsQueryable()
               .Where(x => x.StoreId == guidStoreId)
               .Include(x => x.Orders);

            if (today == true)
            {
                var startOfToday = DateTime.Today;
                reportOrderWraps = reportOrderWraps.Where(x => x.CreatedAt >= startOfToday);
            }

            var wraps = await reportOrderWraps.ToListAsync();

            var totalOrderWraps = wraps.Count;
            var currentProcessingOrderWraps = wraps.Count(x => x.OrderWrapStatus == Common.Enums.OrderWrapStatus.Processing);
            var orderWrapNotInProcess = wraps.Count(x => x.OrderWrapStatus == Common.Enums.OrderWrapStatus.Created);
            var completedOrderWrap = wraps.Count(x => x.OrderWrapStatus == Common.Enums.OrderWrapStatus.Completed);
            var cancelledOrderWraps = wraps.Count(x => x.OrderWrapStatus == Common.Enums.OrderWrapStatus.Cancelled);
            var pendingOrderWraps = wraps.Count(x => x.OrderWrapStatus == Common.Enums.OrderWrapStatus.Created);

            var orderInProcess = wraps
                .Where(x => x.OrderWrapStatus == Common.Enums.OrderWrapStatus.Processing)
                .SelectMany(x => x.Orders)
                .Count();

            var completedOrders = wraps
                .Where(x => x.OrderWrapStatus == Common.Enums.OrderWrapStatus.Completed)
                .SelectMany(x => x.Orders)
                .Where(o => o.RemainingTime.HasValue)
                .Select(o => o.RemainingTime.Value.TotalMinutes)
                .ToList();

            var averageTimeComplete = completedOrders.Any()
                ? completedOrders.Average()
                : 0;

            return new ProdOrderReportResponse
            {
                TotalOrders = totalOrderWraps,
                CurrentProcessingOrderCode = currentProcessingOrderWraps.ToString(),
                OrdersNotInProgress = orderWrapNotInProcess,
                OrdersInProgress = orderInProcess,
                CompletedOrders = completedOrderWrap,
                CanceledOrders = cancelledOrderWraps,
                PendingOrders = pendingOrderWraps,
                AverageCompletionTimeMinutes = averageTimeComplete
            };
        }

        public async Task<RevenueReportResponse> GetFinanceStatsAsync(string storeId)
        {
            var guidStoreId = Guid.Parse(storeId);

            var reportOrders = _orderRepository.AsQueryable()
                .Where(x => x.StoreId == guidStoreId && x.OrderStatus == Common.Enums.OrderStatus.Completed);

            var now = DateTime.Now;
            var startOfToday = DateTime.Today;
            var startOfWeek = startOfToday.AddDays(-(int)startOfToday.DayOfWeek); // Sunday = 0
            var startOfMonth = new DateTime(startOfToday.Year, startOfToday.Month, 1);

            var dailyRevenue = await reportOrders
                .Where(x => x.CreatedAt >= startOfToday)
                .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

            var weeklyRevenue = await reportOrders
                .Where(x => x.CreatedAt >= startOfWeek)
                .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

            var monthlyRevenue = await reportOrders
                .Where(x => x.CreatedAt >= startOfMonth)
                .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

            var completedOrders = await reportOrders.ToListAsync();
            var averageBillRevenue = completedOrders.Any()
                ? completedOrders.Average(o => o.TotalAmount)
                : 0;

            //// Revenue by Payment Method
            //var revenueByPaymentMethod = completedOrders
            //    .GroupBy(o => o.PaymentMethod)
            //    .Select(g => new PaymentMethodRevenueDto
            //    {
            //        PaymentMethod = g.Key.ToString(),
            //        TotalRevenue = g.Sum(x => x.TotalAmount)
            //    })
            //    .ToList();

            return new RevenueReportResponse
            {
                DailyRevenue = dailyRevenue,
                WeeklyRevenue = weeklyRevenue,
                MonthlyRevenue = monthlyRevenue,
                AverageBillValue = averageBillRevenue,
                RevenueByPaymentMethod = new List<PaymentMethodRevenueDto>()
            };
        }


        public async Task<CustomerStatsResponse> GetCustomerStatsAsync(string storeId)
        {
            /// Lấy dữ liệu khách hàng: khách mới, khách quay lại, top spender, điểm đánh giá trung bình.
            return new CustomerStatsResponse();
        }

        public Task<MenuStatsResponse> GetMenuStatsAsync(string storeId)
        {
            throw new NotImplementedException();
        }

        public Task<PromotionStatsResponse> GetPromotionStatsAsync(string storeId)
        {
            throw new NotImplementedException();
        }

        public Task<TableStatsResponse> GetTableStatsAsync(string storeId)
        {
            throw new NotImplementedException();
        }
    }
}
