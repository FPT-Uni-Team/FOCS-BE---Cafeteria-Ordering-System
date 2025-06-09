using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class DiscountResultDTO
    {
        public decimal TotalDiscount { get; set; }

        public string? AppliedCouponCode { get; set; }

        public List<string> AppliedPromotions { get; set; } = new();

        /// <summary>
        /// Danh sách các mục đã áp dụng giảm giá, để hiển thị chi tiết ở FE
        /// </summary>
        public List<DiscountItemDetail> ItemDiscountDetails { get; set; } = new();

        /// <summary>
        /// Các cảnh báo hoặc log trong quá trình tính toán
        /// </summary>
        public List<string> Messages { get; set; } = new();

        public bool IsDiscountApplied => TotalDiscount > 0;
    }
    public class DiscountItemDetail
    {
        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal DiscountAmount { get; set; }

        public string Source { get; set; } = string.Empty; // eg. "Coupon", "Promotion: HappyLunch"
    }
}
