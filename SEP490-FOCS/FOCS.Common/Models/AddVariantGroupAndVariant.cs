using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class AddVariantGroupsAndVariants
    {
        [JsonPropertyName("variant_groups_variants")]
        public List<AddVariantGroupAndVariant> VariantGroupsAndVariants { get; set; }
    }

    public class AddVariantGroupAndVariant
    {
        [JsonPropertyName("id")]
        public Guid VariantGroupId { get; set; }

        [JsonPropertyName("min_select")]
        public int MinSelect {  get; set; }

        [JsonPropertyName("max_select")]
        public int MaxSelect { get; set; }

        [JsonPropertyName("is_required")]
        public bool IsRequired { get; set; }

        [JsonPropertyName("variants")]
        public List<VariantRequest> Variants { get; set; }
    }
}
