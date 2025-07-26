using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDTO>> GetPendingOrdersInDayAsync();

        #region guest

        Task<DiscountResultDTO> CreateOrderAsync(CreateOrderRequest dto, string userId);

        Task<OrderDTO> GetOrderByCodeAsync(long orderCode);

        Task<DiscountResultDTO> ApplyDiscountForOrder(ApplyDiscountOrderRequest request, string userId);


        #endregion

        #region user


        Task<OrderDTO> GetUserOrderDetailAsync(Guid userId, Guid orderId);
        Task<PagedResult<OrderDTO>> GetListOrders(UrlQueryParameters queryParameters, string storeId, string userId);
        Task<bool> CancelOrderAsync(Guid orderId, string userId, string storeId);
        Task<bool> DeleteOrderAsync(Guid orderId, string userId, string storeId);
        Task MarkAsPaid(long orderCode);

        #endregion
    }
}
