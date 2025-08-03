using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using FOCS.NotificationService.Constants;
using FOCS.NotificationService.Models;
using FOCS.NotificationService.Services;
using FOCS.Realtime.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
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
        private readonly FirebaseService _firebaseService;

        public NotifyConsumer(IHubContext<NotifyHub> notifyHub, ILogger<NotifyConsumer> notifyLogger, FirebaseService firebaseService)
        {
            _notifyHub = notifyHub;
            _notifyLogger = notifyLogger;
            _firebaseService = firebaseService;
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
            foreach (var token in payload.MobileTokens)
            {
                var message = new Message()
                {
                    Token = token,
                    Notification = new Notification
                    {
                        Title = payload.Title,
                        Body = payload.Message
                    }
                };

                try
                {
                    var result = await _firebaseService.Messaging.SendAsync(message);
                    _notifyLogger.LogInformation("✅ Push sent to device {Token}, result: {Result}", token, result);
                }
                catch (Exception ex)
                {
                    _notifyLogger.LogError(ex, "❌ Failed to send push to device {Token}", token);
                }
            }
        }
    }

}
