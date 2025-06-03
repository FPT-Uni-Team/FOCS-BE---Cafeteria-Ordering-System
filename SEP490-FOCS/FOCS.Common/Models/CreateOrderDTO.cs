using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class CreateOrderDTO
    {
        public Guid StoreId { get; set; }
        public List<OrderItemDTO> Items { get; set; }
        public string? Note { get; set; }
        public string? CouponCode { get; set; }
    }
}
