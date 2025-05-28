using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Phone {  get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string ConfirmPassword { get; set; }

        public string StoreId { get; set; }
    }
}
