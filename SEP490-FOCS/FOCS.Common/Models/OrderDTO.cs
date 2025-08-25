using FOCS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class OrderDTO
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("order_code")]
        public string OrderCode { get; set; } = null!;

        [JsonIgnore]
        public Guid StoreId { get; set; }

        [JsonPropertyName("user_id")]
        public Guid? UserId { get; set; }

        [JsonPropertyName("order_status")]
        public OrderStatus OrderStatus { get; set; }

        [JsonPropertyName("order_type")]
        public OrderType OrderType { get; set; }

        [JsonPropertyName("payment_status")]
        public PaymentStatus PaymentStatus { get; set; }

        [JsonPropertyName("sub_total_amount")]
        public double SubTotalAmout { get; set; } // total before tax rate and promotion

        [JsonPropertyName("tax_amount")]
        public double TaxAmount { get; set; } // tax rate

        [JsonPropertyName("discount_amount")]
        public double DiscountAmount { get; set; } // discount follow promotion

        [JsonPropertyName("total_amount")]
        public double TotalAmount { get; set; } // total after apply tax and promotion

        [JsonPropertyName("customer_note")]
        public string CustomerNote { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("remaining_time")]
        public TimeSpan? RemainingTime { get; set; } = TimeSpan.Zero;

        [JsonPropertyName("created_by")]
        public string? CreatedBy { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("updated_by")]
        public string? UpdatedBy { get; set; }

        [JsonIgnore]
        public string CouponCode {  get; set; }


        [JsonPropertyName("order_details")]
        //Order details
        public List<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
    }
}
