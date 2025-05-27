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

        public int Quantity { get; set; }

        public string DisplayOrderWrap { get; set; } // save with json format

        public Guid StoreId { get; set; }
        public Store Store { get; set; }
    }
}
