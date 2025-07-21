using FirebaseAdmin.Messaging;
using FOCS.NotificationService.Constants;
using FOCS.NotificationService.Models;
using FOCS.Realtime.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.NotificationService.Consumers
{
    public class NotifyConsumer : IConsumer<NotifyEvent>
    {
        private readonly IHubContext<NotifyHub> _notifyHub;

        public NotifyConsumer(IHubContext<NotifyHub> notifyHub)
        {
            _notifyHub = notifyHub;
        }

        public async Task Consume(ConsumeContext<NotifyEvent> context)
        {
            var payload = context.Message;

            var storeId = payload.storeId;
            var tableId = payload.tableId;

            foreach (var group in payload.TargetGroups)
            {
                await _notifyHub.Clients.Group(group).SendAsync(ActionHub.NewNotification, payload);
            }

            // Gửi Mobile qua Firebase
            if (payload.MobileTokens != null && payload.MobileTokens.Any())
            {
                var fcm = FirebaseMessaging.DefaultInstance;
                var message = new MulticastMessage()
                {
                    Tokens = payload.MobileTokens.ToList(),
                    Notification = new Notification()
                    {
                        Title = payload.Title,
                        Body = payload.Message
                    }
                };
                await fcm.SendMulticastAsync(message);
            }
        }
    }

}
