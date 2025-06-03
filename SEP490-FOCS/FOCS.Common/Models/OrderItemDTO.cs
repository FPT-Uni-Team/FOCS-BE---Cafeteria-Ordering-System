using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class OrderItemDTO
    {
        public Guid MenuItemId { get; set; }
        public Guid? VariantId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }
}
