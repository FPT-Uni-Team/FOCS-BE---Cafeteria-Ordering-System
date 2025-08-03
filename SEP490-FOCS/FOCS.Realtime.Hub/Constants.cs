using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Realtime.Hubs
{
    public class Constants
    {
        public class Method
        {
            public const string ReceiveOrderWrapUpdate = "ReceiveOrderWrapUpdate";
            public const string OrderCreated = "OrderCreated";
            public const string NewNotify = "NewNotify";
        }

        public class ActionTitle
        {
            public const string NewOrderd = "Có đơn mới";

            public static string PaymentSuccess(int tableNumber) => $"Bàn {tableNumber} thanh toán thành công";
            public static string NewOrderAtTable(int tableId) => $"Bàn {tableId} vừa tạo đơn mới";
        }
    }
}
