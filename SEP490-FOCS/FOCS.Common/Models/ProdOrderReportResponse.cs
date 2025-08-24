using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class ProdOrderReportResponse
    {
        [JsonPropertyName("total_order_wraps")]
        public int TotalOrders { get; set; }

        [JsonPropertyName("current_processing_order_wrap")]
        public string? CurrentProcessingOrderCode { get; set; }

        [JsonPropertyName("orders_not_in_progress")]
        public int OrdersNotInProgress { get; set; }

        [JsonPropertyName("orders_in_progress")]
        public int OrdersInProgress { get; set; }

        [JsonPropertyName("completed_orders")]
        public int CompletedOrders { get; set; }

        [JsonPropertyName("canceled_orders")]
        public int CanceledOrders { get; set; }

        [JsonPropertyName("pending_orders")]
        public int PendingOrders { get; set; }

        [JsonPropertyName("average_completion_time_minutes")]
        public double AverageCompletionTimeMinutes { get; set; }
    }
}
