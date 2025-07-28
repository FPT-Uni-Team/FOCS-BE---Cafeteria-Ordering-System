using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class VariantGroupDTO
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("is_required")]
        public bool IsRequired { get; set; }

        [JsonPropertyName("min_select")]
        public int MinSelect { get; set; }

        [JsonPropertyName("max_select")]
        public int MaxSelect { get; set; }

        [JsonPropertyName("variant")]
        public ICollection<MenuItemVariantDTO> Variants { get; set; }
    }
}
