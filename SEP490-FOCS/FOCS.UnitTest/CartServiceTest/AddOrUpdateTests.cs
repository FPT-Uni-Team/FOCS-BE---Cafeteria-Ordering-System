using FOCS.Common.Constants;
using FOCS.Common.Models.CartModels;
using Moq;

namespace FOCS.UnitTest.CartServiceTest
{
    public class AddOrUpdateTests : CartServiceTestBase
    {
        private readonly Guid _tableId = Guid.NewGuid();
        private const string _storeId = "00000000-0000-0000-0000-000000000001";
        private const string _actorId = "actor-123";

        [Fact]
        public async Task AddOrUpdateItemAsync_ShouldAddNewItem_WhenCartIsEmpty()
        {
            var item = new CartItemRedisModel { MenuItemId = Guid.NewGuid(), Quantity = 2, Variants = null };
            SetupCacheForKey(_tableId, _storeId, null);

            await _cartService.AddOrUpdateItemAsync(_tableId, _actorId, item, _storeId);

            VerifyCacheSet(_tableId, _storeId, Times.Once());
            var expectedGroup = SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId);
            VerifyHubSend(expectedGroup, Times.Once());
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_ShouldIncrementQuantity_WhenItemExists_NoVariants()
        {
            var menuItemId = Guid.NewGuid();
            var existing = new CartItemRedisModel { MenuItemId = menuItemId, Quantity = 1, Variants = null };
            var incoming = new CartItemRedisModel { MenuItemId = menuItemId, Quantity = 3, Variants = null };
            SetupCacheForKey(_tableId, _storeId, new List<CartItemRedisModel> { existing });

            await _cartService.AddOrUpdateItemAsync(_tableId, _actorId, incoming, _storeId);

            VerifyCacheSet(_tableId, _storeId, Times.Once());
            VerifyHubSend(SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId), Times.Once());
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_ShouldAddSeparateItem_WhenVariantsDiffer()
        {
            var menuItemId = Guid.NewGuid();
            var existing = new CartItemRedisModel
            {
                MenuItemId = menuItemId,
                Quantity = 1,
                Variants = new List<CartVariantRedisModel>
                {
                    new CartVariantRedisModel { VariantId = Guid.NewGuid(), Quantity = 1 }
                }
            };
            var incoming = new CartItemRedisModel
            {
                MenuItemId = menuItemId,
                Quantity = 2,
                Variants = new List<CartVariantRedisModel>
                {
                    new CartVariantRedisModel { VariantId = Guid.NewGuid(), Quantity = 2 }
                }
            };
            SetupCacheForKey(_tableId, _storeId, new List<CartItemRedisModel> { existing });

            await _cartService.AddOrUpdateItemAsync(_tableId, _actorId, incoming, _storeId);

            VerifyCacheSet(_tableId, _storeId, Times.Once());
            VerifyHubSend(SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId), Times.Once());
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_ShouldMergeVariants_WhenVariantsEqual()
        {
            var menuItemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var existing = new CartItemRedisModel
            {
                MenuItemId = menuItemId,
                Quantity = 1,
                Variants = new List<CartVariantRedisModel>
                {
                    new CartVariantRedisModel { VariantId = variantId, Quantity = 1 }
                }
            };
            var incoming = new CartItemRedisModel
            {
                MenuItemId = menuItemId,
                Quantity = 2,
                Variants = new List<CartVariantRedisModel>
                {
                    new CartVariantRedisModel { VariantId = variantId, Quantity = 3 }
                }
            };
            SetupCacheForKey(_tableId, _storeId, new List<CartItemRedisModel> { existing });

            await _cartService.AddOrUpdateItemAsync(_tableId, _actorId, incoming, _storeId);

            VerifyCacheSet(_tableId, _storeId, Times.Once());
            VerifyHubSend(SignalRGroups.CartUpdate(Guid.Parse(_storeId), _tableId), Times.Once());
        }
    }
}
