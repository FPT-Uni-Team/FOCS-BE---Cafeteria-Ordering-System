using FOCS.Common.Models;
using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs
{
    public class VariantGroupResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("group_name")]
        public string Name { get; set; }

        [JsonPropertyName("is_required")]
        public bool IsRequired { get; set; }

        [JsonPropertyName("min_select")]
        public int MinSelect { get; set; }

        [JsonPropertyName("max_select")]
        public int MaxSelect { get; set; }

        [JsonPropertyName("prep_per_time")]
        public int? PrepPerTime { get; set; }

        [JsonPropertyName("quantity_pre_time")]
        public int? QuantityPerTime {  get; set; }

        [JsonPropertyName("variants")]
        public List<MenuItemVariantDTO> Variants { get; set; }
    }
}
