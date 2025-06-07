using FOCS.Common.Enums;
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
        public Guid Id { get; set; } = Guid.NewGuid();

        public TimeSpan OpenTime { get; set; } = new TimeSpan(0, 0, 0); // Default to start of day if not set

        public TimeSpan CloseTime { get; set; } = new TimeSpan(23, 59, 59); // Default to end of day if not set

        public string Currency {  get; set; } = "VND";

        public PaymentConfig PaymentConfig { get; set; } = Entities.PaymentConfig.Momo;

        public string logoUrl { get; set; } = "";

        public bool IsSelfService { get; set; } = true;

        public DiscountStrategy discountStrategy { get; set; } = DiscountStrategy.CouponThenPromotion;

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public Guid StoreId { get; set; }

        public Store Store { get; set; }
    }
}
