using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class StoreSetting : IAuditable
    {
        public Guid Id { get; set; }

        public TimeSpan OpenTime { get; set; }

        public TimeSpan CloseTime { get; set; }

        public string Currency {  get; set; }

        public PaymentConfig PaymentConfig { get; set; }
        
        public string logoUrl { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        
        public Guid StoreId { get; set; }

        public Store Store { get; set; }


    }
}
