using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class RemoveVariantGroupFromProduct
    {
        [JsonPropertyName("variant-group-ids")]
        public List<Guid> VariantGroupIds { get; set; }
    }
}
