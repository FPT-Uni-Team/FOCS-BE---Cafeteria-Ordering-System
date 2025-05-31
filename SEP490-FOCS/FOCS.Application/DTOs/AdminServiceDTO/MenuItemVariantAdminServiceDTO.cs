using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class MenuItemVariantAdminServiceDTO
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("prep_per_time")]
        public int PrepPerTime { get; set; }

        [JsonPropertyName("quantity_per_time")]
        public int QuantityPerTime { get; set; }

        [JsonPropertyName("is_available")]
        public bool IsAvailable { get; set; }
    }
}
