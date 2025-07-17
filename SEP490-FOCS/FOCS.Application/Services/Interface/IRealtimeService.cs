using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IRealtimeService
    {
        Task SendToAllAsync<THub, TData>(string method, TData data)
            where THub : Hub;

        Task SendToGroupAsync<THub, TData>(string group, string method, TData data)
            where THub : Hub;
    }
}
