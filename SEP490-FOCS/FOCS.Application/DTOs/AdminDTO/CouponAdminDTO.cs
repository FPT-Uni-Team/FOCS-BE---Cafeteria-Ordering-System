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

        [JsonPropertyName("coupon_type")]
        public CouponType CouponType { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("discount_type")]
        public DiscountType DiscountType { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("status")]
        public CouponStatus Status
        {
            get
            {
                var now = DateTime.UtcNow;
                if (!IsActive)
                    return CouponStatus.UnAvailable;
                if (now < StartDate)
                    return CouponStatus.Incomming;
                if (now > EndDate)
                    return CouponStatus.Expired;
                return CouponStatus.On_going;
            }
        }

        [JsonPropertyName("max_usage")]
        public int MaxUsage { get; set; }

        //[JsonPropertyName("count_used")]
        //public int CountUsed { get; set; }

        //[JsonPropertyName("max_usage_per_user")]
        //public int? MaxUsagePerUser { get; set; }

        [JsonPropertyName("accept_for_items")]
        public List<string>? AcceptForItems { get; set; }

        [JsonPropertyName("minimum_order_amount")]
        public double? MinimumOrderAmount { get; set; }

        [JsonPropertyName("minimum_item_quantity")]
        public int? MinimumItemQuantity { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("store_id")]
        [JsonIgnore]
        public string? StoreId { get; set; }

        [JsonPropertyName("promotion_id")]
        public Guid? PromotionId { get; set; }
    }
}
