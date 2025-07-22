using Microsoft.AspNetCore.Routing.Constraints;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;

namespace FOCS.Common.Models
{
    public class MenuItemInsightResponse
    {
        [JsonPropertyName("menu_item_id")]
        public Guid MenuItemId { get; set; }

        [JsonPropertyName("menu_item_name")]
        public string Name { get; set; }

        [JsonPropertyName("variants")]
        public List<VariantInsightResponse> Variants { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("total_orderd")]
        public int? TotalOrderd { get; set; } // for case most orderd

        [JsonPropertyName("promotion_price")]
        public double? PromotionPrice {  get; set; } // for case get the best promotion for product
    }

    public class VariantInsightResponse
    {
        [JsonPropertyName("variant_id")]
        public Guid? Variantid { get; set; }

        [JsonPropertyName("variant_name")]
        public string? VariantName { get; set; }
    }
}
