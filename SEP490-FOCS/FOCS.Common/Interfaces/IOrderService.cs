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
        Task<List<OrderDTO>> GetPendingOrdersAsync();

        #region guest

        Task<DiscountResultDTO> CreateOrderAsync(CreateOrderRequest dto, string userId);

        Task<OrderDTO> GetOrderByCodeAsync(string orderCode);

        Task<DiscountResultDTO> VerifyCouponAsGuestAsync(string couponCode, Guid storeId);

        #endregion

        #region user

        Task<IEnumerable<OrderSummaryDTO>> GetUserOrdersAsync(Guid userId);

        Task<OrderDTO> GetUserOrderDetailAsync(Guid userId, Guid orderId);

        #endregion
    }
}
