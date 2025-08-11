using FOCS.Common.Enums;
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class StaffWorkshiftRegistration : IAuditable
    {
        public Guid Id { get; set; }

        public Guid StaffId { get; set; }

        public string StaffName { get; set; }

        public Guid WorkshiftScheduleId { get; set; }
        public WorkshiftSchedule WorkshiftSchedule { get; set; }

        public Guid WorkshiftId {  get; set; }

        public Workshift Workshift { get; set; }

        public WorkshiftStatus Status { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
