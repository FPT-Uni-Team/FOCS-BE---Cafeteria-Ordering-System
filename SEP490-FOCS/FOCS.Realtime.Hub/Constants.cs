using Microsoft.AspNetCore.Routing;
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
            public const string NewOrderd = "New order created";

            public static string PaymentSuccess(int tableNumber) => $"Table {tableNumber} payment successful!";

            public static string NewOrderAtTable(int tableId) => $"Table {tableId} create new order!";

            public const string PushStaff = "Push";

            public static string ReceiveNotify(string tableNumber) => $"Table {tableNumber} need some help.";
        }
    }
}
