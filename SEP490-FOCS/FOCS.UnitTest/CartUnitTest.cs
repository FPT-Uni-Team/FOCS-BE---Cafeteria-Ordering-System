using AutoMapper;
using FOCS.Application.Services;
using FOCS.Common.Interfaces;
using FOCS.Common.Models.CartModels;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Realtime.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace FOCS.UnitTest
{
    public class CartServiceUnitTest
    {
        private readonly Mock<IRepository<MenuItem>> _menuItemRepositoryMock;
        private readonly Mock<IRepository<MenuItemVariant>> _menuItemVariantRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CartService>> _loggerMock;
        private readonly Mock<IRedisCacheService> _redisMock;
        private readonly Mock<IHubContext<CartHub>> _hubContextMock;
        private readonly Mock<IHubClients> _hubClientsMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly Mock<IRealtimeService> _realtimeMock;
        private readonly CartService _cartService;

        public CartServiceUnitTest()
        {
            _menuItemRepositoryMock = new Mock<IRepository<MenuItem>>();
            _menuItemVariantRepositoryMock = new Mock<IRepository<MenuItemVariant>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<CartService>>();
            _redisMock = new Mock<IRedisCacheService>();

            _hubContextMock = new Mock<IHubContext<CartHub>>();
            _hubClientsMock = new Mock<IHubClients>();
            _clientProxyMock = new Mock<IClientProxy>();

            // mock underlying SendCoreAsync (not the extension SendAsync)
            _clientProxyMock
                .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _hubClientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
            _hubContextMock.SetupGet(h => h.Clients).Returns(_hubClientsMock.Object);

            _realtimeMock = new Mock<IRealtimeService>();
            _realtimeMock
                .Setup(r => r.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>()))
                .Returns(Task.CompletedTask);

            _cartService = new CartService(
                _menuItemRepositoryMock.Object,
                _menuItemVariantRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _redisMock.Object,
                _hubContextMock.Object,
                _realtimeMock.Object
            );
        }

        #region AddOrUpdateItemAsync CM-38
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", true)]
        [InlineData(null, "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "fb05206f-1188-432c-9e5c-4e7094d5b84d", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "fb05206f-1188-432c-9e5c-4e7094d5b84d", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", null, "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", null, 0, "note string", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 1, "note string", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", null, "note string", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, null, false)]
        public async Task AddOrUpdateItemAsync_SimpleRun_ChecksIfServiceRuns(
            string tableIdStr,
            string actorId,
            string menuItemIdStr,
            string variantIdStr,
            int? quantity,
            string note,
            bool shouldSucceed)
        {
            // validate/convert inputs safely: nếu không convert được -> test phải fail (shouldSucceed == false)
            if (!Guid.TryParse(tableIdStr, out var tableId))
            {
                Assert.False(shouldSucceed);
                return;
            }

            if (!Guid.TryParse(menuItemIdStr, out var menuItemId))
            {
                Assert.False(shouldSucceed);
                return;
            }

            Guid? variantId = null;
            if (!string.IsNullOrWhiteSpace(variantIdStr))
            {
                if (!Guid.TryParse(variantIdStr, out var tmp))
                {
                    Assert.False(shouldSucceed);
                    return;
                }
                variantId = tmp;
            }

            if (!quantity.HasValue)
            {
                Assert.False(shouldSucceed);
                return;
            }

            var item = new CartItemRedisModel
            {
                MenuItemId = menuItemId,
                Variants = variantId.HasValue ? new List<CartVariantRedisModel> { new CartVariantRedisModel { VariantId = variantId.Value, Quantity = 1 } } : null,
                Quantity = quantity.Value,
                Note = note
            };

            // First case: cache empty -> Add new item path
            _redisMock.Setup(r => r.GetAsync<List<CartItemRedisModel>>(It.IsAny<string>()))
                .ReturnsAsync((List<CartItemRedisModel>?)null);

            _redisMock.Setup(r => r.SetAsync(It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>(), It.IsAny<TimeSpan?>()))
                .Returns(Task.CompletedTask);

            var ex = await Record.ExceptionAsync(() => _cartService.AddOrUpdateItemAsync(tableId, actorId, item, Guid.NewGuid().ToString()));
            Assert.Null(ex);

            _redisMock.Verify(r => r.SetAsync(It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>(), It.IsAny<TimeSpan?>()), Times.Once);
            _realtimeMock.Verify(r => r.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>()), Times.Once);

            // Second case: existing item in cache -> update quantity path
            var existing = new CartItemRedisModel
            {
                Id = Guid.NewGuid(),
                MenuItemId = menuItemId,
                Variants = item.Variants != null ? item.Variants.Select(v => new CartVariantRedisModel { VariantId = v.VariantId, Quantity = v.Quantity }).ToList() : null,
                Quantity = 1
            };

            _redisMock.Reset();
            _realtimeMock.Reset();

            _redisMock.Setup(r => r.GetAsync<List<CartItemRedisModel>>(It.IsAny<string>()))
                .ReturnsAsync(new List<CartItemRedisModel> { existing });

            _redisMock.Setup(r => r.SetAsync(It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>(), It.IsAny<TimeSpan?>()))
                .Returns(Task.CompletedTask);

            _realtimeMock.Setup(r => r.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>()))
                .Returns(Task.CompletedTask);

            var ex2 = await Record.ExceptionAsync(() => _cartService.AddOrUpdateItemAsync(tableId, actorId, item, Guid.NewGuid().ToString()));
            Assert.Null(ex2);

            _redisMock.Verify(r => r.SetAsync(It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>(), It.IsAny<TimeSpan?>()), Times.Once);
            _realtimeMock.Verify(r => r.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>()), Times.Once);
        }

        #endregion

        #region ClearCartAsync CM-39
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", "a2fca33f-2ff6-4697-903f-1cbe644f5139", true)]
        [InlineData(null, "a2fca33f-2ff6-4697-903f-1cbe644f5139", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "fb05206f-1188-432c-9e5c-4e7094d5b84d", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, false)]
        [InlineData(null, null, false)]
        public async Task ClearCartAsync_SimpleRun_ChecksIfServiceRuns(string tableIdStr, string actorId, bool shouldSucceed)
        {
            if (!Guid.TryParse(tableIdStr, out var tableId))
            {
                Assert.False(shouldSucceed);
                return;
            }

            var storeId = Guid.NewGuid().ToString();

            _redisMock.Setup(r => r.RemoveAsync(It.IsAny<string>())).ReturnsAsync(true);

            var ex = await Record.ExceptionAsync(() => _cartService.ClearCartAsync(tableId, storeId, actorId));
            Assert.Null(ex);

            _redisMock.Verify(r => r.RemoveAsync(It.IsAny<string>()), Times.Once);
            _clientProxyMock.Verify(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetCartAsync CM-40
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", "a2fca33f-2ff6-4697-903f-1cbe644f5139", true)]
        [InlineData(null, "a2fca33f-2ff6-4697-903f-1cbe644f5139", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "fb05206f-1188-432c-9e5c-4e7094d5b84d", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, false)]
        [InlineData(null, null, false)]
        public async Task GetCartAsync_SimpleRun_ChecksIfServiceRuns(string tableIdStr, string actorId, bool shouldSucceed)
        {
            if (!Guid.TryParse(tableIdStr, out var tableId))
            {
                Assert.False(shouldSucceed);
                return;
            }

            var storeId = Guid.NewGuid().ToString();

            var saved = new List<CartItemRedisModel>
            {
                new CartItemRedisModel { Id = Guid.NewGuid(), MenuItemId = Guid.NewGuid(), Quantity = 2 }
            };

            _redisMock.Setup(r => r.GetAsync<List<CartItemRedisModel>>(It.IsAny<string>()))
                .ReturnsAsync(saved);

            var res = await _cartService.GetCartAsync(tableId, storeId, actorId);

            Assert.NotNull(res);
            Assert.Equal(saved.Count, res.Count);
        }

        #endregion

        #region RemoveItemAsync CM-41
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", true)]
        [InlineData(null, "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "fb05206f-1188-432c-9e5c-4e7094d5b84d", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "fb05206f-1188-432c-9e5c-4e7094d5b84d", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", null, "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "note string", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", null, 0, "note string", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 1, "note string", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", null, "note string", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, "", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", "d9bfba5f-5cab-4351-af1e-84cde8422fb2", "3fa85f64-5717-4562-b3fc-2c963f66afa6", 0, null, false)]
        public async Task RemoveItemAsync_SimpleRun_ChecksIfServiceRuns(
            string tableIdStr,
            string actorId,
            string menuItemIdStr,
            string variantIdStr,
            int? quantity,
            string note,
            bool shouldSucceed)
        {
            if (!Guid.TryParse(tableIdStr, out var tableId))
            {
                Assert.False(shouldSucceed);
                return;
            }

            if (!Guid.TryParse(menuItemIdStr, out var menuItemId))
            {
                Assert.False(shouldSucceed);
                return;
            }

            Guid? variantId = null;
            if (!string.IsNullOrWhiteSpace(variantIdStr))
            {
                if (!Guid.TryParse(variantIdStr, out var tmp))
                {
                    Assert.False(shouldSucceed);
                    return;
                }
                variantId = tmp;
            }

            if (!quantity.HasValue)
            {
                Assert.False(shouldSucceed);
                return;
            }

            var storeId = Guid.NewGuid().ToString();

            var existingItem = new CartItemRedisModel
            {
                Id = Guid.NewGuid(),
                MenuItemId = menuItemId,
                Quantity = 2,
                Variants = variantId.HasValue ? new List<CartVariantRedisModel> { new CartVariantRedisModel { VariantId = variantId.Value, Quantity = 1 } } : null
            };

            if (shouldSucceed)
            {
                _redisMock.Setup(r => r.GetAsync<List<CartItemRedisModel>>(It.IsAny<string>()))
                    .ReturnsAsync(new List<CartItemRedisModel> { existingItem });

                _redisMock.Setup(r => r.SetAsync(It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>(), It.IsAny<TimeSpan?>()))
                    .Returns(Task.CompletedTask);

                _realtimeMock.Setup(r => r.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>()))
                    .Returns(Task.CompletedTask);
            }
            else
            {
                _redisMock.Setup(r => r.GetAsync<List<CartItemRedisModel>>(It.IsAny<string>()))
                    .ReturnsAsync(new List<CartItemRedisModel>());
            }

            var ex = await Record.ExceptionAsync(() =>
                _cartService.RemoveItemAsync(tableId, actorId, storeId, menuItemId, variantId.HasValue ? new List<CartVariantRedisModel> { new CartVariantRedisModel { VariantId = variantId.Value, Quantity = 1 } } : null, quantity.Value)
            );

            // service doesn't throw on not-found; we expect no exception but no Set/Send calls when shouldSucceed == false
            Assert.Null(ex);

            if (shouldSucceed)
            {
                _redisMock.Verify(r => r.SetAsync(It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>(), It.IsAny<TimeSpan?>()), Times.Once);
                _realtimeMock.Verify(r => r.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>()), Times.Once);
            }
            else
            {
                _redisMock.Verify(r => r.SetAsync(It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>(), It.IsAny<TimeSpan?>()), Times.Never);
                _realtimeMock.Verify(r => r.SendToGroupAsync<CartHub, List<CartItemRedisModel>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<CartItemRedisModel>>()), Times.Never);
            }
        }
        #endregion
    }
}
