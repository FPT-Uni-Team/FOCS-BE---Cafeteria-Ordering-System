using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models.CartModels
{
    public class ScanModelResponse
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("remove_id")]
        public Guid RemovedId { get; set; }
    }
}
