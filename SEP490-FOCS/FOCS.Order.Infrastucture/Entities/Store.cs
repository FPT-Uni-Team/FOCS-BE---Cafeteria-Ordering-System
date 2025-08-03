using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class Store : IAuditable
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Address {  get; set; }

        public string PhoneNumber { get; set; }

        public double? CustomTaxRate { get; set; }
        
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        [JsonIgnore]
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        public ICollection<UserStore> UserStores { get; set; }
        public double GetEffectiveTaxRate()
        {
            return CustomTaxRate ?? Brand?.DefaultTaxRate ?? 0.0;
        }

    }
}
