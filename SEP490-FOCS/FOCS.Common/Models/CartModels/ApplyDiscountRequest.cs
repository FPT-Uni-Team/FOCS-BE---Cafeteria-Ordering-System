using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models.CartModels
{
    public class ApplyDiscountRequest
    {
        [JsonPropertyName("coupon_code")]
        public string CouponCode { get; set; }
    }
}
