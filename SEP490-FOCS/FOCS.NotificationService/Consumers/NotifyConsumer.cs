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
        private readonly ILogger<NotifyConsumer> _notifyLogger;

        public NotifyConsumer(IHubContext<NotifyHub> notifyHub, ILogger<NotifyConsumer> notifyLogger)
        {
            _notifyHub = notifyHub;
            _notifyLogger = notifyLogger;
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

            // Send notify to Firebase
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

                _notifyLogger.LogInformation(fcm is null ? "fcm is null" : $"fcm not null: {fcm.ToString()}");

                await fcm.SendMulticastAsync(message);
            }
        }
    }

}
