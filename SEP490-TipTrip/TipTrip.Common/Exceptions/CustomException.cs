using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Exceptions
{
    public class CustomException : Exception
    {
        public int ErrorCode { get; }

        public CustomException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
