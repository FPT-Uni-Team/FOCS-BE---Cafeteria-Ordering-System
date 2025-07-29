using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class FeedbackCreateDTO
    {
        [JsonPropertyName("order_id")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("rating")]
        public int Rating { get; set; } 

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        [JsonPropertyName("images")]
        public List<string>? Images { get; set; }
    }
}
