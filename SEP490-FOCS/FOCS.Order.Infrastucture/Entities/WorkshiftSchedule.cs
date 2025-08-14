using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class WorkshiftSchedule : IAuditable
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid WorkshiftId {  get; set; }
        public Workshift Workshift { get; set; }

        public ICollection<StaffWorkshiftRegistration>? StaffWorkshiftRegistrations { get; set; }
 
        public Guid StoreId {  get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
