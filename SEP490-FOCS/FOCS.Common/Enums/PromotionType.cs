using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Enums
{
    public enum PromotionType
    {
        Percentage = 0,        // Giảm theo phần trăm (vd: 10%)
        FixedAmount = 1,       // Giảm số tiền cố định (vd: 50,000đ)
        FreeItem = 2,          // Mua hàng tặng sản phẩm (vd: Mua 1 tặng 1)
        FreeShipping = 3,      // Miễn phí vận chuyển
        BuyXGetY = 4,          // Mua X sản phẩm tặng Y sản phẩm
        TimeBased = 5,         // Khuyến mãi theo khung giờ
        FirstTimeBuyer = 6,    // Áp dụng cho khách hàng lần đầu
        Loyalty = 7            // Áp dụng cho khách hàng thân thiết
    }
}
