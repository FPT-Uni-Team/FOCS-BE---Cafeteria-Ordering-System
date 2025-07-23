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
            if (payload.MobileTokens != null && payload.MobileTokens.Any())
            {
               if (_firebaseService.Messaging == null)
                {
                    _notifyLogger.LogError("❌ FirebaseMessaging is null in NotifyConsumer");
                    return;
                }
                
                _notifyLogger.LogInformation("✅ FirebaseMessaging instance acquired.");
                
                var message = new MulticastMessage()
                {
                    Tokens = payload.MobileTokens.ToList(),
                    Notification = new Notification
                    {
                        Title = payload.Title,
                        Body = payload.Message
                    }
                };
                
                var result = await _firebaseService.Messaging.SendMulticastAsync(message);
                _notifyLogger.LogInformation("✅ Push sent to {Count} devices, success: {Success}", payload.MobileTokens, result.SuccessCount);
            }
        }
    }

}
