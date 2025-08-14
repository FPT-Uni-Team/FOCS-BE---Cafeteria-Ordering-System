using FOCS.Common.Enums;
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class OrderWrap : IAuditable
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public OrderWrapStatus OrderWrapStatus { get; set; }

        public Guid StoreId { get; set; }
        public Store Store { get; set; }

        // Order
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public DateTime? CreatedAt { get ; set ; }
        public string? CreatedBy { get ; set ; }
        public DateTime? UpdatedAt { get ; set ; }
        public string? UpdatedBy { get ; set; }
    }
}
