using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Manager + "," + Roles.User + "," + Roles.Staff + "," + Roles.KitchenStaff)]
    [Route("api/cashier")]
    [ApiController]
    public class CashierOrderController : FocsController
    {

        private readonly ICashierService _cashierService;

        public CashierOrderController(ICashierService cashierService)
        {
            _cashierService = cashierService;
        }

        [HttpPost("orders")]
        public async Task<PagedResult<OrderDTO>> GetOrders([FromBody] UrlQueryParameters urlQueryParameters)
        {
            return await _cashierService.GetOrders(urlQueryParameters, StoreId);
        }

        [HttpPut("{id}/update-payment")]
        public async Task<bool> UpdatePaymentStatus(Guid id, UpdatePaymentStatusRequest updatePaymentStatus)
        {
            return await _cashierService.UpdatePaymentStatus(id, updatePaymentStatus, StoreId);
        }
    }
}
