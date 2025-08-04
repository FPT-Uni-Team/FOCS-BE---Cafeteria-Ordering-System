using FOCS.Common.Models.CartModels;
using Moq;

namespace FOCS.UnitTest.CartServiceTest
{
    public class GetCartTests : CartServiceTestBase
    {
        private readonly Guid _tableId = Guid.NewGuid();
        private const string _storeId = "00000000-0000-0000-0000-000000000001";
        private const string _actorId = "actor-abc";

        [Fact]
        public async Task GetCartAsync_ShouldReturnEmptyList_WhenCacheIsNull()
        {
            // Arrange
            var key = _cartService.GetCartKey(_tableId, _storeId);
            _redisCacheServiceMock.Setup(r => r.GetAsync<List<CartItemRedisModel>>(key))
                .ReturnsAsync((List<CartItemRedisModel>?)null);

            // Act
            var result = await _cartService.GetCartAsync(_tableId, _storeId, _actorId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCartAsync_ShouldReturnCachedItems_WhenExist()
        {
            // Arrange
            var expectedCart = new List<CartItemRedisModel>
        {
            new CartItemRedisModel
            {
                MenuItemId = Guid.NewGuid(),
                Quantity = 2,
                Variants = new List<CartVariantRedisModel>
                {
                    new CartVariantRedisModel
                    {
                        VariantId = Guid.NewGuid(),
                        Quantity = 1
                    }
                }
            }
        };
            var key = _cartService.GetCartKey(_tableId, _storeId);
            _redisCacheServiceMock.Setup(r => r.GetAsync<List<CartItemRedisModel>>(key))
                .ReturnsAsync(expectedCart);

            // Act
            var result = await _cartService.GetCartAsync(_tableId, _storeId, _actorId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedCart[0].MenuItemId, result[0].MenuItemId);
            Assert.Equal(expectedCart[0].Quantity, result[0].Quantity);
            Assert.Equal(expectedCart[0].Variants!.Count, result[0].Variants!.Count);
        }

        [Fact]
        public async Task GetCartAsync_ShouldCallRedisWithCorrectKey()
        {
            // Arrange
            var key = _cartService.GetCartKey(_tableId, _storeId);
            _redisCacheServiceMock.Setup(r => r.GetAsync<List<CartItemRedisModel>>(key))
                .ReturnsAsync(new List<CartItemRedisModel>());

            // Act
            var result = await _cartService.GetCartAsync(_tableId, _storeId, _actorId);

            // Assert
            _redisCacheServiceMock.Verify(r => r.GetAsync<List<CartItemRedisModel>>(key), Times.Once);
        }
    }
}
