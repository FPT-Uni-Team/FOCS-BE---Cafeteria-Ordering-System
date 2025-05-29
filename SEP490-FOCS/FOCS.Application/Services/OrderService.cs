using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class OrderService : IOrderService
    {
        public Task<List<OrderDTO>> GetPendingOrdersAsync()
        {
            throw new NotImplementedException();
        }
    }
}
