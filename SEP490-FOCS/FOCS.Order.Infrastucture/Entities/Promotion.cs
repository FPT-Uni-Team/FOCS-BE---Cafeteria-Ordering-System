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

        public PromotionScope PromotionScope { get; set; } = PromotionScope.Order;

        public double MaxDiscountValue { get; set; }

        public int? MaxUsage { get; set; }
        public int? CountUsed { get; set; }

        public int? MaxUsagePerUser { get; set; }

        public double? MinimumOrderAmount { get; set; }
        public int? MinimumItemQuantity { get; set; }

        public bool? CanApplyCombine { get; set; } = true;

        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;

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
