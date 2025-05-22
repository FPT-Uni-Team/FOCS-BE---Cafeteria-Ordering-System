using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Exceptions
{
    public static class Errors
    {
        public static class SystemError
        {
            public const string UnhandledExceptionOccurred = "Unhandled exception occurred";
        }

        public static class Common
        {
            public const string NotFound = "Not found";
        }

        public static class AuthError
        {
            public const string WrongPassword = "Wrong password";
            public const string PasswordNotMatch = "Passwords do not match.";
            public const string InvalidRefreshToken = "Invalid refresh token";
        }
    }
}
