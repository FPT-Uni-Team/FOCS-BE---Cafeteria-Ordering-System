using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class Statistics
    {
        public class CustomerStatsResponse
        {
            [JsonPropertyName("total_customers")]
            public int TotalCustomers { get; set; }

            [JsonPropertyName("new_customers_today")]
            public int NewCustomersToday { get; set; }

            [JsonPropertyName("active_customers")]
            public int ActiveCustomers { get; set; }

            [JsonPropertyName("returning_customers")]
            public int ReturningCustomers { get; set; }
        }

        public class MenuStatsResponse
        {
            [JsonPropertyName("total_menu_items")]
            public int TotalMenuItems { get; set; }

            [JsonPropertyName("best_selling_items")]
            public List<string> BestSellingItems { get; set; } = new();

            [JsonPropertyName("low_stock_items")]
            public List<string> LowStockItems { get; set; } = new();

            [JsonPropertyName("inactive_items")]
            public int InactiveItems { get; set; }
        }

        public class PromotionStatsResponse
        {
            [JsonPropertyName("total_promotions")]
            public int TotalPromotions { get; set; }

            [JsonPropertyName("active_promotions")]
            public int ActivePromotions { get; set; }

            [JsonPropertyName("expired_promotions")]
            public int ExpiredPromotions { get; set; }

            [JsonPropertyName("promotion_usage_rate")]
            public double PromotionUsageRate { get; set; }
        }

        public class TableStatsResponse
        {
            [JsonPropertyName("total_tables")]
            public int TotalTables { get; set; }

            [JsonPropertyName("available_tables")]
            public int AvailableTables { get; set; }

            [JsonPropertyName("occupied_tables")]
            public int OccupiedTables { get; set; }

            [JsonPropertyName("reserved_tables")]
            public int ReservedTables { get; set; }
        }
    }
}
