using FOCS.Common.Interfaces;
using FOCS.Common.Models;
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
        private readonly IRepository<FOCS.Order.Infrastucture.Entities.Order> _orderRepository;

        public OrderService(IRepository<FOCS.Order.Infrastucture.Entities.Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<OrderDTO>> GetPendingOrdersAsync()
        {
            //var ordersPending = await _orderRepository.FindAsync(x => x.OrderStatus == Common.Enums.OrderStatus.Pending)
            return null;
        }
    }
}
