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
        public async Task SendToGroup(string groupName, object data)
        {
            await Clients.Group(groupName).SendAsync("ReceiveNotify", data);
        }

        public override async Task OnConnectedAsync()
        {
            var storeId = Context.GetHttpContext()?.Request.Query["storeId"]; 
            //var group = Context.GetHttpContext()?.Request.Query["group"];

            if (!string.IsNullOrEmpty(storeId))
            {
                string group = $"storeId={storeId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
            }
            await base.OnConnectedAsync();
        }
    }
}
