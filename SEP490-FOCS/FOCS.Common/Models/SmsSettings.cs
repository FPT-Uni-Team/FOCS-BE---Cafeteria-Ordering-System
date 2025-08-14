using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class EsmsSettings
    {
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string BrandName { get; set; }
        public string IsUnicode { get; set; }
        public string SmsType { get; set; }
    }

}
