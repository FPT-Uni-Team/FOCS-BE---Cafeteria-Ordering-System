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

        [HttpPost]
        public async Task<StaffProfileDTO> CreateManagerAsync(RegisterRequest dto)
        {
            return await _staffService.CreateManagerAsync(dto, StoreId, UserId);
        }

        [HttpGet("{staffId}")]
        public async Task<StaffProfileDTO> GetManagerProfileAsync(string staffId)
        {
            return await _staffService.GetStaffProfileAsync(staffId, UserId);
        }

        [HttpDelete("{staffId}")]
        public async Task<IActionResult> DeleteStaffProfileAsync(string staffId)
        {
            var isDeleted = await _staffService.DeleteManagerAsync(staffId, UserId);
            if (!isDeleted)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
