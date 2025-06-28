using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class UserStoreDTO
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid StoreId { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        public UserStoreStatus Status { get; set; } = UserStoreStatus.Active;
        
        public string? BlockReason { get; set; }
    }
}
