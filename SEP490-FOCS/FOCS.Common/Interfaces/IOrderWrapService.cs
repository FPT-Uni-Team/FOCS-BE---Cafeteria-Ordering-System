using FOCS.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IOrderWrapService
    {
        Task<bool> ChangeStatusProductionOrder(UpdateStatusProductionOrderRequest dto);
    }
}
