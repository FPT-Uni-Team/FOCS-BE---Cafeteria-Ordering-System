using FOCS.Application.Services.Interface;
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class MaxDiscountOnlyStrategy : IDiscountStrategyService
    {
        public Task<DiscountResultDTO> ApplyDiscountAsync(CreateOrderRequest order, string? couponCode = null)
        {
            throw new NotImplementedException();
        }
    }
}
