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
            public const string UserNotFound = "User Not found";
            public const string Empty = "Empty";
        }

        public static class Category
        {
            public const string CategoryIsExist = "Category is exist in system";
        }
        public static class StoreSetting
        {
            public const string DiscountStrategyNotConfig = "Discount Strategy is not config in system";
        }

        public static class AuthError
        {
            public const string WrongPassword = "Wrong password";
            public const string PasswordNotMatch = "Passwords do not match.";
            public const string PasswordReuse = "New password cannot be the same as old password.";
            public const string InvalidRefreshToken = "Invalid refresh token";
            public const string NotVerifyAccount = "Account is not verify";
        }

        public static class Pricing
        {
            public const string InvalidPrice = "Prduct price is not set";
        }

        public static class OrderError
        {
            public const string MenuItemNotFound = "Item Not Found";
            public const string CouponIsNotValid = "Coupon is not valid";
            public const string TableNotFound = "Table is not found in system";
        }

        public static class PromotionError
        {
            public const string CouponNotFound = "Coupon code not found";
            public const string CouponMaxUsed = "Coupon is max used";
            public const string InvalidPeriodDatetime = "Coupon/Promotion is out of date";
            public const string PromotionNotFound = "Promotion not found";
            public const string PromotionActive = "Promotion is already active";
            public const string PromotionInactive = "Promotion is already inactive";
            public const string PromotionTitleExist = "Promotion with this title already exists.";
            public const string PromotionOverLapping = "A promotion of this type already exists in the specified date range.";
        }
    }
}
