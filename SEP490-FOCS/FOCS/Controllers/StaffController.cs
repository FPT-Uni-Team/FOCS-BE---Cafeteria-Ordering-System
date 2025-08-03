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
    [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
    [Route("api/staff")]
    [ApiController]
    public class StaffController : FocsController
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService StaffService)
        {
            _staffService = StaffService;
        }

        [HttpPost("list")]
        public async Task<PagedResult<StaffProfileDTO>> GetStaffListAsync(UrlQueryParameters query, [FromHeader] string storeId)
        {
            return await _staffService.GetStaffListAsync(query, storeId);
        }

        [HttpPost]
        public async Task<StaffProfileDTO> CreateStaffAsync(RegisterRequest dto)
        {
            return await _staffService.CreateStaffAsync(dto, StoreId, UserId);
        }

        [HttpGet("{staffId}")]
        public async Task<StaffProfileDTO> GetStaffProfileAsync(string staffId)
        {
            return await _staffService.GetStaffProfileAsync(staffId, UserId);
        }

        [HttpPut("{staffId}")]
        public async Task<StaffProfileDTO> UpdateStaffProfileAsync(StaffProfileDTO dto, string staffId)
        {
            return await _staffService.UpdateStaffProfileAsync(dto, staffId, UserId);
        }

        [HttpPut("{staffId}/change-password")]
        public async Task<bool> ChangeStaffPasswordAsync(ChangeStaffPasswordRequest request)
        {
            return await _staffService.ChangeStaffPasswordAsync(request, UserId);
        }

        [HttpDelete("{staffId}")]
        public async Task<IActionResult> DeleteStaffProfileAsync(string staffId)
        {
            var isDeleted = await _staffService.DeleteStaffAsync(staffId, UserId);
            if (!isDeleted)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpPut("assign-role")]
        public async Task<bool> AddStaffRoleAsync(UpdateRoleRequest updateRoleRequest)
        {
            return await _staffService.AddStaffRoleAsync(updateRoleRequest.Role, updateRoleRequest.StaffId, UserId);
        }

        [HttpPut("remove-role")]
        public async Task<bool> RemoveStaffRoleAsync(UpdateRoleRequest updateRoleRequest)
        {
            return await _staffService.RemoveStaffRoleAsync(updateRoleRequest.Role, updateRoleRequest.StaffId, UserId);
        }
    }
}
