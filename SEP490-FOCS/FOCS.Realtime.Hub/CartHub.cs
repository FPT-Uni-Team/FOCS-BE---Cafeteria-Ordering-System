using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Realtime.Hubs
{
    public class CartHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var storeId = Context.GetHttpContext()?.Request.Query["storeId"];
            var tableId = Context.GetHttpContext()?.Request.Query["tableId"];
            var actorId = Context.GetHttpContext()?.Request.Query["actorId"];

            if (!string.IsNullOrEmpty(storeId) && !string.IsNullOrEmpty(tableId))
            {
                string group = $"storeId={storeId}&tableId={tableId}&actorId={actorId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
            }

            await base.OnConnectedAsync();
        }
    }
}
