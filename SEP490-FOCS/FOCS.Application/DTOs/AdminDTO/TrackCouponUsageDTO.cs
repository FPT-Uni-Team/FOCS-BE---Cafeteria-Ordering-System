namespace FOCS.Application.DTOs.AdminDTO
{
    public class TrackCouponUsageDTO
    {
        public List<UsageInfo> Usages { get; set; }
        public int TotalLeft { get; set; }

        public class UsageInfo
        {
            public Guid OrderId { get; set; }
            public DateTime UsedAt { get; set; }
        }
    }
}
