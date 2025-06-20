namespace FOCS.Common.Enums
{
    public enum CouponStatus
    {
        UnAvailable = 0, // is_active == false || CountUsed > MaxUsage
        Incomming = 1, // Now < StartDate
        On_going = 2, // (StartDate <= Now <= EndDate)
        Expired = 3, // EndDate < Now
    }
}
