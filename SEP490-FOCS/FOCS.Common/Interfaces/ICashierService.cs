using FOCS.Common.Enums;
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface ICashierService
    {
        Task<PagedResult<OrderDTO>> GetOrders(UrlQueryParameters query, string storeId);

        Task<bool> UpdatePaymentStatus(Guid orderId, UpdatePaymentStatusRequest updatePaymentStatus, string storeId);
    }
}
