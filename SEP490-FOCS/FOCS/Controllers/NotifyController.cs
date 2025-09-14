using FOCS.Application.Services;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.NotificationService.Models;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Realtime.Hubs;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace FOCS.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ITableService _tableService;
        private readonly IStaffService _staffService;
        private readonly IMobileTokenSevice _mobileTokenService;
        private readonly IWorkshiftScheduleService _workshiftScheduleService;

        private readonly IRepository<WorkshiftSchedule> _workshiftSchedule;

        private readonly INotifyService _notificationService;
        public NotifyController(IRepository<WorkshiftSchedule> workshiftSchedule, INotifyService notificationService, IPublishEndpoint publishEndpoint, IWorkshiftScheduleService workshiftScheduleService, IStaffService staffService, ITableService tableService, IMobileTokenSevice mobileTokenService)
        {
            _publishEndpoint = publishEndpoint;
            _mobileTokenService = mobileTokenService;
            _staffService = staffService;
            _tableService = tableService;
            _workshiftScheduleService = workshiftScheduleService;
            _workshiftSchedule = workshiftSchedule;
            _notificationService = notificationService;

        }

        [HttpPost("staff")]
        public async Task NotifyStaff([FromHeader(Name = "storeId")] string storeId, [FromHeader(Name = "actorId")] string actorId, [FromHeader] string tableId)
        {
            //var tokenDeviceMobile = await _mobileTokenService.GetMobileToken(Guid.Parse(actorId));

            var table = await _tableService.GetTableByIdAsync(Guid.Parse(tableId), storeId);

            //var notifyEventModel = new NotifyEvent
            //{
            //    Title = Constants.ActionTitle.ReceiveNotify(table.TableNumber.ToString()),
            //    Message = Constants.ActionTitle.ReceiveNotify(table.TableNumber.ToString()),
            //    TargetGroups = new[] { SignalRGroups.Staff(Guid.Parse(storeId)) },
            //    MobileTokens = new[] { tokenDeviceMobile.Token },
            //    storeId = storeId,
            //    tableId = null
            //};

            var now = DateTime.Now.TimeOfDay;

            var a = await _workshiftSchedule.AsQueryable()
                .Include(x => x.StaffWorkshiftRegistrations)
                .Include(z => z.Workshift).ToListAsync();

            var staffIds = await _workshiftSchedule.AsQueryable()
                .Include(x => x.StaffWorkshiftRegistrations)
                .Include(z => z.Workshift)
                .Where(z => z.Workshift.WorkDate.Date == DateTime.Now.Date)
                .Where(x => x.StartTime < now && x.EndTime > now)
                .SelectMany(x => x.StaffWorkshiftRegistrations.Select(x => x.StaffId))
                .ToListAsync();

            if (staffIds != null)
            {
                foreach (var staffId in staffIds)
                {
                    var currentDeviceToken = await _mobileTokenService.GetMobileToken((Guid)staffId);

                    if(currentDeviceToken == null) { continue; }

                    var notifyEventModelStaff = staffIds.Select(x => new NotifyEvent
                    {
                        Title = Constants.ActionTitle.ReceiveNotify(table.TableNumber.ToString()),
                        Message = Constants.ActionTitle.ReceiveNotify(table.TableNumber.ToString()),
                        TargetGroups = new[] { SignalRGroups.Staff(Guid.Parse(storeId)) },
                        MobileTokens = new[] { currentDeviceToken.Token },
                        storeId = storeId,
                        tableId = null
                    });

                    await _publishEndpoint.Publish(notifyEventModelStaff);
                    //var test = "7B098C79-819C-47C4-96BE-D1630F667FFA";
                    await _notificationService.AddNotifyAsync(staffId.ToString(), Constants.ActionTitle.ReceiveNotify(table.TableNumber.ToString()).ToString());
                }
            }

            //var test1 = "7B098C79-819C-47C4-96BE-D1630F667FFA";
            //await _notificationService.AddNotifyAsync(test1, Constants.ActionTitle.ReceiveNotify(table.TableNumber.ToString()).ToString());
            //await _publishEndpoint.Publish(notifyEventModel);
        }

        [HttpPost("{actorId}")]
        public async Task<IActionResult> Add(string actorId, [FromBody] string message)
        {
            await _notificationService.AddNotifyAsync(actorId, message);
            return Ok(new { success = true });
        }

        [HttpGet("{actorId}")]
        public async Task<IActionResult> Get(string actorId)
        {
            var notifies = await _notificationService.GetNotifiesAsync(actorId);
            return Ok(notifies);
        }

        [HttpDelete("{actorId}")]
        public async Task<IActionResult> Clear(string actorId)
        {
            await _notificationService.ClearNotifiesAsync(actorId);
            return Ok(new { success = true });
        }

    }
}
