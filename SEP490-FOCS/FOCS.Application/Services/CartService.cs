using AutoMapper;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models.CartModels;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var key = GetCartKey(tableId, storeId, actorId);

            var cart = await _redisCacheService.GetAsync<List<CartItemRedisModel>>(key) ?? new List<CartItemRedisModel> { };

            var existingItem = cart.FirstOrDefault(x => x.MenuItemId == item.MenuItemId && x.VariantId == item.VariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
                if (!string.IsNullOrEmpty(item.Note))
                {
                    existingItem.Note = item.Note;
                }
            } 
            else
            {
                cart.Add(item);
            }

            await _redisCacheService.SetAsync(key, cart, _cacheExpiry);

            //Send data realtime to client
            var group = SignalRGroups.User(Guid.Parse(storeId), tableId, Guid.Parse(actorId));

            await _realtimeService.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(group, SignalRGroups.ActionHub.UpdateCart, cart);
        }

        public async Task ClearCartAsync(Guid tableId, string storeId, string actorId)
        {
            var key = GetCartKey(tableId, storeId, actorId);

            await _redisCacheService.RemoveAsync(key);

            var group = SignalRGroups.User(Guid.Parse(storeId), tableId, Guid.Parse(actorId));

            await _cartHubContext.Clients.Group(group).SendAsync(SignalRGroups.ActionHub.UpdateCart, new List<CartItemRedisModel>());
        }

        public async Task<List<CartItemRedisModel>> GetCartAsync(Guid tableId, string storeId, string actorId)
        {
            var key = GetCartKey(tableId, storeId, actorId);

            var cartItems = await _redisCacheService.GetAsync<List<CartItemRedisModel>>(key);

            return cartItems ?? new List<CartItemRedisModel> { };
        }

        public async Task RemoveItemAsync(Guid tableId, string actorId, string storeId, Guid menuItemId, Guid? variantId, int quantity)
        {
            var key = GetCartKey(tableId, storeId, actorId);

            var cartItems = await _redisCacheService.GetAsync<List<CartItemRedisModel>>(key);

            if(cartItems == null) { return; }

            var itemToRemove = cartItems.FirstOrDefault(x => x.MenuItemId == menuItemId && x.VariantId == variantId);
            if(itemToRemove != null)
            {
                if(itemToRemove.Quantity <= quantity)
                {
                    cartItems.Remove(itemToRemove);
                }

                itemToRemove.Quantity -= quantity;

                await _redisCacheService.SetAsync(key, cartItems, _cacheExpiry);

                var group = SignalRGroups.User(Guid.Parse(storeId), tableId, Guid.Parse(actorId));

                await _cartHubContext.Clients.Group(group).SendAsync(SignalRGroups.ActionHub.UpdateCart, cartItems);
            }

        }

        public string GetCartKey(Guid tableId, string storeId, string actorId)
        {
            return $"cart:{storeId}:{tableId}:{actorId}";
        }

    }
}
