using FOCS.Common.Enums;
using FOCS.Order.Infrastucture.Entities;

namespace FOCS.Common.Models
{
    public class StoreSettingDTO
    {
        public Guid? Id { get; set; }

        public TimeSpan? OpenTime { get; set; }

        public TimeSpan? CloseTime { get; set; }

        public string? Currency {  get; set; } = "VND";

        public PaymentConfig? PaymentConfig { get; set; }

        public string? logoUrl { get; set; } = "";

        public bool? IsSelfService { get; set; }

        public DiscountStrategy? discountStrategy { get; set; }
    }
}
