using FOCS.Application.DTOs;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Route("api/manager")]
    [ApiController]
    public class ManagerController : FocsController
    {
        private readonly IStaffService _staffService;

        public ManagerController(IStaffService StaffService)
        {
            _staffService = StaffService;
        }

        [HttpPost("list")]
        public async Task<PagedResult<StaffProfileDTO>> GetManagerListAsync(UrlQueryParameters query)
        {
            return await _staffService.GetManagerListAsync(query, StoreId);
        }

        [HttpPost("list/{brandId}")]
        public async Task<PagedResult<StaffProfileDTO>> GetManagerListByBrandAsync(UrlQueryParameters query, Guid brandId)
        {
            return await _staffService.GetManagerListByBrandAsync(query, brandId);
        }

        [HttpPost("{storeId}")]
        public async Task<StaffProfileDTO> CreateManagerAsync(RegisterRequest dto, string storeId)
        {
            return await _staffService.CreateManagerAsync(dto, storeId, UserId);
        }

        [HttpGet("{managerId}")]
        public async Task<StaffProfileDTO> GetManagerProfileAsync(string managerId)
        {
            return await _staffService.GetStaffProfileAsync(managerId, UserId);
        }

        [HttpPut("{managerId}")]
        public async Task<StaffProfileDTO> UpdateStaffProfileAsync(StaffProfileDTO dto, string managerId)
        {
            return await _staffService.UpdateManagerProfileAsync(dto, managerId, UserId);
        }

        [HttpDelete("{managerId}")]
        public async Task<IActionResult> DeleteStaffProfileAsync(string managerId)
        {
            var isDeleted = await _staffService.DeleteManagerAsync(managerId, UserId);
            if (!isDeleted)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
