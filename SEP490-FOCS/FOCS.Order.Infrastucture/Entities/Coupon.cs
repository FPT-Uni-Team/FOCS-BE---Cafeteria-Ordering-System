using FOCS.Common.Enums;
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class Coupon : IAuditable
    {
        public Guid Id { get; set; } 

        public string Code { get; set; }
        public CouponType CouponType { get; set; }

        public string Description { get; set; }

        public DiscountType DiscountType { get; set; } 

        public double Value { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int MaxUsage { get; set; } 
        public int CountUsed { get; set; }

        public int? MaxUsagePerUser { get; set; } 

        public List<string>? AcceptForItems { get; set; }

        public double? MinimumOrderAmount { get; set; }
        public int? MinimumItemQuantity { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Foreign key store
        public Guid StoreId { get; set; }
        public Store Store { get; set; }

        //promotion
        public Guid? PromotionId {  get; set; }
        public Promotion? Promotion { get; set; }
    }

}
