using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class ReceiveHookRequest
    {
        [JsonPropertyName("order_code")]
        public string OrderCode { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
