using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class ImageSycnMetaData
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [JsonPropertyName("is_main")]
        public bool IsMain { get; set; }

        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
