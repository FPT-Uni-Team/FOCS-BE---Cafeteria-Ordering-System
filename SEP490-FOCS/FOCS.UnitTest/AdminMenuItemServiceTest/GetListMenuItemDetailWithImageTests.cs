using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Exceptions;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FOCS.UnitTest.AdminMenuItemServiceTest
{
    public class GetListMenuItemDetailWithImageTests : AdminMenuItemServiceTestBase
    {
        [Fact]
        public async Task Should_Return_List_When_ValidInput()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var menuItemIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var menuItems = menuItemIds.Select(id =>
            {
                var item = CreateValidMenuItem(storeId);
                item.Id = id;
                return item;
            }).ToList();

            SetupValidUser(userId.ToString(), new User { Id = userId.ToString() }, new UserStore { StoreId = storeId, UserId = userId });
            SetupValidStore(storeId, new Store { Id = storeId });
            SetupMenuQueryable(menuItems);

            _mapperMock.Setup(m => m.Map<List<MenuItemDetailAdminDTO>>(menuItems))
                .Returns(menuItems.Select(m => new MenuItemDetailAdminDTO { Id = m.Id }).ToList());

            var result = await _adminMenuItemService.GetListMenuItemDetail(menuItemIds, storeId.ToString(), userId.ToString());

            Assert.Equal(menuItemIds.Count, result.Count);
            Assert.All(result, dto => Assert.Contains(dto.Id, menuItemIds));
        }

        [Fact]
        public async Task Should_Throw_When_StoreId_Invalid()
        {
            var invalidStoreId = "abc"; // not a GUID
            var menuItemIds = new List<Guid> { Guid.NewGuid() };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminMenuItemService.GetListMenuItemDetail(menuItemIds, invalidStoreId, "user"));

            AssertConditionException(ex, Errors.Common.InvalidGuidFormat);
        }

        [Fact]
        public async Task Should_Throw_When_User_NotAuthorized()
        {
            var storeId = Guid.NewGuid();
            var userId = "user-not-allowed";
            var menuItemIds = new List<Guid> { Guid.NewGuid() };

            SetupValidUser(userId, new User { Id = userId }, null); // No UserStore

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminMenuItemService.GetListMenuItemDetail(menuItemIds, storeId.ToString(), userId));

            AssertConditionException(ex, Errors.AuthError.UserUnauthor);
        }

        [Fact]
        public async Task Should_Throw_When_Store_NotFound()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var menuItemIds = new List<Guid> { Guid.NewGuid() };

            SetupValidUser(userId, new User { Id = userId }, new UserStore { StoreId = storeId, UserId = Guid.Parse(userId) });
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId)).ReturnsAsync((Store?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminMenuItemService.GetListMenuItemDetail(menuItemIds, storeId.ToString(), userId));

            AssertConditionException(ex, Errors.Common.StoreNotFound);
        }

        [Fact]
        public async Task Should_Return_Empty_When_NoMenuItemFound()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var menuItemIds = new List<Guid> { Guid.NewGuid() };

            SetupValidUser(userId, new User { Id = userId }, new UserStore { StoreId = storeId, UserId = Guid.Parse(userId) });
            SetupValidStore(storeId, new Store { Id = storeId });

            SetupMenuQueryable(new List<MenuItem>());

            _mapperMock.Setup(m => m.Map<List<MenuItemDetailAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns(new List<MenuItemDetailAdminDTO>());

            var result = await _adminMenuItemService.GetListMenuItemDetail(menuItemIds, storeId.ToString(), userId);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_Map_CorrectMenuItems_To_DTO()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var menuItemId = Guid.NewGuid();
            var item = CreateValidMenuItem(storeId);
            item.Id = menuItemId;

            SetupValidUser(userId.ToString(), new User { Id = userId.ToString() }, new UserStore { StoreId = storeId, UserId = userId });
            SetupValidStore(storeId, new Store { Id = storeId });
            SetupMenuQueryable(new List<MenuItem> { item });

            _mapperMock.Setup(m => m.Map<List<MenuItemDetailAdminDTO>>(It.Is<List<MenuItem>>(l => l.Count == 1 && l[0].Id == item.Id)))
                .Returns(new List<MenuItemDetailAdminDTO> { new MenuItemDetailAdminDTO { Id = item.Id } });

            var result = await _adminMenuItemService.GetListMenuItemDetail(new List<Guid> { menuItemId }, storeId.ToString(), userId.ToString());

            _mapperMock.Verify(m => m.Map<List<MenuItemDetailAdminDTO>>(It.IsAny<List<MenuItem>>()), Times.Once);
            Assert.Single(result);
            Assert.Equal(item.Id, result[0].Id);
        }

        [Fact]
        public async Task Should_Throw_When_User_NotFound()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var menuItemIds = new List<Guid> { Guid.NewGuid() };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((User?)null); // User not found

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminMenuItemService.GetListMenuItemDetail(menuItemIds, storeId.ToString(), userId));

            AssertConditionException(ex, Errors.Common.UserNotFound);
        }

        [Fact]
        public async Task Should_Ignore_MenuItems_From_OtherStores()
        {
            var storeId = Guid.NewGuid();
            var otherStoreId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var menuItemIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var menuItems = new List<MenuItem>
            {
                new MenuItem { Id = menuItemIds[0], StoreId = storeId },
                new MenuItem { Id = menuItemIds[1], StoreId = otherStoreId } // wrong store
            };

            SetupValidUser(userId.ToString(), new User { Id = userId.ToString() }, new UserStore { StoreId = storeId, UserId = userId });
            SetupValidStore(storeId, new Store { Id = storeId });
            SetupMenuQueryable(menuItems);

            _mapperMock.Setup(m => m.Map<List<MenuItemDetailAdminDTO>>(It.Is<List<MenuItem>>(list => list.Count == 1 && list[0].StoreId == storeId)))
                .Returns(new List<MenuItemDetailAdminDTO> { new MenuItemDetailAdminDTO { Id = menuItemIds[0] } });

            var result = await _adminMenuItemService.GetListMenuItemDetail(menuItemIds, storeId.ToString(), userId.ToString());

            Assert.Single(result);
            Assert.Equal(menuItemIds[0], result[0].Id);
        }

        [Fact]
        public async Task Should_Handle_Duplicate_MenuItemIds_Gracefully()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var menuItemIds = new List<Guid> { itemId, itemId }; // duplicated

            var item = CreateValidMenuItem(storeId);
            item.Id = itemId;

            SetupValidUser(userId.ToString(), new User { Id = userId.ToString() }, new UserStore { StoreId = storeId, UserId = userId });
            SetupValidStore(storeId, new Store { Id = storeId });
            SetupMenuQueryable(new List<MenuItem> { item });

            _mapperMock.Setup(m => m.Map<List<MenuItemDetailAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns(new List<MenuItemDetailAdminDTO> { new MenuItemDetailAdminDTO { Id = itemId } });

            var result = await _adminMenuItemService.GetListMenuItemDetail(menuItemIds, storeId.ToString(), userId.ToString());

            Assert.Single(result); // vẫn là 1 item duy nhất
            Assert.Equal(itemId, result[0].Id);
        }

    }
}
