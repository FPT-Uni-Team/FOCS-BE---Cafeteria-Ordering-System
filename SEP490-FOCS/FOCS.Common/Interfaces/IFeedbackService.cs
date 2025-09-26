using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Threading.Tasks;
    using FOCS.Common.Models;
    using Microsoft.AspNetCore.Mvc;

    namespace Focs.Application.Interfaces
    {
        public interface IFeedbackService
        {
            // CUSTOMER FUNCTIONALITIES

            /// <summary>
            /// Gửi feedback cho một đơn hàng đã hoàn tất.
            /// </summary>
            Task<bool> SubmitFeedbackAsync(CreateFeedbackRequest request, string storeId);

            /// <summary>
            /// Lấy feedback theo OrderId (để hiển thị lại hoặc check đã gửi chưa).
            /// </summary>
            Task<FeedbackDTO> GetFeedbackByOrderIdAsync(string orderCode, string storeId);


            Task<List<FeedbackDTO>> GetFeedbackByMenuItemAsync(Guid menuItem, string storeId);


            // ADMIN FUNCTIONALITIES

            /// <summary>
            /// Lấy danh sách feedback có phân trang và filter.
            /// </summary>
            Task<PagedResult<FeedbackDTO>> GetAllFeedbacksAsync(UrlQueryParameters parameters, string storeId);

            /// <summary>
            /// Lấy chi tiết một feedback cụ thể.
            /// </summary>
            Task<FeedbackDTO> GetFeedbackByIdAsync(Guid feedbackId, string storeId);

            /// <summary>
            /// Trả lời một feedback (nếu cần).
            /// </summary>
            Task ReplyToFeedbackAsync(Guid feedbackId, string reply, string storeId);

            /// <summary>
            /// Ẩn hoặc hiện một feedback trên giao diện công khai (IsPublic).
            /// </summary>
            Task<bool> SetFeedbackVisibilityAsync(Guid feedbackId, bool isPublic, string storeId);
            Task<FeedbackDTO> UpdatePublicCommentRequest(Guid id, UpdatePublicCommentRequest request, string storeId);
        }
    }

}
