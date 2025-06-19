using FOCS.Application.DTOs;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Authorize(Roles = Roles.User + "," + Roles.Admin + "," + Roles.Manager)]
    [Route("staff")]
    [ApiController]
    public class StaffController : FocsController
    {
        private readonly IStaffService _staffService;

        public StaffController(IStaffService StaffService)
        {
            _staffService = StaffService;
        }

        [HttpPut("assign-role")]
        public async Task<bool> AddStaffRoleAsync(UpdateRoleRequest updateRoleRequest)
        {
            return await _staffService.AddStaffRoleAsync(updateRoleRequest.Role, updateRoleRequest.UserToUpdateId, UserId);
        }

        [HttpPut("remove-role")]
        public async Task<bool> RemoveStaffRoleAsync(UpdateRoleRequest updateRoleRequest)
        {
            return await _staffService.RemoveStaffRoleAsync(updateRoleRequest.Role, updateRoleRequest.UserToUpdateId, UserId);
        }
    }
}
