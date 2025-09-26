using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static FOCS.Common.Exceptions.Errors;

namespace FOCS.Common.Models
{
    public class OrderDetailDTO
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("menu_item_id")]
        public Guid MenuItemId { get; set; }

        [JsonPropertyName("menu_item_name")]
        public string MenuItemName { get; set; }

        [JsonPropertyName("variants")]
        public List<OrderDetailVariantDTO> Variants { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public double UnitPrice { get; set; } // base price

        [JsonPropertyName("images")]
        public List<ImageDto>? Images { get; set; } = null;

        [JsonPropertyName("total_price")]
        public double TotalPrice { get; set; } // total: base price + variants price

        [JsonPropertyName("note")]
        public string Note { get; set; }
    }

    public class OrderDetailVariantDTO
    {
        [JsonPropertyName("variant_id")]
        public Guid VariantId { get; set; }

        [JsonPropertyName("variant_name")]
        public string VariantName { get; set; }
    }
}
