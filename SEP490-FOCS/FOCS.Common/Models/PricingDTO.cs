using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class PricingDTO
    {
        [JsonPropertyName("product_price")]
        public double ProductPrice { get; set; }

        [JsonPropertyName("variant_price")]
        public double? VariantPrice { get; set; } = 0;
    }
}
