using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.NotificationService.Models
{
    public class NotifyEvent
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("targer_groups")]
        public string[] TargetGroups { get; set; } // e.g., "cashier", "kitchen"

        [JsonPropertyName("mobile_tokens")]
        public string[] MobileTokens { get; set; } // for push to mobile

        [JsonPropertyName("store_id")]
        public string storeId { get; set; }

        [JsonPropertyName("table_id")]
        public string? tableId { get; set; }
    }
}
