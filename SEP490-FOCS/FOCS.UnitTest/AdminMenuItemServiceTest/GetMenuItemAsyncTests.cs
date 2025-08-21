using FOCS.Common.Exceptions;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.AdminMenuItemServiceTest
{
    public class GetMenuItemAsyncTests : AdminMenuItemServiceTestBase
    {
        [Fact]
        public async Task Should_Return_MenuItem_When_Exists_And_UserAuthorized()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var item = CreateValidMenuItem(storeId);
            item.IsActive = false;

            SetupValidUser(userId, new User { Id = userId }, new UserStore { UserId = Guid.Parse(userId), StoreId = storeId });
            SetupValidStore(storeId, new Store { Id = storeId });
            SetupMenuQueryable(new List<MenuItem> { item });

            var result = await _adminMenuItemService.ActivateMenuItemAsync(item.Id, userId);

            Assert.True(result);
            Assert.True(item.IsActive);
        }

        [Fact]
        public async Task Should_Return_Null_When_MenuItem_NotFound()
        {
            var userId = Guid.NewGuid().ToString();
            var menuItemId = Guid.NewGuid();

            SetupMenuQueryable(new List<MenuItem>()); // empty list

            var result = await _adminMenuItemService.ActivateMenuItemAsync(menuItemId, userId);

            Assert.False(result);
        }

        [Fact]
        public async Task Should_Throw_When_User_NotFound()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var item = CreateValidMenuItem(storeId);
            item.IsActive = false;

            _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((User?)null);
            SetupValidStore(storeId, new Store { Id = storeId });
            SetupMenuQueryable(new List<MenuItem> { item });

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminMenuItemService.ActivateMenuItemAsync(item.Id, userId));

            AssertConditionException(ex, Errors.Common.UserNotFound, Errors.FieldName.UserId);
        }

        [Fact]
        public async Task Should_Throw_When_User_NotAuthorized_To_Access_MenuItem()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var item = CreateValidMenuItem(storeId);
            item.IsActive = false;

            _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
            _userStoreRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(new List<UserStore>()); // empty = unauthorized

            SetupMenuQueryable(new List<MenuItem> { item });

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminMenuItemService.ActivateMenuItemAsync(item.Id, userId));

            AssertConditionException(ex, Errors.AuthError.UserUnauthor, Errors.FieldName.UserId);
        }

        [Fact]
        public async Task Should_Ignore_Deleted_MenuItem()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var item = CreateValidMenuItem(storeId);
            item.IsDeleted = true;

            SetupValidUser(userId, new User { Id = userId }, new UserStore { StoreId = storeId, UserId = Guid.Parse(userId) });
            SetupMenuQueryable(new List<MenuItem> { item });

            var result = await _adminMenuItemService.ActivateMenuItemAsync(item.Id, userId);

            Assert.False(result); // GetMenuItemAsync returns null when item.IsDeleted = true
        }
    }
}