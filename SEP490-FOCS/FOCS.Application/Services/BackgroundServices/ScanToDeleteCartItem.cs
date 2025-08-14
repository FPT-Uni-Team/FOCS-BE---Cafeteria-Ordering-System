using FOCS.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services.BackgroundServices
{
    public class ScanToDeleteCartItem : BackgroundService
    {
        private readonly ILogger<ScanToDeleteCartItem> _loggerJobScanDel;

        private readonly IServiceScopeFactory _scopedFactory;
        public ScanToDeleteCartItem(ILogger<ScanToDeleteCartItem> loggerJobScanDel, IServiceScopeFactory scopedFactory)
        {
            _loggerJobScanDel = loggerJobScanDel;
            _scopedFactory = scopedFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _loggerJobScanDel.LogInformation("Start scan cart item");

            while(!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopedFactory.CreateScope())
                {
                    try
                    {
                        var cartService = scope.ServiceProvider.GetService<ICartService>();

                        var removeItems = await cartService.ScanAndRemoveExpiryItem();

                        if(removeItems != null && removeItems.Count > 0)
                        {
                            _loggerJobScanDel.LogInformation("Removed item {item}", removeItems);
                            _loggerJobScanDel.LogInformation("Removed item ids: {Ids}", string.Join(", ", removeItems.Select(x => x.RemovedId)));
                        } else
                        {
                            _loggerJobScanDel.LogInformation("not found any item to remove");
                        }
                    }catch(Exception ex)
                    {
                        _loggerJobScanDel.LogError("Error when scan: {msg}", ex.Message);
                    }
                }
                var delay = TimeSpan.FromMinutes(2);
                await Task.Delay(delay, stoppingToken);
            }

            _loggerJobScanDel.LogInformation("scan stopped");
        }
    }
}
