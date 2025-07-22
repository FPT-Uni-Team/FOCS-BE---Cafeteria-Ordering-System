using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FOCS.Application.Services.BackgroundServices
{
    public class OrderBatchingService : BackgroundService
    {
        private readonly ILogger<OrderBatchingService> _logger;
        private readonly IServiceScopeFactory _scopedFactory;

        private readonly IOptionsMonitor<OrderBatchingOptions> _options;

        public OrderBatchingService(ILogger<OrderBatchingService> logger, IServiceScopeFactory serviceScopeFactory, IOptionsMonitor<OrderBatchingOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopedFactory = serviceScopeFactory;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderBatchingService is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopedFactory.CreateScope())
                {
                    try
                    {
                        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                        var kitchenService = scope.ServiceProvider.GetRequiredService<IKitchenService>();

                        _logger.LogInformation("Fetching pending order...");
                        var pendingOrders = await orderService.GetPendingOrdersInDayAsync();

                        if (pendingOrders.Any())
                        {
                            _logger.LogInformation($"Batching {pendingOrders.Count} orders to kitchen");
                            await kitchenService.SendOrdersToKitchenAsync(pendingOrders);
                        }
                        else
                        {
                            _logger.LogInformation("No pending orders found");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while batching orders.");
                    }
                }

                var delay = TimeSpan.FromMinutes(_options.CurrentValue.IntervalInMinutes);
                await Task.Delay(delay, stoppingToken);
            }

            _logger.LogInformation("OrderBatchingService is stopping.");
        }
    }
}
