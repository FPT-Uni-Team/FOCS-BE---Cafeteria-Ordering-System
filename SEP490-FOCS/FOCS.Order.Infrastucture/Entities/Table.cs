using FOCS.Common.Enums;
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class Table : IAuditable
    {
        public Guid Id { get; set; }

        public int TableNumber {  get; set; }

        public string QrCode { get; set; }

        public TableStatus Status { get; set; }

        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public Guid StoreId { get; set; }
        public Store Store { get; set; }

    }
}
