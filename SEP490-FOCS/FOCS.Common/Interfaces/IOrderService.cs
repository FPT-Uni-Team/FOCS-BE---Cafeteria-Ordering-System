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
    }
}
