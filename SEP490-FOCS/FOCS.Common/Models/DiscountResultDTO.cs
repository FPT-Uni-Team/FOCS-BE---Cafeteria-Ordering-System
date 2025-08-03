using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class DiscountResultDTO
    {
        [JsonPropertyName("total_discount")]
        public decimal TotalDiscount { get; set; }

        [JsonPropertyName("total_price")]
        public decimal TotalPrice {  get; set; }

        [JsonPropertyName("applied_coupon_code")]
        public string? AppliedCouponCode { get; set; }

        [JsonPropertyName("applied_promotions")]
        public List<string> AppliedPromotions { get; set; } = new();

        [JsonPropertyName("item_discount_details")]
        public List<DiscountItemDetail> ItemDiscountDetails { get; set; } = new();

        [JsonPropertyName("messages")]
        public List<string> Messages { get; set; } = new();

        [JsonPropertyName("is_discount_applied")]
        public bool IsDiscountApplied => TotalDiscount > 0;

        [JsonPropertyName("order_code")]
        public long? OrderCode { get; set; }

        [JsonPropertyName("is_use_point")]
        public bool? IsUsePoint {  get; set; }

        [JsonPropertyName("point")]
        public int? Point {  get; set; }
    }
    public class DiscountItemDetail
    {
        [JsonPropertyName("item_code")]
        public string ItemCode { get; set; } = string.Empty;

        [JsonPropertyName("item_name")]
        public string ItemName { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("discount_amount")]
        public decimal DiscountAmount { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
    }
}
