namespace FOCS.Common.Constants
{
    public class AdminCouponConstants
    {
        // Create service
        public const string UserIdEmpty = "User ID cannot be null or empty.";
        public const string CouponCodeAuto = "AutoGenerate";
        public const string CouponCodeManual = "Manual";
        public const string CheckCouponCodeType = "Invalid coupon type. Must be 'auto' or 'manual'.";
        public const string CheckCouponCodeForManual = "Coupon code must be provided for manual type.";
        public const string CheckCreateUniqueCode = "Coupon code already exists";
        public const string CheckCreateDate = "Start date must be before end date.";
        public const string GenerateUniqueCouponCode = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        //Get by list Id
        public const string GetCouponsByListIdNotFound = "Coupon not found or has been deleted";

        // Update controller
        public const string UpdateNotFound = "Update unsuccessfully";
        public const string UpdateOk = "Update successfully";
        // Update service
        public const string CheckUpdateUniqueCode = "Coupon code already exists";
        public const string CheckUpdateDate = "Start date must be before end date.";

        // Delete controller
        public const string DeleteNotFound = "Delete unsuccessfully";
        public const string DeleteOk = "Delete successfully";

        // Track Coupon Usage controller
        public const string TrackNotFound = "Coupon cannot be used anymore.";

        // Set Coupon Status controller
        public const string CouponStatusNotFound = "Coupon not found or has been deleted.";
        public const string CouponStatusOk = "Coupon has been {0} successfully.";

        // Assign Coupons To Promotion controller
        public const string CouponsToPromotionNotFound = "Update unsuccessfully";
        public const string CouponsToPromotionOk = "Update successfully";
        // Assign Coupons To Promotion service
        public const string CheckPromotion = "Promotion not found or deleted.";

        //Field Names
        public const string FieldStoreId = "storeId";
        public const string FieldUserId = "userId";
        public const string FieldCouponType= "coupon_type";
        public const string FieldCode= "code";
        public const string FieldDate= "end_date";
        public const string FieldPromotionId= "promotion_id";
        public const string FieldListCouponId= "accept_for_items";

    }
}
