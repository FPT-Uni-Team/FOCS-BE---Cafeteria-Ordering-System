using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class VariantGroup : IAuditable
    {
        public Guid Id {  get; set; }

        public string Name { get; set; } // size, topping, ...
       
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        
        public ICollection<MenuItemVariant>? Variants { get; set; }
    }
}
