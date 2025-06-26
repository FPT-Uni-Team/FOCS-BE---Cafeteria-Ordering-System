using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Utils;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class PromotionDTO
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("status")]
        public PromotionStatus Status
        {
            get
            {
                if (IsActive == false || CountUsed >= MaxUsage) return PromotionStatus.UnAvailable;
                var now = DateTime.UtcNow;
                if (StartDate > now) return PromotionStatus.Incomming;
                if (EndDate < now) return PromotionStatus.Expired;
                return PromotionStatus.OnGoing;
            }
        }


        [JsonPropertyName("promotion_scope")]
        public PromotionScope PromotionScope { get; set; } = PromotionScope.Order;

        [JsonPropertyName("max_discount_value")]
        public double? MaxDiscountValue { get; set; }

        [JsonPropertyName("max_usage")]
        public int? MaxUsage { get; set; }

        [JsonPropertyName("count_used")]
        public int? CountUsed { get; set; }

        [JsonPropertyName("max_usage_per_user")]
        public int? MaxUsagePerUser { get; set; }

        [JsonPropertyName("minimum_order_amount")]
        public double? MinimumOrderAmount { get; set; }

        [JsonPropertyName("minimum_item_quantity")]
        public int? MinimumItemQuantity { get; set; }

        [JsonPropertyName("can_apply_combine")]
        public bool? CanApplyCombine { get; set; } = true;

        [JsonPropertyName("promotion_type")]
        public PromotionType PromotionType { get; set; }

        [JsonPropertyName("discount_value")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount Value must be greater than 0")]
        public double? DiscountValue { get; set; }
        [JsonPropertyName("is_active")]
        public bool? IsActive { get; set; } = true;

        [JsonPropertyName("accept_for_items")]
        public List<Guid>? AcceptForItems { get; set; }

        [JsonPropertyName("promotion_item_condition")]
        public PromotionItemConditionDTO? PromotionItemConditionDTO { get; set; }

        [JsonPropertyName("coupon_ids")]
        public List<Guid>? CouponIds { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            ConditionCheck.CheckCondition(StartDate.Date > DateTime.UtcNow.Date,
                                                Errors.PromotionError.StartDateInPast, 
                                                Errors.FieldName.StartDate);

            ConditionCheck.CheckCondition(StartDate < EndDate,
                                                Errors.PromotionError.StartDateAfterEndDate,
                                                Errors.FieldName.EndDate);

            ConditionCheck.CheckCondition(StartDate.Date > DateTime.UtcNow.Date,
                                                Errors.PromotionError.MaxPercentageDiscountValue,
                                                Errors.FieldName.DiscountValue);

            if (DiscountValue.HasValue && MaxDiscountValue.HasValue)
            {
                ConditionCheck.CheckCondition(DiscountValue < MaxDiscountValue,
                                                    Errors.PromotionError.DiscountValueExceedMaxValue,
                                                    Errors.FieldName.MaxDiscountValue);
            }

            if (PromotionType == PromotionType.BuyXGetY)
            {
                ConditionCheck.CheckCondition(PromotionItemConditionDTO != null,
                                                    Errors.PromotionError.RequireItemCondition,
                                                    Errors.FieldName.PromotionItemCondition);
            }

            return results;
        }
    }
}
