using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class Workshift : IAuditable
    {
        public Guid Id { get; set; }

        public DateTime WorkDate { get; set; }

        public Guid StoreId { get; set; }

        public ICollection<WorkshiftSchedule> WorkshiftSchedules { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
