using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Enums
{
    public enum OrderWrapStatus
    {
        /// <summary>
        /// Đơn gộp vừa được tạo
        /// </summary>
        Created = 0,

        /// <summary>
        /// Một số đơn con đang trong quá trình xử lý
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Tất cả các đơn con đã hoàn thành
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Một số hoặc tất cả đơn con bị hủy
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// Đơn gộp đã thanh toán toàn bộ
        /// </summary>
        Paid = 4,

        /// <summary>
        /// Đơn gộp đã hoàn tất & thanh toán
        /// </summary>
        Finalized = 5
    }

}
