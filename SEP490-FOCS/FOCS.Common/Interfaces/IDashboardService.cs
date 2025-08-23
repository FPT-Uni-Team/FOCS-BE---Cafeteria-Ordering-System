using FOCS.Common.Models;
using FOCS.Common.Models.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FOCS.Common.Models.Statistics;

namespace FOCS.Common.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Lấy dữ liệu tổng quan: doanh thu trong ngày, số đơn, bàn hoạt động, món bán chạy.
        /// </summary>
        Task<OverviewDayResponse> GetOverviewAsync(string storeId, bool? today = null);

        /// <summary>
        /// Lấy thống kê đơn hàng: theo trạng thái, thời gian hoàn thành trung bình.
        /// </summary>
        Task<OrderReportDayResponse> GetOrderStatsAsync(string storeId, bool? today = null);

        /// <summary>
        /// Lấy dữ liệu bếp: món đang chế biến, đơn hàng bị trễ.
        /// </summary>
        Task<ProdOrderReportResponse> GetKitchenStatsAsync(string storeId, bool? today = null);

        /// <summary>
        /// Lấy thống kê menu & category: tổng số món, món sắp hết hàng, doanh thu theo category.
        /// </summary>
        Task<MenuStatsResponse> GetMenuStatsAsync(string storeId);

        /// <summary>
        /// Lấy thống kê bàn: bàn trống, bàn đang dùng, bàn đã đặt trước, thời gian trung bình sử dụng.
        /// </summary>
        Task<TableStatsResponse> GetTableStatsAsync(string storeId);

        /// <summary>
        /// Lấy dữ liệu tài chính: doanh thu ngày/tuần/tháng, doanh thu theo phương thức thanh toán, giá trị trung bình hóa đơn.
        /// </summary>
        Task<RevenueReportResponse> GetFinanceStatsAsync(string storeId);

        /// <summary>
        /// Lấy dữ liệu khuyến mãi: số coupon phát hành/đã dùng, tổng doanh thu giảm giá, top khách hàng dùng coupon.
        /// </summary>
        Task<PromotionStatsResponse> GetPromotionStatsAsync(string storeId);

        /// <summary>
        /// Lấy dữ liệu khách hàng: khách mới, khách quay lại, top spender, điểm đánh giá trung bình.
        /// </summary>
        Task<CustomerStatsResponse> GetCustomerStatsAsync(string storeId);
    }
}
