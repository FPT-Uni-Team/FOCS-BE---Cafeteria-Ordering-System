using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class UpdateConfigPaymentRequest
    {
        public string PayOSClientId { get; set; }
        public string PayOSApiKey { get; set; }
        public string PayOSChecksumKey { get; set; }
    }
}
