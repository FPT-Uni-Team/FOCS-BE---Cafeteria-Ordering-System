using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class CreateMenuItemWithVariantRequest
    {
        #region menu item
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("images")]
        public string Images { get; set; }

        [JsonPropertyName("base_price")]
        public double BasePrice { get; set; }

        [JsonPropertyName("is_available")]
        public bool IsAvailable { get; set; }

        [JsonPropertyName("store_id")]
        public Guid StoreId { get; set; }

        [JsonPropertyName("variant_groups")]
        public List<VariantGroupRequest> VariantGroupRequests { get; set; }
        #endregion
    }

    public class VariantGroupRequest
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("is_required")]
        public bool IsRequired { get; set; }

        [JsonPropertyName("min_select")]
        public int MinSelect { get; set; }

        [JsonPropertyName("max_select")]
        public int MaxSelect { get; set; }

        [JsonPropertyName("variant_ids")]
        public List<Guid> VariantIds {  get; set; }
    }
}
