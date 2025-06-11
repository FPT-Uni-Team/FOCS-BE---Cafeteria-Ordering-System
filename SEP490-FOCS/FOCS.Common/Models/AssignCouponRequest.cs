using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class AssignCouponRequest
    {
        [JsonPropertyName("coupon_ids")]
        public List<Guid> CouponIds { get; set; }

        [JsonPropertyName("promotion_id")]
        public Guid PromotionId { get; set; }
    }

}
