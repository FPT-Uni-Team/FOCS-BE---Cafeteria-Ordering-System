using FOCS.Application.DTOs;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Constants;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface ICustomerService
    {
        Task<PagedResult<UserProfileDTO>> GetCustomerListAsync(UrlQueryParameters query, string storeId);
        Task<UserProfileDTO> GetCustomerProfileAsync(string storeId, string customerId, string managerId);
        Task<bool> BlockCustomerAsync(string storeId, string customerId, string managerId);
        Task<bool> UnblockCustomerAsync(string storeId, string customerId, string managerId);
    }
}
