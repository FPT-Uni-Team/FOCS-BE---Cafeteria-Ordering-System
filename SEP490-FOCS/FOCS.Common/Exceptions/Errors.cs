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
            public const string StoreNotFound = "Store Not found";
        }

        public static class AuthError
        {
            public const string WrongPassword = "Wrong password";
            public const string PasswordNotMatch = "Passwords do not match.";
            public const string InvalidRefreshToken = "Invalid refresh token";
            public const string NotVerifyAccount = "Account is not verify";
        }

        public static class OrderError
        {
            public const string NotFoundStore = "Store not found";
            public const string MenuItemNotFound = "Item Not Found";
            public const string CouponIsNotValid = "Coupon is not valid";
        }
    }
}
