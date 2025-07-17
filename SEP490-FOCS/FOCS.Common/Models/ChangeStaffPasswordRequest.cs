using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FOCS.Common.Models
{
    public class ChangeStaffPasswordRequest
    {
        public string StaffId { get; set; }

        [Required]
        [JsonPropertyName("new_password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }


        [JsonPropertyName("confirm_password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
