using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class MenuItem : IAuditable
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public string Images { get; set; }

        public double BasePrice { get; set; }

        public bool IsAvailable {  get; set; }

        public bool IsDeleted {  get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Data Foreign Key: Store
        public Guid StoreId {  get; set; }

        public Store Store { get; set; }

        public ICollection<MenuItemVariantGroup> MenuItemVariantGroups { get; set; }
    }
}
