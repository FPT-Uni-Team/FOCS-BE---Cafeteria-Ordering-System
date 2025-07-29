using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class CreateFeedbackRequest
    {
        [FromForm(Name = "rating")]
        public int Rating { get; set; }

        [FromForm(Name = "comment")]
        public string? Comment { get; set; }

        [FromForm(Name = "images")]
        public List<IFormFile>? Files {  get; set; }

        [FromForm(Name = "order_id")]
        public Guid OrderId { get; set; }

        [FromForm(Name = "actor_id")]
        public Guid ActorId {  get; set; }
    }
}
