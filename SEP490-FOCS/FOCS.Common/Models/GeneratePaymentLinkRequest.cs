using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class GeneratePaymentLinkRequest
    {
        [JsonPropertyName("order_code")]
        public long OrderCode { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("description")]
        public string? Description {  get; set; }

        [JsonPropertyName("table_id")]
        public Guid? TableId { get; set; }

        [JsonPropertyName("items")]
        public object? Items {  get; set; }
    }
}
