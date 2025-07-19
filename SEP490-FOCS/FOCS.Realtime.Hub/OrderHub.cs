using Microsoft.AspNetCore.SignalR;

namespace FOCS.Realtime.Hubs
{
    public class OrderHub : Hub
    {

        //// Called when a client connects
        //public override async Task OnConnectedAsync()
        //{
        //    // Optional: add to default group (e.g., cashier group)
        //    await Groups.AddToGroupAsync(Context.ConnectionId, "cashiers");
        //    await base.OnConnectedAsync();
        //}

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

        // Called when a client disconnects
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Optional: remove from group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "cashiers");
            await base.OnDisconnectedAsync(exception);
        }

        //Send notify to all client
        public async Task BroadcastOrderWrapUpdate(object orderWrap)
        {
            await Clients.All.SendAsync(Constants.Method.ReceiveOrderWrapUpdate, orderWrap);
        }

        // Follow group 
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
