using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models.CartModels
{
    public class CartDTO
    {
        [JsonPropertyName("items")]
        public List<CartItemDTO> Items { get; set; }

        [JsonPropertyName("total_price")]
        public decimal TotalPrice { get; set; }

        [JsonPropertyName("discounted_price")]
        public decimal? DiscountedPrice { get; set; }

        [JsonPropertyName("applied_coupon_code")]
        public string? AppliedCouponCode { get; set; }
    }

    public class CartItemDTO
    {
        [JsonPropertyName("cart_item_id")]
        public Guid CartItemId { get; set; }

        [JsonPropertyName("menu_item_id")]
        public Guid MenuItemId { get; set; }

        [JsonPropertyName("variant_id")]
        public Guid? VariantId { get; set; }

        [JsonPropertyName("menu_item_name")]
        public string MenuItemName { get; set; }

        [JsonPropertyName("variant_name")]
        public string? VariantName { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("total_price")]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
