using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services.Interface
{
    public interface IDiscountStrategyService
    {
        Task<DiscountResultDTO> ApplyDiscountAsync(ApplyDiscountOrderRequest order, string? couponCode = null);

    }
}
