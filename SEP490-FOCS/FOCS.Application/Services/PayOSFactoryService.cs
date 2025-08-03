using FOCS.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class PayOSServiceFactory : IPayOSServiceFactory
    {
        public IPayOSService Create(string clientId, string apiKey, string checksumKey)
        {
            return new PayOSService(clientId, apiKey, checksumKey);
        }
    }
}
