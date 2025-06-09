using FOCS.Common.Enums;
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class Promotion : IAuditable
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        
        public string? Description { get; set; }

        public PromotionType PromotionType { get; set; }

        public double? DiscountValue { get; set; }

        public List<Guid>? AcceptForItems {  get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Foreign key store
        public Guid StoreId { get; set; }
        public Store Store { get; set; }

        public ICollection<Coupon>? Coupons { get; set; }
        public ICollection<PromotionItemCondition>? PromotionItemConditions { get; set; }

    }
}
