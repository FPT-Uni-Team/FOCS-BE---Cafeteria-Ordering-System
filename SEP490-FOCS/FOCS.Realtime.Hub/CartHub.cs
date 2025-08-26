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
            

            if (!string.IsNullOrEmpty(storeId) && !string.IsNullOrEmpty(tableId))
            {
                string group = $"dept=user&storeId={storeId}&tableId={tableId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, group);
            }

            await base.OnConnectedAsync();
        }
    }
}
