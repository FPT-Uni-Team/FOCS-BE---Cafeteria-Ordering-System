using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.NotificationService.Models
{
    public class NotifyEvent
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string[] TargetGroups { get; set; } // e.g., "cashier", "kitchen"
        public string[] MobileTokens { get; set; } // for push to mobile

        public string storeId { get; set; }
        public string? tableId { get; set; }
    }
}
