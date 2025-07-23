using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class MobileTokenRequest
    {
        public string Token { get; set; }
        public string DeviceId { get; set; }
        public string Platform { get; set; } // iOS / Android
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }

        public Guid? ActorId { get; set; }
    }
}
