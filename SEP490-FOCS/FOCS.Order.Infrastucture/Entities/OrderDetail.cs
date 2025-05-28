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

        public string Variants { get; set; } // save with string format or json format. exmaple: "size L, trung" -> after that split(",") -> List variants and check with db variants list
    }
}
