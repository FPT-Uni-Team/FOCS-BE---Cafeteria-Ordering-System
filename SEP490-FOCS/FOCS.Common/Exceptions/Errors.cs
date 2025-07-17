using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Exceptions
{
    public static class Errors
    {
        public static class FieldName
        {
            public const string Id = "id";
            public const string Title = "title";
            public const string Email = "email";
            public const string StoreId = "store_id";
            public const string UserId = "user_id";
            public const string MenuItemId = "menu_item_id";
            public const string CategoryId = "category_id";
            public const string VariantGroupId = "variantgroup_id";
            public const string VariantId = "variant_id";
            public const string CouponId = "coupon_id";
            public const string CouponCode = "coupon_code";
            public const string CouponMaxUsed = "coupon_max_used";
            public const string CouponIds = "coupon_ids";
            public const string IsActive = "is_active";
            public const string IsAvailable = "is_available";
            public const string StartDate = "start_date";
            public const string EndDate = "end_date";
            public const string Description = "description";
            public const string DiscountValue = "discount_value";
            public const string MaxDiscountValue = "max_discount_value";
            public const string PromotionItemCondition = "promotion_item_condition";
            public const string NewPassword = "new_password";
        }

        public static class SystemError
        {
            public const string UnhandledExceptionOccurred = "Unhandled exception occurred";
            public const string ErrorWhenUpload = "Error when upload images";
        }

        public static class Common
        {
            public const string NotFound = "Not found";
            public const string StoreNotFound = "Store Not found";
            public const string UserNotFound = "User Not found";
            public const string Empty = "Empty";
            public const string IsExist = "Exist in system";
            public const string InvalidGuidFormat = "Invalid Guid format";
        }

        public static class Variant
        {
            public const string Exist = "Group name already exists for this menu item.";
            public const string FailWhenAssign = "Fail when assign";
        }

        public static class Category
        {
            public const string CategoryIsExist = "Category is exist in system";
        }

        public static class StoreSetting
        {
            public const string StoreSettingNotFound = "No store settings found for this store";
            public const string SettingExist = "Store have setting already";
            public const string DiscountStrategyNotConfig = "Discount Strategy is not config in system";
        }

        public static class AuthError
        {
            public const string WrongPassword = "Wrong password";
            public const string PasswordNotMatch = "Passwords do not match.";
            public const string PasswordReuse = "New password cannot be the same as old password.";
            public const string InvalidRefreshToken = "Invalid refresh token";
            public const string NotVerifyAccount = "Account is not verify";
            public const string UserUnauthor = "You don't have permission to access";
        }

        public static class StaffError
        {
            public const string InvalidRole = "You can't update invalid role";
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
            public const string CouponAssigned = "Coupon has assigned to another promotion";
            public const string InvalidPeriodDatetime = "Coupon/Promotion is out of date";
            public const string PromotionNotFound = "Promotion not found";
            public const string PromotionActive = "Promotion is active now";
            public const string PromotionInactive = "Promotion is inactive now";
            public const string PromotionTitleExist = "Promotion with this title exists";
            public const string PromotionOverLapping = "A promotion of this type exists in the specified date range.";
            public const string PromotionInvalidDateToActive = "Invalid date to active promotion";
            public const string CannotChangeScopeWhilePromotionIsOngoing = "Cannot change scope while promotion is ongoing";
            public const string CannotChangeTypeWhilePromotionIsOngoing = "Cannot change type while promotion is ongoing";
            public const string CannotChangeDiscountValueWhilePromotionIsOngoing = "Cannot change discount value while promotion is ongoing";
            public const string StartDateInPast = "Start Date cannot be in the past";
            public const string StartDateAfterEndDate = "Start Date must be before End Date";
            public const string MaxPercentageDiscountValue = "Discount Value cannot exceed 100% for Percentage discount type";
            public const string RequireItemCondition = "Condition is required for Buy X Get Y promotion type";
        }

        public static class MenuItemError
        {
            public const string MenuItemActive = "Item is active now";
            public const string MenuItemInactive = "Item is inactive now";
            public const string MenuItemAvailable = "Item is available now";
            public const string MenuItemUnavailable = "Item is unavailable now";
        }
    }
}
