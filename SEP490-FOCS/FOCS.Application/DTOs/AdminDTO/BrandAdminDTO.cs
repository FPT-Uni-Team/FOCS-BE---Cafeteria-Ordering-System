using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class BrandAdminDTO
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("default_tax_rate")]
        public double DefaultTaxRate { get; set; }
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
    }
}
