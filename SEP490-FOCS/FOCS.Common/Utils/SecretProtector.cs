using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Utils
{
    public class SecretProtector
    {
        private readonly IDataProtector _dataProtector;

        public SecretProtector(IDataProtectionProvider dataProtector)
        {
            _dataProtector = dataProtector.CreateProtector("PayOS.Protection");
        }

        public string Encrypt(string plain) => _dataProtector.Protect(plain);
        public string Decrypt(string cipher) => _dataProtector.Unprotect(cipher);
    }
}
