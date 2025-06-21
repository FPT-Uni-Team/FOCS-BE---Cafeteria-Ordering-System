namespace FOCS.Common.Enums
{
    public enum CouponByPromotionStatus
    {
        UnAvailable = 0, // is_active == false || is_deleted == true
        InPromotionDuration = 1, // (PromotionStartDate <= CouponStartDate) && (CouponEndDate <= PromotionEndDate)
    }
}
