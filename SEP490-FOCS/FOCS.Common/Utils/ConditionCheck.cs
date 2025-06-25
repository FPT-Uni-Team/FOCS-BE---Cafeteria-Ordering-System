using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FOCS.Common.Exceptions;

namespace FOCS.Common.Utils
{
    public static class ConditionCheck
    {
        public static void CheckCondition(bool condition, Func<Exception> exceptionFatory)
        {
            if (!condition)
            {
                throw exceptionFatory();
            }
        }

        public static void CheckCondition(bool condition, string message, string? fieldName = null)
        {
            if (!condition)
            {
                throw new Exception(message + "@" + fieldName);
            }
        }
        public static void CheckCondition(bool condition, string message, int errorCode)
        {
            if (!condition)
            {
                throw new CustomException(message, errorCode);
            }
        }
    }
}
