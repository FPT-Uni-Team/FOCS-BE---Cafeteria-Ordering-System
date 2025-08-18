using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface ICouponUsageService
    {
        Task<bool> SaveCouponUsage(string couponCode, Guid userId, Guid orderId);

        Task<PagedResult<CouponUsageResponse>> GetList(UrlQueryParameters urlQuery, string storeId);
    }
}
