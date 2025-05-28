using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Constants
{
    public static class SignalRGroups
    {
        public static string Kitchen(Guid storeid) => $"Kitchen_{storeid}";
        public static string Cashier(Guid storeid) => $"Cashier_{storeid}";
        public static string Admin(Guid storeid) => $"Admin_{storeid}";
    }
}
