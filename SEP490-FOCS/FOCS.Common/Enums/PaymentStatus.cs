using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Enums
{
    public enum PaymentStatus
    {
        /// <summary>
        /// Chưa thanh toán
        /// </summary>
        Unpaid = 0,

        /// <summary>
        /// Đã thanh toán đầy đủ
        /// </summary>
        Paid = 1,

        /// <summary>
        /// Đã thanh toán một phần (cọc trước hoặc thanh toán nhiều phần)
        /// </summary>
        PartiallyPaid = 2,

        /// <summary>
        /// Thanh toán thất bại (qua cổng online)
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Đã hoàn tiền
        /// </summary>
        Refunded = 4,


        Waiting
    }

}
