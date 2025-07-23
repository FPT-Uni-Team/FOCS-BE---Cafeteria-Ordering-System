using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using FOCS.NotificationService;
using FOCS.NotificationService.Consumers;
using Google.Apis.Auth.OAuth2;
using MassTransit;
using System.Text;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

using var loggerFactory = LoggerFactory.Create(config =>
{
    config.AddConsole();
});
var logger = loggerFactory.CreateLogger("FirebaseInit");

try
{
    var path = Path.Combine(Directory.GetCurrentDirectory(), "firebase-service-account.json");

    if (!File.Exists(path))
    {
        logger.LogError("Firebase service account file not found at: {Path}", path);
    }
    else
    {
        logger.LogInformation("Firebase service account file found at: {Path}", path);

        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(path)
            });

            logger.LogInformation("Firebase has been successfully initialized.");
        }
        else
        {
            logger.LogWarning("FirebaseApp already initialized.");
        }
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to initialize Firebase.");
}

builder.Services.AddHostedService<Worker>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotifyConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("notify-queue", e =>
        {
            e.ConfigureConsumer<NotifyConsumer>(ctx);
        });
    });
});

var host = builder.Build();
host.Run();
