using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class OrderWrapResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("status")]
        public OrderWrapStatus Status { get; set; }

        [JsonPropertyName("orders")]
        public List<OrderKithcenResponse> Orders { get; set; }
    }

    public class OrderKithcenResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }
    }
}
