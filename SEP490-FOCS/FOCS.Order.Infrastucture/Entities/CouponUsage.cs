using FOCS.Infrastructure.Identity.Identity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class CouponUsage
    {
        public Guid Id { get; set; }
        public Guid CouponId { get; set; }
        public Guid? UserId { get; set; } // Nếu có đăng nhập
        public User? User { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        public DateTime UsedAt { get; set; }
    }
}
