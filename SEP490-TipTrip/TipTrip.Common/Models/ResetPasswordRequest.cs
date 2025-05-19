using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TipTrip.Common.Models
{
    public class ResetPasswordRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
        
        [JsonPropertyName("token")]
        public string Token { get; set; }
        
        [JsonPropertyName("new_password")]
        public string NewPassword { get; set; }
    }
}
