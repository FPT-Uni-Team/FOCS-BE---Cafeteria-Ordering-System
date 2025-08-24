using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models.Dashboard
{
    public class OverviewDayResponse
    {
        [JsonPropertyName("total_revenue_today")]
        public double TotalRevenueToday { get; set; }

        [JsonPropertyName("total_orders")]
        public int TotalOrders { get; set; }

        [JsonPropertyName("active_tables")]
        public int ActiveTables { get; set; }

        [JsonPropertyName("available_tables")]
        public int AvailableTables { get; set; }

        [JsonPropertyName("best_selling_item")]
        public List<BestSellingItemResponse> BestSellingItem { get; set; }

    }

    public class BestSellingItemResponse
    {
        [JsonPropertyName("item_name")]
        public string ItemName { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
