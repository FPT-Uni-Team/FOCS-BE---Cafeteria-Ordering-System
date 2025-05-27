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

        public double Price { get; set; }

        public string Note { get; set; }


    }
}
