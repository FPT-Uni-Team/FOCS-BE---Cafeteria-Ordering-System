using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class GetVariantGroupAndVariantResponse
    {
        [JsonPropertyName("Group")]
        public Dictionary<string, List<GetVariantGroupResponse>> Group { get; set; }
    }

    public class GetVariantGroupResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
