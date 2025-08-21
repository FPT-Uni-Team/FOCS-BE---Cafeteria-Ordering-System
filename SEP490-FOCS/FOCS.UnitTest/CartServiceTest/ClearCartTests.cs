using FOCS.Common.Constants;
using FOCS.Common.Models.CartModels;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace FOCS.UnitTest.CartServiceTest
{
    public class ClearCartTests : CartServiceTestBase
    {
        private readonly Guid _tableId = Guid.NewGuid();
        private const string _storeId = "00000000-0000-0000-0000-000000000001";
        private const string _actorId = "actor-123";

        [Fact]
        public async Task ClearCartAsync_ShouldRemoveCacheAndSendEmptyCart_UpdateCart()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var storeId = Guid.NewGuid().ToString();
            var actorId = Guid.NewGuid().ToString();

            var key = $"cart:{storeId}:{tableId}";
            var groupName = SignalRGroups.CartUpdate(Guid.Parse(storeId), tableId);

            var mockClientProxy = new Mock<IClientProxy>();

            _redisCacheServiceMock
                .Setup(r => r.RemoveAsync(key))
                .ReturnsAsync(true);

            _hubContextMock
                .Setup(h => h.Clients.Group(groupName))
                .Returns(mockClientProxy.Object);

            List<CartItemRedisModel>? sentPayload = null;

            mockClientProxy.Setup(c => c.SendCoreAsync(
                SignalRGroups.ActionHub.UpdateCart,
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, object?[], CancellationToken>((method, args, _) =>
            {
                sentPayload = args[0] as List<CartItemRedisModel>;
            })
            .Returns(Task.CompletedTask);

            // Act
            await _cartService.ClearCartAsync(tableId, storeId, actorId);

            // Assert
            _redisCacheServiceMock.Verify(r => r.RemoveAsync(key), Times.Once);
            _hubContextMock.Verify(h => h.Clients.Group(groupName), Times.Once);

            Assert.NotNull(sentPayload);
            Assert.Empty(sentPayload!);
        }

        [Fact]
        public async Task ClearCartAsync_ShouldNotFail_WhenCacheRemoveReturnsFalse()
        {
            // Arrange
            var key = _cartService.GetCartKey(_tableId, _storeId);

            _redisCacheServiceMock
                .Setup(r => r.RemoveAsync(key))
                .ReturnsAsync(false);

            var mockClientProxy = new Mock<IClientProxy>();

            _hubContextMock
                .Setup(h => h.Clients.Group(SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId)))
                .Returns(mockClientProxy.Object);

            List<CartItemRedisModel>? sentPayload = null;

            mockClientProxy
                .Setup(c => c.SendCoreAsync(
                    SignalRGroups.ActionHub.UpdateCart,
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, object[], CancellationToken>((method, args, _) =>
                {
                    sentPayload = args[0] as List<CartItemRedisModel>;
                })
                .Returns(Task.CompletedTask);

            // Act
            var ex = await Record.ExceptionAsync(() =>
                _cartService.ClearCartAsync(_tableId, _storeId, _actorId));

            // Assert
            Assert.Null(ex);

            Assert.NotNull(sentPayload);
            Assert.Empty(sentPayload!);
        }

        [Fact]
        public async Task ClearCartAsync_ShouldUseCorrectCartKeyFormat()
        {
            // Arrange
            var expectedKey = $"cart:{_storeId}:{_tableId}";
            _redisCacheServiceMock.Setup(r => r.RemoveAsync(It.IsAny<string>()))
                                  .ReturnsAsync(true);

            var mockClientProxy = new Mock<IClientProxy>();
            _hubContextMock.Setup(h => h.Clients.Group(It.IsAny<string>()))
                .Returns(mockClientProxy.Object);

            // Act
            await _cartService.ClearCartAsync(_tableId, _storeId, _actorId);

            // Assert
            _redisCacheServiceMock.Verify(r => r.RemoveAsync(expectedKey), Times.Once);
        }

        [Fact]
        public async Task ClearCartAsync_ShouldSendToCorrectSignalRGroup()
        {
            // Arrange
            var expectedGroup = SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId);
            var mockClientProxy = new Mock<IClientProxy>();

            _redisCacheServiceMock.Setup(r => r.RemoveAsync(It.IsAny<string>()))
                                  .ReturnsAsync(true);

            _hubContextMock.Setup(h => h.Clients.Group(expectedGroup))
                .Returns(mockClientProxy.Object)
                .Verifiable();

            // Act
            await _cartService.ClearCartAsync(_tableId, _storeId, _actorId);

            // Assert
            _hubContextMock.Verify(h => h.Clients.Group(expectedGroup), Times.Once);
        }
    }
}
