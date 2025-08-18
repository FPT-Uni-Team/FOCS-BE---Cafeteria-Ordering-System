using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class CreateAdminBrandRequest
    {
        [JsonPropertyName("name")]
        [Required]
        public string Name { get; set; }

        [JsonPropertyName("default_tax_rate")]
        [Range(0.01, 1.0, ErrorMessage = "Default Tax Rate must between 0.01 and 1")]
        public double DefaultTaxRate { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
    }
}
