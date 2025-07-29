using FOCS.Common.Interfaces.Focs.Application.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/feedback")]
    [ApiController]
    public class FeedbackController : FocsController
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListFeedback(UrlQueryParameters query, [FromHeader(Name = "storeId")] string storeId)
        {
            var rs = await _feedbackService.GetAllFeedbacksAsync(query, storeId);

            return Ok(rs);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromForm] CreateFeedbackRequest request, [FromHeader(Name = "storeId")] string storeId)
        {
            var rs = await _feedbackService.SubmitFeedbackAsync(request, storeId);
            return Ok(rs);
        }

        [HttpPost("order/{orderId}")]
        public async Task<IActionResult> GetOnOrder(Guid orderId, [FromHeader(Name = "storeId")] string storeId)
        {
            var rs = await _feedbackService.GetFeedbackByOrderIdAsync(orderId, storeId);
            return Ok(rs);
        }

        [HttpPost("menu-item/{menuItemId}")]
        public async Task<IActionResult> GetOnMenuItem(Guid menuItemId, [FromHeader(Name = "storeId")] string storeId)
        {
            var rs = await _feedbackService.GetFeedbackByMenuItemAsync(menuItemId, storeId);
            return Ok(rs);
        }


    }
}
