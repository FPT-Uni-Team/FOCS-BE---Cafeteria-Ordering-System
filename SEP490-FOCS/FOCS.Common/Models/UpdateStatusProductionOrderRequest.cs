using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class UpdateStatusProductionOrderRequest
    {
        [JsonPropertyName("order_wrap_id")]
        public Guid OrderWrapId {  get; set; }

        [JsonPropertyName("status")]
        public OrderWrapStatus Status { get; set; }

        
    }
}
