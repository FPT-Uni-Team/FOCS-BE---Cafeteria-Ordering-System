using System.ComponentModel.DataAnnotations;

namespace FOCS.Common.Models
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
