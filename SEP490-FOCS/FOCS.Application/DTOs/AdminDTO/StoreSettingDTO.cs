using FOCS.Common.Enums;
using FOCS.Order.Infrastucture.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class StoreSettingDTO
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [JsonPropertyName("open_time")]
        public TimeSpan OpenTime { get; set; }

        [JsonPropertyName("close_time")]
        public TimeSpan CloseTime { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; } = "VND";

        [JsonPropertyName("payment_config")]
        public PaymentConfig PaymentConfig { get; set; }

        [JsonPropertyName("logo_url")]
        public string? LogoUrl { get; set; } = "";

        [JsonPropertyName("is_self_service")]
        public bool IsSelfService { get; set; }

        [JsonPropertyName("discount_strategy")]
        public DiscountStrategy DiscountStrategy { get; set; }

        [JsonPropertyName("allow_combine_promotion_coupon")]
        public bool AllowCombinePromotionAndCoupon { get; set; } = true;

        [JsonPropertyName("spending_rate")]
        [Range(1, int.MaxValue, ErrorMessage = "must be greater than 0")]
        public int? SpendingRate { get; set; } = 1;

    }
}
