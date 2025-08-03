using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public class ChangeOrderStatusRequest
    {
        [JsonPropertyName("status")]
        public OrderStatus OrderStatus { get; set; }
    }
}
