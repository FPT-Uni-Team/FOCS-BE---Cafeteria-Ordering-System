using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class CreateImageRequest
    {
        [JsonPropertyName("image_file")]
        [FromForm]
        public IFormFile ImageFile { get; set; }

        [JsonPropertyName("is_main")]
        [FromForm]
        public bool IsMain { get; set; }
    }
}
