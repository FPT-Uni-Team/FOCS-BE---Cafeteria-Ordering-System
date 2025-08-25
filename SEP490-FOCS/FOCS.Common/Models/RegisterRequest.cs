using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [StringLength(11, ErrorMessage = "Phone number must be 10 to 11 digits.", MinimumLength = 10)]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must be 10 to 11 digits.")]
        public string? Phone {  get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [DataType(DataType.Password)]
        [JsonPropertyName("confirm_password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
