using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static FOCS.Common.Exceptions.Errors;

namespace FOCS.Common.Models
{
    public class OrderDetailDTO
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public double UnitPrice { get; set; } // base price

        [JsonPropertyName("total_price")]
        public double TotalPrice { get; set; } // total: base price + variants price

        [JsonPropertyName("note")]
        public string Note { get; set; }

        [JsonPropertyName("variants")]
        public string Variants { get; set; } // save with string format or json format. exmaple: "size L, trung" -> after that split(",") -> List variants and check with db variants list
    }
}
