using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class UpdateRoleRequest
    {
        [Required]
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [Required]
        [JsonPropertyName("user_to_update_id")]
        public string UserToUpdateId { get; set; }
    }

}
