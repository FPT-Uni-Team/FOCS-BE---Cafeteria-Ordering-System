using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class CouponUsageResponse
    {
        [JsonPropertyName("coupon_code")]
        public string CouponCode { get; set; }

        [JsonPropertyName("order_code")]
        public string OrderCode { get; set; }

        [JsonPropertyName("actor_id")]
        public string ActorId { get; set; }

        [JsonPropertyName("used_at")]
        public DateTime UsedAt { get; set; }
    }
}
