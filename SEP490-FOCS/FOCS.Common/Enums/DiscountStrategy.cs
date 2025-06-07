using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Enums
{
    public enum DiscountStrategy
    {
        CouponOnly,           // Apply for only
        PromotionOnly,        // apply for only promotion
        CouponThenPromotion,  // APply: coupon first, after that promotion
        MaxDiscountOnly       // Choose max discount between coupon and promotion
    }

}
