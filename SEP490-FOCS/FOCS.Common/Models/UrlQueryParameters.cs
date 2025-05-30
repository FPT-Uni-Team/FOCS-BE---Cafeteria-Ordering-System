using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class UrlQueryParameters
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }
        [JsonPropertyName("search_by")]
        public string? SearchBy { get; set; }
        [JsonPropertyName("search_value")]
        public string? SearchValue { get; set; }
        [JsonPropertyName("sort_by")]
        public string? SortBy { get; set; }
        [JsonPropertyName("sort_order")]
        public string? SortOrder { get; set; }
        [JsonPropertyName("filters")]
        public Dictionary<string, string>? Filters { get; set; }

    }
}
