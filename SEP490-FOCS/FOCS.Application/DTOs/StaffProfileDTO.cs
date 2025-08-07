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
    public class StaffProfileDTO
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [JsonPropertyName("phone_number")]
        [StringLength(11, ErrorMessage = "Phone number must be 10 to 11 digits.", MinimumLength = 10)]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must be 10 to 11 digits.")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("roles")]
        public IList<string>? Roles { get; set; }
    }
}
