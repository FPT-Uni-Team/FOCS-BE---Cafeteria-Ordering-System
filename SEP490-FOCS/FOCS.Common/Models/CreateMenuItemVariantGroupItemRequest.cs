using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class CreateMenuItemVariantGroupItemRequest
    {
        [JsonPropertyName("menu_item_variant_group_item")]
        public List<MenuItemVariantGroupItemRequest> MenuItemVariantGroupItemRequests { get; set; }
    }

    public class MenuItemVariantGroupItemRequest
    {
        [JsonPropertyName("menu_item_variant_group_id")]
        public Guid MenuItemVariantGroupId { get; set; }


        [JsonPropertyName("variants")]
        public List<VariantRequest> Variants { get; set; }
    }
}
