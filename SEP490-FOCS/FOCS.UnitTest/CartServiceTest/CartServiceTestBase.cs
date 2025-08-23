using AutoMapper;
using FOCS.Application.Services;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models.CartModels;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace FOCS.UnitTest.CartServiceTest
{
    public abstract class CartServiceTestBase
    {
        protected readonly Mock<IRepository<MenuItem>> _menuItemRepositoryMock;
        protected readonly Mock<IRepository<MenuItemVariant>> _menuItemVariantRepositoryMock;
        protected readonly Mock<IMapper> _mapperMock;
        protected readonly Mock<ILogger<CartService>> _loggerMock;
        protected readonly Mock<IRedisCacheService> _redisCacheServiceMock;
        protected readonly Mock<IHubContext<CartHub>> _hubContextMock;
        protected readonly Mock<IRealtimeService> _realtimeServiceMock;

        protected readonly CartService _cartService;

        protected CartServiceTestBase()
        {
            _menuItemRepositoryMock = new Mock<IRepository<MenuItem>>();
            _menuItemVariantRepositoryMock = new Mock<IRepository<MenuItemVariant>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<CartService>>();
            _redisCacheServiceMock = new Mock<IRedisCacheService>();
            _hubContextMock = new Mock<IHubContext<CartHub>>();
            _realtimeServiceMock = new Mock<IRealtimeService>();

            _cartService = new CartService(
                _menuItemRepositoryMock.Object,
                _menuItemVariantRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _redisCacheServiceMock.Object,
                _hubContextMock.Object,
                _realtimeServiceMock.Object
            );
        }

        protected void SetupCacheForKey(Guid tableId, string storeId, List<CartItemRedisModel>? cart)
        {
            var key = _cartService.GetCartKey(tableId, storeId);
            _redisCacheServiceMock
                .Setup(r => r.GetAsync<List<CartItemRedisModel>>(key))
                .ReturnsAsync(cart);
        }

        protected void VerifyCacheSet(Guid tableId, string storeId, Times times)
        {
            var key = _cartService.GetCartKey(tableId, storeId);
            _redisCacheServiceMock.Verify(r =>
                r.SetAsync(key, It.IsAny<List<CartItemRedisModel>>(), It.IsAny<TimeSpan>()), times);
        }

        protected void VerifyCacheRemove(Guid tableId, string storeId, Times times)
        {
            var key = _cartService.GetCartKey(tableId, storeId);
            _redisCacheServiceMock.Verify(r => r.RemoveAsync(key), times);
        }

        protected void VerifyHubSend(string groupName, Times times)
        {
            _realtimeServiceMock.Verify(r =>
                r.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(
                    groupName,
                    SignalRGroups.ActionHub.UpdateCart,
                    It.IsAny<List<CartItemRedisModel>>()),
                times);
        }
    }
}
