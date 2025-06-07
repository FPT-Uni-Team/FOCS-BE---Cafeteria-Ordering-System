using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly IRepository<MenuItemVariant> _variantRepository;
        private readonly IRepository<FOCS.Order.Infrastucture.Entities.Order> _orderRepository;

        private readonly ICouponService _couponService;

        public OrderService(IRepository<FOCS.Order.Infrastucture.Entities.Order> orderRepository, IRepository<Store> storeRepository, IRepository<MenuItem> menuRepository, IRepository<MenuItemVariant> variantRepository, ICouponService couponService)
        {
            _orderRepository = orderRepository;
            _storeRepository = storeRepository;
            _menuItemRepository = menuRepository;
            _variantRepository = variantRepository;
            _couponService = couponService;
        }

        public async Task<OrderResultDTO> CreateOrderAsGuestAsync(CreateOrderGuestDTO dto)
        {
            var store = await _storeRepository.FindAsync(x => x.Id == dto.StoreId);
            ConditionCheck.CheckCondition(store != null, Errors.OrderError.NotFoundStore);

            foreach(var item in dto.Items)
            {
                var currentItem = await _menuItemRepository.FindAsync(x => x.Id == item.MenuItemId);
                ConditionCheck.CheckCondition(currentItem.Any(), Errors.OrderError.MenuItemNotFound);

                var currentVariant = await _variantRepository.FindAsync(x => x.Id == item.VariantId);
                ConditionCheck.CheckCondition(currentVariant.Any(), Errors.OrderError.MenuItemNotFound);
            }

            await _couponService.IsValidApplyCouponAsync(dto.CouponCode);

            // Get Store Config

            return new OrderResultDTO
            {

            };
        }

        public Task<DiscountResultDTO> ApplyCouponAsync(Guid userId, string couponCode, Guid storeId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderResultDTO> CreateOrderAsync(Guid userId, CreateOrderDTO dto)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDetailDTO> GetOrderByCodeAsync(string orderCode)
        {
            throw new NotImplementedException();
        }

        public async Task<List<OrderDTO>> GetPendingOrdersAsync()
        {
            //var ordersPending = await _orderRepository.FindAsync(x => x.OrderStatus == Common.Enums.OrderStatus.Pending)
            return null;
        }

        public Task<OrderDetailDTO> GetUserOrderDetailAsync(Guid userId, Guid orderId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrderSummaryDTO>> GetUserOrdersAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task SubmitFeedbackAsGuestAsync(OrderFeedbackDTO dto)
        {
            throw new NotImplementedException();
        }

        public Task SubmitFeedbackAsync(Guid userId, OrderFeedbackDTO dto)
        {
            throw new NotImplementedException();
        }

        public Task<DiscountResultDTO> VerifyCouponAsGuestAsync(string couponCode, Guid storeId)
        {
            throw new NotImplementedException();
        }


        #region private methods
        private async Task<bool> IsValidApplyCoupon(string? couponCode)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
