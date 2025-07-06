using FOCS.Common.Models;
using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs
{
    public class VariantGroupResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("variant")]
        public List<MenuItemVariantDTO> Variant { get; set; }
    }
}
