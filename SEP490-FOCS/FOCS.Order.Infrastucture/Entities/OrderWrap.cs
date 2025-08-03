using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class OrderWrap
    {
        public Guid Id { get; set; }

        public OrderWrapStatus OrderWrapStatus { get; set; }

        public Guid StoreId { get; set; }
        public Store Store { get; set; }

        // Order
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
