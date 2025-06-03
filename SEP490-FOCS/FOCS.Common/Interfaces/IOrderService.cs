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

        Task<OrderResultDTO> CreateOrderAsGuestAsync(CreateOrderGuestDTO dto);

        Task<OrderDetailDTO> GetOrderByCodeAsync(string orderCode);

        Task<DiscountResultDTO> VerifyCouponAsGuestAsync(string couponCode, Guid storeId);

        Task SubmitFeedbackAsGuestAsync(OrderFeedbackDTO dto);

        #endregion

        #region user
        Task<OrderResultDTO> CreateOrderAsync(Guid userId, CreateOrderDTO dto);

        Task<IEnumerable<OrderSummaryDTO>> GetUserOrdersAsync(Guid userId);

        Task<OrderDetailDTO> GetUserOrderDetailAsync(Guid userId, Guid orderId);

        Task<DiscountResultDTO> ApplyCouponAsync(Guid userId, string couponCode, Guid storeId);

        Task SubmitFeedbackAsync(Guid userId, OrderFeedbackDTO dto);

        #endregion
    }
}
