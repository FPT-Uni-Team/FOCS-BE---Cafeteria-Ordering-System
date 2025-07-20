using FOCS.Common.Enums;
using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs
{
    public class TableDTO
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }
        
        [JsonPropertyName("table_number")]
        public int TableNumber { get; set; }

        [JsonPropertyName("status")]
        public TableStatus Status { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
        
        [JsonPropertyName("store_id")]
        public Guid StoreId { get; set; }
    }
}
