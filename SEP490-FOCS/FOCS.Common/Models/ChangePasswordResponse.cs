using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class ChangePasswordResponse
    {
        [JsonPropertyName("is_succes")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("errors")]
        public IEnumerable<string> Errors { get; set; }
    }
}
