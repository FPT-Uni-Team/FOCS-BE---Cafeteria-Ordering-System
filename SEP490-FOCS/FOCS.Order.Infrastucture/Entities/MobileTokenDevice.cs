using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class MobileTokenDevice
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public string DeviceId { get; set; }
        public string Platform { get; set; } // iOS / Android
        public Guid? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }
}
