using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface ICouponService
    {
        Task IsValidApplyCouponAsync(string couponCode, Guid storeId);
    }
}
