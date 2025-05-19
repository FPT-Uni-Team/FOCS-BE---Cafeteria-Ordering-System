using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TipTrip.Common.Models
{
    public class UserRefreshTokenDTO
    {
        public string UserId { get; set; }
        public string RefreshToken { get; set; }

        public DateTime ExpirationDate { get; set; }
        public bool IsRevoked { get; set; } = false;
    }
}
