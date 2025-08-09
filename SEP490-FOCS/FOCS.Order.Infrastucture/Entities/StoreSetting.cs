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

        public TimeSpan OpenTime { get; set; }

        public TimeSpan CloseTime { get; set; }

        public string Currency {  get; set; } = "VND";

        public PaymentConfig PaymentConfig { get; set; }

        public string LogoUrl { get; set; } = "";

        public bool IsSelfService { get; set; }

        public DiscountStrategy discountStrategy { get; set; }

        public bool AllowCombinePromotionAndCoupon { get; set; } = true;

        public int? SpendingRate { get; set; }

        public string? PayOSClientId { get; set; }
        public string? PayOSApiKey { get; set; }
        public string? PayOSChecksumKey { get; set; }

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
