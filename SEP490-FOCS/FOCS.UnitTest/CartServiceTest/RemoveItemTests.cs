using FOCS.Common.Constants;
using FOCS.Common.Models.CartModels;
using Moq;

namespace FOCS.UnitTest.CartServiceTest
{
    public class RemoveItemTests : CartServiceTestBase
    {
        private readonly Guid _tableId = Guid.NewGuid();
        private readonly string _actorId = Guid.NewGuid().ToString();
        private readonly string _storeId = Guid.NewGuid().ToString();
        private readonly Guid _menuItemId = Guid.NewGuid();
        private readonly Guid _variantId = Guid.NewGuid();

        [Fact]
        public async Task Should_DoNothing_WhenCartIsNull()
        {
            SetupCacheForKey(_tableId, _storeId, null);

            await _cartService.RemoveItemAsync(_tableId, _actorId, _storeId, _menuItemId, new(), 1);

            VerifyCacheSet(_tableId, _storeId, Times.Never());
            VerifyHubSend(SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId), Times.Never());
        }

        [Fact]
        public async Task Should_DoNothing_WhenItemNotFound()
        {
            var cart = new List<CartItemRedisModel>();
            SetupCacheForKey(_tableId, _storeId, cart);

            await _cartService.RemoveItemAsync(_tableId, _actorId, _storeId, _menuItemId, null, 1);

            VerifyCacheSet(_tableId, _storeId, Times.Never());
            VerifyHubSend(SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId), Times.Never());
        }

        [Fact]
        public async Task Should_RemoveItemCompletely_WhenNoVariant_And_QuantitySufficient()
        {
            var cart = new List<CartItemRedisModel>
            {
                new() { MenuItemId = _menuItemId, Quantity = 1 }
            };
            SetupCacheForKey(_tableId, _storeId, cart);

            await _cartService.RemoveItemAsync(_tableId, _actorId, _storeId, _menuItemId, null, 1);

            Assert.Empty(cart);
            VerifyCacheSet(_tableId, _storeId, Times.Once());
            VerifyHubSend(SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId), Times.Once());
        }

        [Fact]
        public async Task Should_DecreaseQuantity_WhenNoVariant_And_QuantityLess()
        {
            var cart = new List<CartItemRedisModel>
            {
                new() { MenuItemId = _menuItemId, Quantity = 5 }
            };
            SetupCacheForKey(_tableId, _storeId, cart);

            await _cartService.RemoveItemAsync(_tableId, _actorId, _storeId, _menuItemId, null, 2);

            Assert.Equal(3, cart.First().Quantity);
            VerifyCacheSet(_tableId, _storeId, Times.Once());
            VerifyHubSend(SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId), Times.Once());
        }

        [Fact]
        public async Task Should_RemoveVariant_WhenVariantQuantityLessOrEqual()
        {
            var variant = new CartVariantRedisModel { VariantId = _variantId, Quantity = 1 };
            var cart = new List<CartItemRedisModel>
            {
                new()
                {
                    MenuItemId = _menuItemId,
                    Quantity = 1,
                    Variants = new List<CartVariantRedisModel> { variant }
                }
            };
            SetupCacheForKey(_tableId, _storeId, cart);

            var removeVariants = new List<CartVariantRedisModel> { new() { VariantId = _variantId, Quantity = 1 } };
            await _cartService.RemoveItemAsync(_tableId, _actorId, _storeId, _menuItemId, removeVariants, 1);

            Assert.Empty(cart);
            VerifyCacheSet(_tableId, _storeId, Times.Once());
            VerifyHubSend(SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId), Times.Once());
        }

        [Fact]
        public async Task Should_DecreaseVariantQuantity_WhenVariantQuantityGreater()
        {
            var variant = new CartVariantRedisModel { VariantId = _variantId, Quantity = 3 };
            var cart = new List<CartItemRedisModel>
            {
                new()
                {
                    MenuItemId = _menuItemId,
                    Quantity = 2,
                    Variants = new List<CartVariantRedisModel> { variant }
                }
            };
            SetupCacheForKey(_tableId, _storeId, cart);

            var removeVariants = new List<CartVariantRedisModel> { new() { VariantId = _variantId, Quantity = 1 } };
            await _cartService.RemoveItemAsync(_tableId, _actorId, _storeId, _menuItemId, removeVariants, 1);

            var updatedVariant = cart.First().Variants!.First();
            Assert.Equal(2, updatedVariant.Quantity);
            Assert.Single(cart);
            VerifyCacheSet(_tableId, _storeId, Times.Once());
            VerifyHubSend(SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId), Times.Once());
        }
    }
}