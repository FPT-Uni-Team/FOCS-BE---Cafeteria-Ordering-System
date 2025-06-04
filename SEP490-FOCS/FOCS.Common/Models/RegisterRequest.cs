using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [RegularExpression(@"^(0[3|5|7|8|9])([0-9]{8})$", ErrorMessage = "It must be 10 digits and start with 03, 05, 07, 08, or 09.")]
        public string? Phone {  get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "confirm_password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string StoreId { get; set; }
    }
}
