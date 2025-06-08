using FOCS.Common.Enums;
using FOCS.Order.Infrastucture.Entities;
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
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("promotion_type")]
        public PromotionType PromotionType { get; set; }

        [JsonPropertyName("discount_value")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount Value must be greater than 0")]
        public double? DiscountValue { get; set; }

        [JsonPropertyName("is_active")]
        public bool? IsActive { get; set; }

        [JsonPropertyName("accept_for_items")]
        public string? AcceptForItems { get; set; }

        [JsonPropertyName("promotion_item_condition")]
        public PromotionItemConditionDTO? PromotionItemConditionDTO { get; set; }

        [JsonPropertyName("store_id")]
        public Guid StoreId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (StartDate.Date < DateTime.UtcNow.Date)
            {
                results.Add(new ValidationResult(
                    "Start Date cannot be in the past",
                    new[] { nameof(StartDate) }));
            }

            // Validate Start Date must be before End Date
            if (StartDate > EndDate)
            {
                results.Add(new ValidationResult(
                    "Start Date must be before End Date",
                    new[] { nameof(StartDate), nameof(EndDate) }));
            }

            // Validate Discount Value for Percentage type
            if (PromotionType.Equals(PromotionType.Percentage) && DiscountValue > 100)
            {
                results.Add(new ValidationResult(
                    "Discount Value cannot exceed 100% for percentage discount type",
                    new[] { nameof(DiscountValue) }));
            }

            if (PromotionType == PromotionType.FixedAmount && DiscountValue <= 0)
            {
                results.Add(new ValidationResult(
                    "Valid Discount Value is required for Discount promotion type",
                    new[] { nameof(DiscountValue) }));
            }

            return results;
        }
    }
}
