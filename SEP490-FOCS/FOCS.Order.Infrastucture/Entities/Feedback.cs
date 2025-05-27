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
    public class Feedback
    {
        public Guid Id { get; set; }

        [Range(1,5)]
        public int Rating { get; set; }
        
        public string Comment { get; set; }


        public Guid? UserId { get; set; }
        public User User { get; set; }


        public Guid OrderId { get; set; }
        public Order Order { get; set; }
    }
}
