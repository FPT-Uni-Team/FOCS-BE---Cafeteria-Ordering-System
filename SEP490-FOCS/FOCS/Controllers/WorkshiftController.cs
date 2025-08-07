using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FOCS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkshiftController : FocsController
    {
        private readonly IWorkshiftScheduleService _workshiftScheduleService;

        public WorkshiftController(IWorkshiftScheduleService workshiftScheduleService)
        {
            _workshiftScheduleService = workshiftScheduleService;
        }

        [HttpPost("list")]
        public async Task<PagedResult<WorkshiftResponse>> ListWorkshiftStaff([FromBody] UrlQueryParameters urlQueryParameters, [FromHeader(Name = "storeId")] string storeId)
        {
            return await _workshiftScheduleService.ListAll(urlQueryParameters, storeId);
        }

        // ========== Workshift Schedule ==========

        [HttpPost("schedule")]
        public async Task<ActionResult<WorkshiftScheduleDto>> CreateScheduleAsync([FromQuery] DateTime workDate)
        {
            var result = await _workshiftScheduleService.CreateScheduleAsync(Guid.Parse(StoreId), workDate);
            return Ok(result);
        }

        [HttpGet("schedule/{scheduleId}")]
        public async Task<ActionResult<WorkshiftScheduleDto?>> GetScheduleByIdAsync(Guid scheduleId)
        {
            var result = await _workshiftScheduleService.GetScheduleByIdAsync(scheduleId);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("schedules")]
        public async Task<ActionResult<List<WorkshiftScheduleResponse>>> GetSchedulesByStoreAsync([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = await _workshiftScheduleService.GetSchedulesByStoreAsync(Guid.Parse(StoreId), fromDate, toDate);
            return Ok(result);
        }

        [HttpDelete("schedule/{scheduleId}")]
        public async Task<ActionResult> DeleteScheduleAsync(Guid scheduleId)
        {
            var success = await _workshiftScheduleService.DeleteScheduleAsync(scheduleId);
            return success ? NoContent() : NotFound();
        }

        // ========== Workshift (Shift) ==========

        [HttpPost("schedule/{scheduleId}/shift")]
        public async Task<ActionResult> AddWorkShiftToScheduleAsync(Guid scheduleId, [FromBody] CreateWorkShiftDto workShiftDto)
        {
            var success = await _workshiftScheduleService.AddWorkShiftToScheduleAsync(scheduleId, workShiftDto, StoreId);
            return success ? Ok() : BadRequest("Failed to add shift");
        }

        [HttpGet("schedule/{scheduleId}/shifts")]
        public async Task<ActionResult<List<WorkShiftDto>>> GetWorkShiftsByScheduleAsync(Guid scheduleId)
        {
            var result = await _workshiftScheduleService.GetWorkShiftsByScheduleAsync(scheduleId);
            return Ok(result);
        }

        [HttpDelete("shift/{workShiftId}")]
        public async Task<ActionResult> DeleteWorkShiftAsync(Guid workShiftId)
        {
            var success = await _workshiftScheduleService.DeleteWorkShiftAsync(workShiftId);
            return success ? NoContent() : NotFound();
        }

        [HttpGet("shift/{workShiftId}")]
        public async Task<ActionResult<WorkShiftDto?>> GetWorkShiftByIdAsync(Guid workShiftId)
        {
            var result = await _workshiftScheduleService.GetWorkShiftByIdAsync(workShiftId);
            return result == null ? NotFound() : Ok(result);
        }

        // ========== Staff Registration ==========

        [HttpPost("shift/register/bulk")]
        public async Task<ActionResult> BulkRegisterStaffToShiftsAsync([FromBody] BulkRegisterRequest request)
        {
            await _workshiftScheduleService.BulkRegisterStaffToShiftsAsync(request);
            return Ok();
        }

        [HttpPost("shift/{workShiftId}/register")]
        public async Task<ActionResult<StaffWorkshiftRegistrationDto>> RegisterStaffToWorkShiftAsync(Guid workShiftId, [FromQuery] Guid staffId)
        {
            var result = await _workshiftScheduleService.RegisterStaffToWorkShiftAsync(staffId, workShiftId);
            return Ok(result);
        }

        [HttpGet("shift/{workShiftId}/registrations")]
        public async Task<ActionResult<List<StaffWorkshiftRegistrationDto>>> GetRegistrationsByWorkShiftAsync(Guid workShiftId)
        {
            var result = await _workshiftScheduleService.GetRegistrationsByWorkShiftAsync(workShiftId);
            return Ok(result);
        }

        [HttpGet("staff/{staffId}/registrations")]
        public async Task<ActionResult<List<StaffWorkshiftRegistrationResponse>>> GetRegistrationsByStaffAsync(Guid staffId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = await _workshiftScheduleService.GetRegistrationsByStaffAsync(staffId, fromDate, toDate);
            return Ok(result);
        }

        [HttpDelete("registration/{registrationId}")]
        public async Task<ActionResult> CancelRegistrationAsync(Guid registrationId)
        {
            var success = await _workshiftScheduleService.CancelRegistrationAsync(registrationId);
            return success ? NoContent() : NotFound();
        }

        [HttpPost("registration/{registrationId}/approve")]
        public async Task<ActionResult> ApproveRegistrationAsync(Guid registrationId)
        {
            var success = await _workshiftScheduleService.ApproveRegistrationAsync(registrationId);
            return success ? Ok() : NotFound();
        }
    }
}
