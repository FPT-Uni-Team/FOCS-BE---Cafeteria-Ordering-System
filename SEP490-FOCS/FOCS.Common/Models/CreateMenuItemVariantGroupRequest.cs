using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class CreateMenuItemVariantGroupRequest
    {
        [JsonPropertyName("menu_item_id")]
        public Guid? MenuItemId { get; set; }

        [JsonPropertyName("variant_group_ids")]
        public List<Guid>? VariantGroupIds { get; set; }

        [JsonPropertyName("is_required")]
        public bool IsRequired { get; set; }

        [JsonPropertyName("min_select")]
        public int MinSelect { get; set; }

        [JsonPropertyName("max_select")]
        public int MaxSelect { get; set; }
    }
}
