using FOCS.Common.Enums;
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class CartItem : IAuditable
    {
        public int Id { get; set; }
        
        public Guid MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }

        public CartStatus CartStatus { get; set; }

        public Guid MenuItemVariantId { get; set; }
        public MenuItemVariant Variant { get; set; }

        public Guid TableId { get; set; }
        public Table Table { get; set; }
        
        public int Quantity { get; set; }

        public string Note { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
