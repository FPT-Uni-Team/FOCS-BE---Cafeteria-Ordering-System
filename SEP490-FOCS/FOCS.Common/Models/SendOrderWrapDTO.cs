using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class SendOrderWrapDTO
    {
        [JsonPropertyName("menu_item_id")]
        public Guid MenuItemId { get; set; }

        [JsonPropertyName("menu_item_name")]
        public string MenuItemName { get; set; }

        [JsonPropertyName("variants")]
        public List<VariantWrapOrder> Variants { get; set; }
    }

    public class VariantWrapOrder
    {
        [JsonPropertyName("variant_id")]
        public Guid? VariantId { get; set; }

        [JsonPropertyName("variant_name")]
        public string? VariantName { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }
    }
}
