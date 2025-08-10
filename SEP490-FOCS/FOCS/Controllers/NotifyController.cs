using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.NotificationService.Models;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Realtime.Hubs;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMobileTokenSevice _mobileTokenService;

        public NotifyController(IPublishEndpoint publishEndpoint, IMobileTokenSevice mobileTokenService)
        {
            _publishEndpoint = publishEndpoint;
            _mobileTokenService = mobileTokenService;

        }

        [HttpPost("staff")]
        public async Task NotifyStaff([FromHeader(Name = "storeId")] string storeId, [FromHeader(Name = "actorId")] string actorId)
        {
            var tokenDeviceMobile = await _mobileTokenService.GetMobileToken(Guid.Parse(actorId));

            var notifyEventModel = new NotifyEvent
            {
                Title = Constants.ActionTitle.PushStaff,
                Message = Constants.ActionTitle.PushStaff,
                TargetGroups = new[] { SignalRGroups.Staff(Guid.Parse(storeId)) },
                MobileTokens = new[] { tokenDeviceMobile.Token },
                storeId = storeId,
                tableId = null
            };

            await _publishEndpoint.Publish(notifyEventModel);
        }
    }
}
