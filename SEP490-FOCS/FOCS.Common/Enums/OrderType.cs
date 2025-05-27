using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Enums
{
    public enum OrderType
    {
        /// <summary>
        /// Khách ăn tại chỗ, order qua QR code hoặc nhân viên nhập.
        /// </summary>
        DineIn = 0,

        /// <summary>
        /// Khách đến lấy trực tiếp tại quầy.
        /// </summary>
        TakeAway = 1,

        /// <summary>
        /// Giao hàng tận nơi.
        /// </summary>
        Delivery = 2,

        /// <summary>
        /// Đơn đặt trước (pre-order cho thời điểm tương lai).
        /// </summary>
        PreOrder = 3
    }

}
