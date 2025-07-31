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
using MimeKit.Cryptography;
using Net.payOS.Types;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
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
            var key = GetCartKey(tableId, storeId);

            var cart = await _redisCacheService.GetAsync<List<CartItemRedisModel>>(key) ?? new List<CartItemRedisModel>();

            var existingItem = cart.FirstOrDefault(x => x.MenuItemId == item.MenuItemId);

            if (existingItem != null)
            {
                foreach(var itemVariant in item.Variants)
                {
                    var currentVariant = existingItem.Variants.FirstOrDefault(x => x.VariantId == itemVariant.VariantId);
                    if(currentVariant != null)
                    {
                        currentVariant.Quantity += itemVariant.Quantity;
                    } else
                    {
                        existingItem.Variants.Add(new CartVariantRedisModel
                        {
                            VariantId = itemVariant.VariantId,
                            Quantity = itemVariant.Quantity,
                        });
                    }
                }
            }
            else
            {
                cart.Add(item);
            }

            await _redisCacheService.SetAsync(key, cart, _cacheExpiry);

            var group = SignalRGroups.CartUpdate(Guid.Parse(storeId), tableId);
            await _realtimeService.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(group, SignalRGroups.ActionHub.UpdateCart, cart);
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

        public async Task RemoveItemAsync(Guid tableId, string actorId, string storeId, Guid menuItemId, List<CartVariantRedisModel> variants, int quantity)
        {
            var key = GetCartKey(tableId, storeId);
            var cartItems = await _redisCacheService.GetAsync<List<CartItemRedisModel>>(key);

            if (cartItems == null) return;

            var itemToRemove = cartItems.FirstOrDefault(x => x.MenuItemId == menuItemId);
            if (itemToRemove == null) return;

            if (variants != null && variants.Any())
            {
                foreach (var variant in variants)
                {
                    var currentVariant = itemToRemove.Variants?.FirstOrDefault(x => x.VariantId == variant.VariantId);
                    if (currentVariant != null)
                    {
                        if (currentVariant.Quantity <= quantity)
                        {
                            itemToRemove.Variants.Remove(currentVariant);
                        }
                        else
                        {
                            currentVariant.Quantity -= variant.Quantity;
                        }
                    }
                }

                if ((itemToRemove.Variants == null || !itemToRemove.Variants.Any()) && itemToRemove.Quantity <= 1)
                {
                    cartItems.Remove(itemToRemove);
                }
            }
            else
            {
                if (itemToRemove.Quantity <= quantity)
                {
                    cartItems.Remove(itemToRemove);
                }
                else
                {
                    itemToRemove.Quantity -= quantity;
                }
            }

            await _redisCacheService.SetAsync(key, cartItems, _cacheExpiry);

            var group = SignalRGroups.CartUpdate(Guid.Parse(storeId), tableId);
            await _realtimeService.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(group, SignalRGroups.ActionHub.UpdateCart, cartItems);
        }



        public string GetCartKey(Guid tableId, string storeId)
        {
            return $"cart:{storeId}:{tableId}";
        }

    }
}
