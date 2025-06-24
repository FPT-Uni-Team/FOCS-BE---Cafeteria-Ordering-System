using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class AddVariantToGroupRequest
    {
        [JsonPropertyName("menu_item_id")]
        public Guid MenuItemId { get; set; }

        [JsonPropertyName("group_name")]
        public string GroupName { get; set; } // Eg: "Size", "Topping"

        [JsonPropertyName("variant_ids")]
        public List<Guid> VariantIds { get; set; }

        [JsonPropertyName("is_required")]
        public bool IsRequired { get; set; }

        [JsonPropertyName("min_select")]
        public int MinSelect { get; set; }

        [JsonPropertyName("max_select")]
        public int MaxSelect { get; set; }
    }

    public class UpdateGroupSettingRequest
    {
        [JsonPropertyName("is_required")]
        public bool IsRequired { get; set; }

        [JsonPropertyName("min_select")]
        public int MinSelect { get; set; }

        [JsonPropertyName("max_select")]
        public int MaxSelect { get; set; }
    }

    public class VariantGroupDetailDTO
    {
        [JsonPropertyName("group_name")]
        public string GroupName { get; set; }

        [JsonPropertyName("is_required")]
        public bool IsRequired { get; set; }

        [JsonPropertyName("min_select")]
        public int MinSelect { get; set; }

        [JsonPropertyName("max_select")]
        public int MaxSelect { get; set; }

        [JsonPropertyName("variants")]
        public List<VariantOptionDTO> Variants { get; set; }
    }

    public class VariantOptionDTO
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
