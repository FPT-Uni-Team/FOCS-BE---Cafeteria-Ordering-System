using FOCS.Common.Exceptions;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.AdminMenuItemServiceTest
{
    public class UpdateMenuItemStatusTests : AdminMenuItemServiceTestBase
    {
        [Fact]
        public async Task ActivateMenuItem_Should_Set_IsActive_True_When_Valid()
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var item = CreateValidMenuItem(storeId);
            item.IsActive = false;

            SetupValidUser(userId, new User { Id = userId }, new UserStore { StoreId = storeId, UserId = Guid.Parse(userId) });
            SetupValidStore(storeId, new Store { Id = storeId });
            SetupMenuQueryable(new List<MenuItem> { item });

            var result = await _adminMenuItemService.ActivateMenuItemAsync(item.Id, userId);

            Assert.True(result);
            Assert.True(item.IsActive);
            Assert.Equal(userId, item.UpdatedBy);
            Assert.NotNull(item.UpdatedAt);
            _menuRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ActivateMenuItem_Should_Throw_When_Already_Active()
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var item = CreateValidMenuItem(storeId);
            item.IsActive = true;

            SetupValidUser(userId, new User { Id = userId }, new UserStore { StoreId = storeId, UserId = Guid.Parse(userId) });
            SetupValidStore(storeId, new Store { Id = storeId });
            SetupMenuQueryable(new List<MenuItem> { item });

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminMenuItemService.ActivateMenuItemAsync(item.Id, userId));

            AssertConditionException(ex, Errors.MenuItemError.MenuItemActive, Errors.FieldName.IsActive);
        }

        [Fact]
        public async Task DisableMenuItem_Should_Set_IsAvailable_False_When_Valid()
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var item = CreateValidMenuItem(storeId);
            item.IsAvailable = true;

            SetupValidUser(userId, new User { Id = userId }, new UserStore { StoreId = storeId, UserId = Guid.Parse(userId) });
            SetupValidStore(storeId, new Store { Id = storeId });
            SetupMenuQueryable(new List<MenuItem> { item });

            var result = await _adminMenuItemService.DisableMenuItemAsync(item.Id, userId);

            Assert.True(result);
            Assert.False(item.IsAvailable);
            _menuRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DisableMenuItem_Should_Throw_When_Already_Unavailable()
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var item = CreateValidMenuItem(storeId);
            item.IsAvailable = false;

            SetupValidUser(userId, new User { Id = userId }, new UserStore { StoreId = storeId, UserId = Guid.Parse(userId) });
            SetupValidStore(storeId, new Store { Id = storeId });
            SetupMenuQueryable(new List<MenuItem> { item });

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminMenuItemService.DisableMenuItemAsync(item.Id, userId));

            AssertConditionException(ex, Errors.MenuItemError.MenuItemUnavailable, Errors.FieldName.IsAvailable);
        }

        [Fact]
        public async Task UpdateMenuItemStatus_Should_ReturnFalse_If_Item_NotFound()
        {
            var userId = Guid.NewGuid().ToString();
            var menuItemId = Guid.NewGuid();

            // Simulate no menu item returned
            SetupMenuQueryable(new List<MenuItem>());

            var result = await _adminMenuItemService.ActivateMenuItemAsync(menuItemId, userId);

            Assert.False(result);
            _menuRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}