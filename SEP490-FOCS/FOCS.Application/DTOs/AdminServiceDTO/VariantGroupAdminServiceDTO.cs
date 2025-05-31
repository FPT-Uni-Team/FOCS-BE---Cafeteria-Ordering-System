using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class VariantGroupAdminServiceDTO
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; }
    }
}
