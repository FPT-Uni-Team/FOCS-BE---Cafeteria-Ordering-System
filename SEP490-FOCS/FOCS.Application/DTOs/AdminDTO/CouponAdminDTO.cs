using FOCS.Common.Enums;
using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class CouponAdminDTO
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("discount_type")]
        public DiscountType DiscountType { get; set; }
        [JsonPropertyName("value")]
        public double Value { get; set; }
        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }
        [JsonPropertyName("end-date")]
        public DateTime EndDate { get; set; }
        [JsonPropertyName("max_usage")]
        public int MaxUsage { get; set; }
        [JsonPropertyName("count_used")]
        public int CountUsed { get; set; }
        [JsonPropertyName("max_usage_per_user")]
        public int? MaxUsagePerUser { get; set; }
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        // Foreign key
        [JsonPropertyName("store_id")]
        public Guid StoreId { get; set; }
        [JsonPropertyName("promotion_id")]
        public Guid? PromotionId { get; set; }
    }
}
