using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class OrderDetail
    {
        public Guid Id { get; set; }
        
        public int Quantity { get; set; }

        public double UnitPrice { get; set; } // base price

        public double TotalPrice { get; set; } // total: base price + variants price

        public string Note { get; set; }

        public Guid MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }

        public ICollection<MenuItemVariant>? Variants {  get; set; }

        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }    
    }
}
