using FOCS.Common.Enums;
using FOCS.Common.Models;
using MailKit.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Entities
{
    public class Order : IAuditable
    {
        public Guid Id { get; set; }

        public long OrderCode { get; set; }

        public Guid UserId { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public OrderType OrderType { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public double SubTotalAmout { get; set; } // total before tax rate and promotion
        public double TaxAmount { get; set; } // tax rate
        public double DiscountAmount { get; set; } // discount follow promotion
        public double TotalAmount { get; set; } // total after apply tax and promotion

        public bool IsDeleted { get; set; } = false;
        public string CustomerNote { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }

        public List<string>? SourceDiscount { get; set; } = new List<string>();

        public int? PointUsed { get; set; }

        public TimeSpan? RemainingTime {get ;set;} = TimeSpan.Zero;

        //Store
        public Guid StoreId {  get; set; }
        public Store Store { get; set; }

        //Coupon
        public Guid? CouponId {  get; set; }
        public Coupon? Coupon {  get; set; } 

        //Order wrap 
        public Guid? OrderWrapId { get; set; }
        public OrderWrap? OrderWrap { get; set; }

        //Table
        public Guid? TableId { get; set; }
        public Table? Table { get; set; }

        //Order details
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();


    }
}
