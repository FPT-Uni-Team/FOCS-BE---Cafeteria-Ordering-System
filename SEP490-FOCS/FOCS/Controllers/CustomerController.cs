using FOCS.Application.DTOs;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/admin/customer")]
    [ApiController]
    public class CustomerController : FocsController
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<PagedResult<UserProfileDTO>> GetCustomerList([FromBody] UrlQueryParameters query)
        {
            return await _customerService.GetCustomerListAsync(query, StoreId);
        }

        [HttpGet("{customerId}")]
        public async Task<UserProfileDTO> GetBrandDetail(string customerId)
        {
            return await _customerService.GetCustomerProfileAsync(StoreId, customerId, UserId);
        }

        [HttpPatch("{customerId}/block")]
        public async Task<bool> BlockCustomer(string customerId)
        {
            return await _customerService.BlockCustomerAsync(StoreId, customerId, UserId);
        }

        [HttpPatch("{customerId}/unblock")]
        public async Task<bool> UnblockCustomer(string customerId)
        {
            return await _customerService.UnblockCustomerAsync(StoreId, customerId, UserId);
        }

    }
}
