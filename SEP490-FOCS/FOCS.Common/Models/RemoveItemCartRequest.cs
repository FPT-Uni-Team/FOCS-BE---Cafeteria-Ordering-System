using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class RemoveItemCartRequest
    {
        [JsonPropertyName("menu_item_id")]
        public Guid MenuItemId { get; set; }

        [JsonPropertyName("variant_id")]
        public Guid? VariantId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity {  get; set; }
    }
}
