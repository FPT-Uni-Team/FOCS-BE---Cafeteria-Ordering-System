using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class CategoryMenuItemDTO
    {
        public Guid Id { get; set; }

        public Guid MenuItemId { get; set; }

        public Guid CategoryId { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
