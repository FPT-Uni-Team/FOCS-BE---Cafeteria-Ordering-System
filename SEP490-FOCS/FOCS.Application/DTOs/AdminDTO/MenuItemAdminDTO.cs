using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class MenuItemAdminDTO
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("images")]
        public string Images { get; set; }
        [JsonPropertyName("base_price")]
        public double BasePrice { get; set; }
        [JsonPropertyName("is_available")]
        public bool IsAvailable { get; set; }

        [JsonPropertyName("menu_category_id")]
        public Guid MenuCategoryId { get; set; }
        [JsonPropertyName("store_id")]
        public Guid StoreId { get; set; }
    }
}
