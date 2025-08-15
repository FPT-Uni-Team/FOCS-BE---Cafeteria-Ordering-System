using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FOCS.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs
{
    public class UserProfileDTO
    {
        [EmailAddress]
        [Required]
        public string? Email { get; set; }

        [JsonPropertyName("phone_number")]
        [Required]
        [StringLength(11, ErrorMessage = "Phone number must be 10 to 11 digits.", MinimumLength = 10)]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must be 10 to 11 digits.")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("first_name")]
        public string? Firstname { get; set; }

        [JsonPropertyName("last_name")]
        public string? Lastname { get; set; }
    }
}
