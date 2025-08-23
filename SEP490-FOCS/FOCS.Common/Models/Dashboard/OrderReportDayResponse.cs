using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models.Dashboard
{
    public class OrderReportDayResponse
    {
        [JsonPropertyName("pending_orders")]
        public int PendingOrders { get; set; }

        [JsonPropertyName("inprogress_orders")]
        public int InProgressOrders { get; set; }

        [JsonPropertyName("completed_orders")]
        public int CompletedOrders { get; set; }

        [JsonPropertyName("cancelled_orders")]
        public int CanceledOrders { get; set; }

        [JsonPropertyName("average_complete_time")]
        public double AvetageCompleteTime { get; set; }
    }
}
