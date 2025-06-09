using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IPromotionService
    {
        Task IsValidPromotionCouponAsync(string couponCode, string userId, Guid storeId);
    }
}
