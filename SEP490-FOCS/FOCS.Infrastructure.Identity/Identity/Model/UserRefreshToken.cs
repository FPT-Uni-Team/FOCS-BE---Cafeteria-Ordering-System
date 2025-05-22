using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Infrastructure.Identity.Identity.Model
{
    public class UserRefreshToken
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpirationDate { get; set; }
        public bool IsRevoked { get; set; } = false;

        public User User { get; set; } = null!;
    }
}
