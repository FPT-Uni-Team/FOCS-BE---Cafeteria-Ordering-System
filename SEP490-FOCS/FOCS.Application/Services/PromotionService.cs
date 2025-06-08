using FOCS.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class PromotionService : IPromotionService
    {
        public Task IsValidPromotionCouponAsync(string couponCode, string storeId)
        {
            throw new NotImplementedException();
        }
    }
}
