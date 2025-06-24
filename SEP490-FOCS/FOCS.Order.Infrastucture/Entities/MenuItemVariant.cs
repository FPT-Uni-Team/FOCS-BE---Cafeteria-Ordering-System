using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class MenuItemVariant : IAuditable
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public double Price { get; set; }

        public int PrepPerTime { get; set; }

        public int QuantityPerTime { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsDeleted {  get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        //Foreign key: variant group
        public Guid? VariantGroupId { get; set; }
        public VariantGroup? VariantGroup { get; set; }
    }
}
