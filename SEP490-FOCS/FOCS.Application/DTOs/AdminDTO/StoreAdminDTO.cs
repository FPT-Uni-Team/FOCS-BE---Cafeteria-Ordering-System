using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class StoreAdminDTO
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("phone_number")]
        [StringLength(11, ErrorMessage = "Phone number must have 10 to 11 digits.", MinimumLength = 10)]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must be digits.")]
        public string PhoneNumber { get; set; }
        [JsonPropertyName("custom_tax_rate")]
        [Range(0.01, 1.0, ErrorMessage = "Custom Tax Rate must between 0.01 and 1")]
        public double? CustomTaxRate { get; set; }
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
        [JsonPropertyName("brand_id")]
        public Guid BrandId { get; set; }
    }
}
