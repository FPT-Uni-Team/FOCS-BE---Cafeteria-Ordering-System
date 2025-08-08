using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class WorkshiftScheduleDto
    {
        public Guid Id { get; set; }
        public Guid StoreId { get; set; }
        public DateTime WorkDate { get; set; }
    }

    public class WorkshiftScheduleResponse
    {
        public Guid Id { get; set; }
        public Guid StoreId { get; set; }
        public DateTime WorkDate { get; set; }

        public List<CreateWorkShiftDto> WorkShifts { get; set; }
    }

    public class WorkShiftDto
    {
        public Guid ScheduleId { get; set; }
        public string Name { get; set; } = default!;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class CreateWorkShiftDto
    {
        public Guid? StaffId {  get; set; }
        public string Name { get; set; } = default!;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class WorkshiftResponse
    {
        public DateTime WorkDate { get; set; }

        public List<StaffWorkshiftResponse> Shift { get; set; }
    }

    public class StaffWorkshiftResponse
    {

        public Guid StaffId { get; set; }
        public string StaffName { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }

    public class StaffWorkshiftRegistrationDto
    {
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public DateTime RegisteredAt { get; set; }
        public WorkshiftStatus Status { get; set; } = default!;
    }

    public class StaffWorkshiftRegistrationResponse
    {
        public DateTime WorkDate { get; set; }

        public List<CreateWorkShiftDto> WorkShifts { get; set; }
    }

    public class BulkRegisterRequest
    {
        public Guid StaffId { get; set; }
        public Guid StoreId { get; set; }
        public string ShiftName { get; set; } = default!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

}
