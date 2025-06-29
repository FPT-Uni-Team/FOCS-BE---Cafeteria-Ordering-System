using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class ImageDto
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("is_main")]
        public bool IsMain { get; set; }
    }
}
