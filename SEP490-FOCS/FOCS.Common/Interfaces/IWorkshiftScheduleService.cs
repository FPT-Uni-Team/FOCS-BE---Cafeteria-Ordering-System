using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IWorkshiftScheduleService
    {
        // ========== Workshift Schedule ==========
        Task<WorkshiftScheduleDto> CreateScheduleAsync(Guid storeId, DateTime workDate);
        Task<WorkshiftScheduleDto?> GetScheduleByIdAsync(Guid scheduleId);
        Task<List<WorkshiftScheduleResponse>> GetSchedulesByStoreAsync(Guid storeId, DateTime? fromDate, DateTime? toDate);
        Task<bool> DeleteScheduleAsync(Guid scheduleId);

        // ========== Workshift (Shift) ==========
        Task<bool> AddWorkShiftToScheduleAsync(Guid scheduleId, CreateWorkShiftDto workShiftDto, string storeId);
        Task<List<WorkShiftDto>> GetWorkShiftsByScheduleAsync(Guid scheduleId);
        Task<bool> DeleteWorkShiftAsync(Guid workShiftId);
        Task<WorkShiftDto?> GetWorkShiftByIdAsync(Guid workShiftId);


        // ========== Staff Registration ==========
        Task BulkRegisterStaffToShiftsAsync(BulkRegisterRequest request);
        Task<StaffWorkshiftRegistrationDto> RegisterStaffToWorkShiftAsync(Guid staffId, Guid workShiftId);
        Task<List<StaffWorkshiftRegistrationDto>> GetRegistrationsByWorkShiftAsync(Guid workShiftId);
        Task<List<StaffWorkshiftRegistrationResponse>> GetRegistrationsByStaffAsync(Guid staffId, DateTime? fromDate, DateTime? toDate);
        Task<bool> CancelRegistrationAsync(Guid registrationId);
        Task<bool> ApproveRegistrationAsync(Guid registrationId);
        Task<PagedResult<WorkshiftResponse>> ListAll(UrlQueryParameters urlQueryParameters, string storeId);
    }
}
