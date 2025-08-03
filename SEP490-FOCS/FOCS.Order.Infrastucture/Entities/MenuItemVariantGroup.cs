using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class MenuItemVariantGroup
    {
        public Guid Id { get; set; }

        public Guid MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }

        public Guid VariantGroupId { get; set; }
        public VariantGroup VariantGroup { get; set; }

        public int MinSelect { get; set; }
        public int MaxSelect { get; set; }
        public bool IsRequired { get; set; }

        public ICollection<MenuItemVariantGroupItem> MenuItemVariantGroupItems { get; set; }

    }
}
