using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class VariantGroupDTO
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("variant")]
        public ICollection<MenuItemVariantDTO> Variants { get; set; }
    }
}
