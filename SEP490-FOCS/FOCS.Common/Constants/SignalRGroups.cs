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
        public static string Cashier(Guid storeid, Guid tableId) => $"Cashier_{storeid}_{tableId}";
        public static string Admin(Guid storeid) => $"Admin_{storeid}";

        public static string User(Guid storeId, Guid tableId, Guid userId) => $"User/store:{storeId}/table:{tableId}/user:{userId}";
        
        public static class ActionHub
        {
            public static string UpdateCart = "Update Cart";
        }
    }

}
