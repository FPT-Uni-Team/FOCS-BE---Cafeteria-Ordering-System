using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models.CartModels;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FOCS.Application.Services.BackgroundServices
{
    public class CartFlushBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CartFlushBackgroundService> _logger;

        public CartFlushBackgroundService(IServiceProvider serviceProvider, ILogger<CartFlushBackgroundService> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await FlushRedisCarts(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }

        private async Task FlushRedisCarts(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var redisService = scope.ServiceProvider.GetRequiredService<IRedisCacheService>();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IRepository<Order.Infrastucture.Entities.Order>>();

            var keys = await redisService.GetKeysByPatternAsync("cart:*");

            foreach (var key in keys)
            {
                var cartItems = await redisService.GetAsync<List<CartItemRedisModel>>(key);

                if (cartItems == null || cartItems.Count == 0)
                    continue;

                var (storeId, tableId, actorId) = ParseKey(key);

                var actorGuid = Guid.TryParse(actorId, out var parsedActor) ? parsedActor : Guid.Empty;

                var existing = await orderRepository
                    .AsQueryable()
                    .Include(x => x.OrderDetails)
                    .FirstOrDefaultAsync(x =>
                        x.TableId == tableId &&
                        x.UserId == actorGuid &&
                        x.OrderStatus == OrderStatus.CartSaved,
                        cancellationToken);

                if (existing != null)
                {
                    existing.OrderDetails.Clear();
                    foreach (var item in cartItems)
                    {
                        existing.OrderDetails.Add(new OrderDetail
                        {
                            Id = Guid.NewGuid(),
                            MenuItemId = item.MenuItemId,
                            //VariantId = item.VariantId,
                            Quantity = item.Quantity,
                            Note = item.Note
                        });
                    }

                    existing.UpdatedAt = DateTime.UtcNow;
                    orderRepository.Update(existing);
                }
                else
                {
                    var newOrder = new Order.Infrastucture.Entities.Order
                    {
                        Id = Guid.NewGuid(),
                        StoreId = storeId,
                        TableId = tableId,
                        UserId = actorGuid,
                        OrderStatus = OrderStatus.CartSaved,
                        CreatedAt = DateTime.UtcNow,
                        OrderDetails = cartItems.SelectMany(ci =>
                            ci.VariantIds.Select(variantId => new OrderDetail
                            {
                                Id = Guid.NewGuid(),
                                MenuItemId = ci.MenuItemId,
                                VariantId = variantId,
                                Quantity = ci.Quantity,
                                Note = ci.Note
                            })
                        ).ToList()
                    };

                    await orderRepository.AddAsync(newOrder);
                }

                _logger.LogInformation("Flushed cart {key} into DB at {time}", key, DateTime.UtcNow);
            }
        }

        private (Guid storeId, Guid tableId, string actorId) ParseKey(string key)
        {
            // key format: cart:{storeId}:{tableId}:{actorId}
            var parts = key.Split(":");
            return (Guid.Parse(parts[1]), Guid.Parse(parts[2]), parts[3]);
        }
    }
}
