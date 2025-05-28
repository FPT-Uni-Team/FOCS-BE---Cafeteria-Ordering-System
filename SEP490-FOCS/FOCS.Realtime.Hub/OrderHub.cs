using Microsoft.AspNetCore.SignalR;

namespace FOCS.Realtime.Hubs
{
    public class OrderHub : Hub
    {
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
