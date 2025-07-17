using FOCS.Common.Interfaces;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class RealtimeService : IRealtimeService
    {
        private readonly IServiceProvider _serviceProvider;

        public RealtimeService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SendToAllAsync<THub, TData>(string method, TData data)
            where THub : Hub
        {
            var hubContext = _serviceProvider.GetRequiredService<IHubContext<THub>>();
            await hubContext.Clients.All.SendAsync(method, data);
        }

        public async Task SendToGroupAsync<THub, TData>(string group, string method, TData data)
            where THub : Hub
        {
            var hubContext = _serviceProvider.GetRequiredService<IHubContext<THub>>();
            await hubContext.Clients.Group(group).SendAsync(method, data);
        }
    }
}
