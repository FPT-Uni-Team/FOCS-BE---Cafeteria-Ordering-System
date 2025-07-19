using FirebaseAdmin;
using FOCS.NotificationService;
using FOCS.NotificationService.Consumers;
using Google.Apis.Auth.OAuth2;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("firebase-adminsdk.json")
});

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
