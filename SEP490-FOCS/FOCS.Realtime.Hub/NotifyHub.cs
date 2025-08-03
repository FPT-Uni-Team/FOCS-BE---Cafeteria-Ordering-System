using FOCS.NotificationService.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Realtime.Hubs
{
    public class NotifyHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var storeId = Context.GetHttpContext()?.Request.Query["storeId"];
            var tableId = Context.GetHttpContext()?.Request.Query["tableId"];
            var dept = Context.GetHttpContext()?.Request.Query["dept"];
            //var group = Context.GetHttpContext()?.Request.Query["group"];

            if (!string.IsNullOrEmpty(storeId))
            {
                string group = $"dept={dept}&storeId={storeId}&tableId={tableId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
            }
            await base.OnConnectedAsync();
        }
    }
}
