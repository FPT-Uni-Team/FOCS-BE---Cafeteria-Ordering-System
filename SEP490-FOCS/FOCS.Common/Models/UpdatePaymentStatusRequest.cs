using FOCS.Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class UpdatePaymentStatusRequest
    {
        [JsonPropertyName("status")]
        public PaymentStatus PaymentStatus { get; set; }

    }
}
