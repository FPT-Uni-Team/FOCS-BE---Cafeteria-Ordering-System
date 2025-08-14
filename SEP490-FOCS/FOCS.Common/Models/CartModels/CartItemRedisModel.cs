using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models.CartModels
{
    public class CartItemRedisModel
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        [JsonPropertyName("menu_item_id")]
        public Guid MenuItemId { get; set; }

        [JsonPropertyName("variants")]
        public List<CartVariantRedisModel>? Variants { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonIgnore]
        public DateTime? CreatedTime { get; set; }
    }

    public class CartVariantRedisModel
    {
        [JsonPropertyName("variant_id")]
        public Guid VariantId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }

    public class OrderRedisModel
    {
        [JsonPropertyName("menu_item_id")]
        public Guid MenuItemId { get; set; }

        [JsonPropertyName("variant_id")]
        public Guid? VariantId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }
    }
}
