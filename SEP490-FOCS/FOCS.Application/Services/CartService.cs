using AutoMapper;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models.CartModels;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Realtime.Hubs;
using MassTransit.Initializers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit.Cryptography;
using Net.payOS.Types;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly IRepository<MenuItemVariant> _menuItemVariantRepository;

        private readonly IRedisCacheService _redisCacheService;
        private readonly IHubContext<CartHub> _cartHubContext;

        private readonly IRealtimeService _realtimeService;

        private readonly IMapper _mapper;

        private readonly ILogger<CartService> _logger;

        private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(1);
        public CartService(IRepository<MenuItem> menuItemRepository, 
                           IRepository<MenuItemVariant> menuItemVariantRepository, 
                           IMapper mapper, 
                           ILogger<CartService> logger,
                           IRedisCacheService redisCacheService,
                           IHubContext<CartHub> orderHubContext,
                           IRealtimeService realtimeService)
        {
            _menuItemVariantRepository = menuItemVariantRepository;
            _mapper = mapper;
            _logger = logger;
            _menuItemRepository = menuItemRepository;
            _redisCacheService = redisCacheService;
            _cartHubContext = orderHubContext;
            _realtimeService = realtimeService;
        }

        public async Task AddOrUpdateItemAsync(Guid tableId, string actorId, CartItemRedisModel item, string storeId)
        {
            var itemAvailable = await _menuItemRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == item.MenuItemId);
            
            if(itemAvailable != null && (itemAvailable.IsDeleted == true || itemAvailable.IsActive == false))
            {
                return;
            }
        
            var key = GetCartKey(tableId, storeId);

            var cart = await _redisCacheService.GetAsync<List<CartItemRedisModel>>(key) ?? new List<CartItemRedisModel>();

            var existingItem = cart.FirstOrDefault(x =>
                x.MenuItemId == item.MenuItemId &&
                AreVariantsEqual(x.Variants, item.Variants)
            );

            if (existingItem != null)
            {
                existingItem.CreatedTime = DateTime.UtcNow;
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                item.CreatedTime = DateTime.UtcNow;
                item.Id = Guid.NewGuid();
                cart.Add(item);
            }

            await _redisCacheService.SetAsync(key, cart, _cacheExpiry);

            var group = SignalRGroups.CartUpdate(Guid.Parse(storeId), tableId);
            await _realtimeService.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(group, SignalRGroups.ActionHub.UpdateCart, cart);
        }

        private bool AreVariantsEqual(List<CartVariantRedisModel>? a, List<CartVariantRedisModel>? b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;

            var dictA = a.GroupBy(x => x.VariantId).ToDictionary(g => g.Key, g => g.Sum(v => v.Quantity));
            var dictB = b.GroupBy(x => x.VariantId).ToDictionary(g => g.Key, g => g.Sum(v => v.Quantity));

            return dictA.Count == dictB.Count && !dictA.Except(dictB).Any();
        }

        public async Task ClearCartAsync(Guid tableId, string storeId, string actorId)
        {
            var key = GetCartKey(tableId, storeId);

            await _redisCacheService.RemoveAsync(key);

            var group = SignalRGroups.CartUpdate(Guid.Parse(storeId), tableId);

            await _cartHubContext.Clients.Group(group).SendAsync(SignalRGroups.ActionHub.UpdateCart, new List<CartItemRedisModel>());
        }

        public async Task<List<CartItemRedisModel>> GetCartAsync(Guid tableId, string storeId, string actorId)
        {
            var key = GetCartKey(tableId, storeId);

            var cartItems = await _redisCacheService.GetAsync<List<CartItemRedisModel>>(key);

            return cartItems ?? new List<CartItemRedisModel> { };
        }

        public async Task<bool> RemoveItemAsync(Guid tableId, string actorId, string storeId, Guid menuItemId, List<CartVariantRedisModel> variants, int quantity)
        {
            var key = GetCartKey(tableId, storeId);
            var cartItems = await _redisCacheService.GetAsync<List<CartItemRedisModel>>(key);

            if (cartItems == null) return false;

            var item = cartItems.FirstOrDefault(x =>
                x.MenuItemId == menuItemId &&
                ((x.Variants == null && (variants == null || !variants.Any())) ||
                 (x.Variants != null && variants != null &&
                  x.Variants.OrderBy(v => v.VariantId).SequenceEqual(
                      variants.OrderBy(v => v.VariantId),
                      new CartVariantComparer()
                  ))
                )
            );

            if (item == null) return false;

            if (item.Quantity <= quantity)
            {
                cartItems.Remove(item);
            }
            else
            {
                item.Quantity -= quantity;
            }

            await _redisCacheService.SetAsync(key, cartItems, _cacheExpiry);

            var group = SignalRGroups.CartUpdate(Guid.Parse(storeId), tableId);
            await _realtimeService.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(
                group,
                SignalRGroups.ActionHub.UpdateCart,
                cartItems
            );

            return true;
        }

        public class CartVariantComparer : IEqualityComparer<CartVariantRedisModel>
        {
            public bool Equals(CartVariantRedisModel? x, CartVariantRedisModel? y)
            {
                if (x == null || y == null) return false;
                return x.VariantId == y.VariantId && x.Quantity == y.Quantity;
            }

            public int GetHashCode(CartVariantRedisModel obj)
            {
                return HashCode.Combine(obj.VariantId, obj.Quantity);
            }
        }

        public async Task<Guid> RemoveItemAsync(string id, Dictionary<string, List<CartItemRedisModel>> allCartItems)
        {
            if (!Guid.TryParse(id, out Guid parsedId))
                return Guid.Empty;

            foreach (var kvp in allCartItems)
            {
                var cartItems = kvp.Value;
                var itemToRemove = cartItems.FirstOrDefault(x => x.Id == parsedId);

                if (itemToRemove != null)
                {
                    cartItems.Remove(itemToRemove);
                    await _redisCacheService.SetAsync(kvp.Key, cartItems);
                    return parsedId;
                }
            }

            return Guid.Empty;
        }

        public async Task<List<ScanModelResponse>> ScanAndRemoveExpiryItem()
        {
            var delItems = await ScanItemsExpiryDate();
            var rs = new List<ScanModelResponse>();

            foreach (var kvp in delItems)
            {
                foreach (var itemProduct in kvp.Value)
                {
                    var removedId = await RemoveItemAsync(itemProduct.Id.ToString(), delItems);

                    if (removedId != Guid.Empty)
                    {
                        rs.Add(new ScanModelResponse
                        {
                            Key = kvp.Key,
                            RemovedId = removedId
                        });
                    }
                }
            }
            return rs;
        }


        private async Task<Dictionary<string, List<CartItemRedisModel>>> ScanItemsExpiryDate()
        {
            var rs = new Dictionary<string, List<CartItemRedisModel>>();

            var allItems = await _redisCacheService.GetAllAsync<List<CartItemRedisModel>>();

            foreach (var kvp in allItems)
            {
                var expiredItems = kvp.Value
                    .Where(item => DateTime.UtcNow - item.CreatedTime >= TimeSpan.FromHours(1))
                    .ToList();

                if (expiredItems.Any())
                {   
                    rs[kvp.Key] = expiredItems;
                }
            }

            return rs;
        }

        public string GetCartKey(Guid tableId, string storeId)
        {
            return $"cart:{storeId.ToLower()}:{tableId}";
        }

    }
}
