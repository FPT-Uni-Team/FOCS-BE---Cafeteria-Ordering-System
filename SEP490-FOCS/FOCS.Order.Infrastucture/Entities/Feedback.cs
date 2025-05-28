using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using MimeKit.Tnef;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class Feedback : IAuditable
    {
        public Guid Id { get; set; }

        [Range(1,5)]
        public int Rating { get; set; }
        
        public string Comment { get; set; }

        public Guid? UserId { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public Guid OrderId { get; set; }
        public Order Order { get; set; }
    }
}
