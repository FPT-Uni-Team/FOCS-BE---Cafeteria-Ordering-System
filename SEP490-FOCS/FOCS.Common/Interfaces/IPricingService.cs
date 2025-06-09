using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IPricingService
    {
        Task<PricingDTO> GetPriceByProduct(Guid productId, Guid? variantId, Guid? storeId);
    }
}
