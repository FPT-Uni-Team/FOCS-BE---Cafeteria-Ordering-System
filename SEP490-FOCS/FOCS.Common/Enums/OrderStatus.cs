using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Enums
{
    public enum OrderStatus
    {
        /// <summary>
        /// Đơn hàng được tạo nhưng chưa xác nhận.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Đơn hàng đã được xác nhận và đang trong quá trình xử lý (bếp chuẩn bị món).
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// Món ăn đã được nấu xong, chờ giao đến bàn hoặc giao đi.
        /// </summary>
        Ready = 2,

        /// <summary>
        /// Đơn hàng đang được giao cho khách (dành cho Delivery).
        /// </summary>
        Delivering = 3,

        /// <summary>
        /// Đơn hàng đã hoàn thành (đã giao hoặc khách ăn xong).
        /// </summary>
        Completed = 4,

        /// <summary>
        /// Đơn hàng bị huỷ bởi khách hoặc hệ thống.
        /// </summary>
        Canceled = 5,

        /// <summary>
        /// Đơn hàng bị từ chối bởi hệ thống (hết món, sai QR, v.v.).
        /// </summary>
        Rejected = 6
    }

}
