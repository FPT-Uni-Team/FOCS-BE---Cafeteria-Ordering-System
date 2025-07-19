using FirebaseAdmin.Messaging;
using FOCS.NotificationService.Models;
using MassTransit;
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
        public async Task Consume(ConsumeContext<NotifyEvent> context)
        {
            var msg = context.Message;

            var storeId = msg.storeId;

            // Gửi Web/POS qua SignalR
            var conn = new HubConnectionBuilder()
                .WithUrl($"https://focs.site/hubs/notify?storeId={storeId}")
                .Build();
            await conn.StartAsync();

            foreach (var group in msg.TargetGroups)
            {
                await conn.InvokeAsync("NewNotify", group, new { msg.Title, msg.Message });
            }

            await conn.StopAsync();

            // Gửi Mobile qua Firebase
            if (msg.MobileTokens != null && msg.MobileTokens.Any())
            {
                var fcm = FirebaseMessaging.DefaultInstance;
                var message = new MulticastMessage()
                {
                    Tokens = msg.MobileTokens.ToList(),
                    Notification = new Notification()
                    {
                        Title = msg.Title,
                        Body = msg.Message
                    }
                };
                await fcm.SendMulticastAsync(message);
            }
        }
    }

}
