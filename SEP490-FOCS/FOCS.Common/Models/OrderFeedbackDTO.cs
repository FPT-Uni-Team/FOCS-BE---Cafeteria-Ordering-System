using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class OrderFeedbackDTO
    {
        public Guid OrderId { get; set; }
        public int Rating { get; set; } 
        public string? Comment { get; set; }
    }
}
