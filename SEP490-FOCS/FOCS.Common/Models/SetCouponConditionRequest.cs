using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class SetCouponConditionRequest
    {
        [JsonPropertyName("condition_type")]
        public CouponConditionType ConditionType { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}
