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
        public Guid id {  get; set; }

        public string name { get; set; } // siZe, topping, ...

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public Guid MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }
        
        public ICollection<MenuItemVariant> Variants { get; set; }
    }
}
