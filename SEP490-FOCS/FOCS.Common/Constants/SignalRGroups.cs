using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Constants
{
    public static class SignalRGroups
    {
        public static string Kitchen(Guid storeid) => $"Kitchen_{storeid}";
        public static string Cashier(Guid storeid) => $"Cashier_{storeid}";
        public static string Admin(Guid storeid) => $"Admin_{storeid}";

        public static string User(Guid storeId, Guid tableId) => $"store:{storeId}:table:{tableId}";
        
        public static class ActionHub
        {
            public static string UpdateCart = "UpdateCart";
        }
    }

}
