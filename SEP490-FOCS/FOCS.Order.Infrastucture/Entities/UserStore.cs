using FOCS.Common.Enums;
using FOCS.Infrastructure.Identity.Identity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class UserStore
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        
        public Guid StoreId { get; set; }
        public Store Store { get; set; }
        
        public DateTime JoinDate { get; set; } = DateTime.Now;

        public UserStoreStatus Status { get; set; } = UserStoreStatus.Active;
        public string? BlockReason { get; set; }
    }
}
