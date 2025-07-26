using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class PaymentAccount : IAuditable
    {
        public Guid Id { get; set; }
        public Guid StoreId { get; set; }

        public string BankName { get; set; } = string.Empty; 
        public string BankCode { get; set; } = string.Empty; 
        public string AccountNumber { get; set; } = string.Empty; 
        public string AccountName { get; set; } = string.Empty; 

        public bool IsActive { get; set; } = true;

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
