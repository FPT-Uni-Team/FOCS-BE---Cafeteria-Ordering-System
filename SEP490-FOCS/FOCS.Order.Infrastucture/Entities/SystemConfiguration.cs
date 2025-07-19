using FOCS.Common.Models;
using MailKit.Net.Pop3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class SystemConfiguration : IAuditable
    {
        public Guid Id { get; set; }

        public double EarningRate { get; set; }

        public double SpendingRate { get; set; }

        public string Language { get; set; } = "vi";

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
