using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class RevenueReportResponse
    {
        [JsonPropertyName("daily_revenue")]
        public decimal DailyRevenue { get; set; }

        [JsonPropertyName("weekly_revenue")]
        public decimal WeeklyRevenue { get; set; }

        [JsonPropertyName("monthly_revenue")]
        public decimal MonthlyRevenue { get; set; }

        [JsonPropertyName("revenue_by_payment_method")]
        public List<PaymentMethodRevenueDto> RevenueByPaymentMethod { get; set; } = new();

        [JsonPropertyName("average_bill_value")]
        public double AverageBillValue { get; set; }
    }

    public class PaymentMethodRevenueDto
    {
        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [JsonPropertyName("total_revenue")]
        public decimal TotalRevenue { get; set; }
    }
}
