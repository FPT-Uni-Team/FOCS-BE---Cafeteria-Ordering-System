using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class MenuItemVariantGroupItem
    {
        public Guid Id { get; set; }

        public Guid MenuItemVariantGroupId { get; set; }
        public MenuItemVariantGroup MenuItemVariantGroup { get; set; }

        public Guid MenuItemVariantId { get; set; }
        public MenuItemVariant MenuItemVariant { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsAvailable { get; set; } = true;

        public int PrepPerTime { get; set; }

        public int QuantityPerTime { get; set; }
    }
}
