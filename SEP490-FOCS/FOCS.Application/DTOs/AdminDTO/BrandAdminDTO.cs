using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class BrandAdminDTO
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("default_tax_rate")]
        [Range(0.01, 1.0, ErrorMessage = "Default Tax Rate must between 0.01 and 1")]
        public double DefaultTaxRate { get; set; }
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
    }
}
