using FOCS.Application.Services.Interface;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Models.Dashboard;
using FOCS.Order.Infrastucture.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api")]
    [ApiController]
    public class DashboardController : FocsController
    {
       private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpPost("order-statistic")]
        public async Task<OrderReportDayResponse> OrderReportDayResponse([FromHeader] bool? today = null)
        {
            return await _dashboardService.GetOrderStatsAsync(StoreId, today);
        }

        [HttpPost("overview")]
        public async Task<OverviewDayResponse> GetOverviewAsync([FromHeader] bool? today = null)
        {
            return await _dashboardService.GetOverviewAsync(StoreId, today);
        }

        [HttpPost("kitchen-statistic")]
        public async Task<ProdOrderReportResponse> GetKitchenStatsAsync([FromHeader] bool? today = null)
        {
            return await _dashboardService.GetKitchenStatsAsync(StoreId, today);
        }

        [HttpPost("finance-statistic")]
        public async Task<RevenueReportResponse> GetFinanceStatsAsync()
        {
            return await _dashboardService.GetFinanceStatsAsync(StoreId);
        }
    }
}
