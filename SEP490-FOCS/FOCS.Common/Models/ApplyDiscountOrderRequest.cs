using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class ApplyDiscountOrderRequest
    {
        [JsonPropertyName("store_id")]
        public Guid StoreId { get; set; }

        [JsonPropertyName("table_id")]
        public Guid TableId { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItemDTO> Items { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("coupon_code")]
        public string? CouponCode { get; set; }

        [JsonPropertyName("point")]
        public int? Point { get; set; }

        [JsonPropertyName("is_use_point")]
        public bool? IsUseLoyatyPoint { get; set; }
    }

    public class CreateOrderRequest
    {
        [JsonPropertyName("store_id")]
        public Guid StoreId { get; set; }

        [JsonPropertyName("table_id")]
        public Guid TableId { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItemDTO> Items { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("coupon_code")]
        public string? CouponCode { get; set; }

        [JsonPropertyName("payment_type")]
        public PaymentType PaymentType { get; set; }

        [JsonPropertyName("order_type")]
        public OrderType OrderType { get; set; }

        [JsonPropertyName("customer_info")]
        public CustomerInfo customerInfo { get; set; }

        [JsonPropertyName("discount")]
        public DiscountResultDTO DiscountResult { get; set; }
    }
}
