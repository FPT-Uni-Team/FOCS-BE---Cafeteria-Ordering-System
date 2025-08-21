using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.AdminMenuItemServiceTest
{
    public class AdminMenuItemServiceTestBase
    {
        protected readonly Mock<IRepository<MenuItem>> _menuRepositoryMock;
        protected readonly Mock<IRepository<Store>> _storeRepositoryMock;
        protected readonly Mock<UserManager<User>> _userManagerMock;
        protected readonly Mock<IRepository<UserStore>> _userStoreRepositoryMock;
        protected readonly Mock<IMapper> _mapperMock;
        protected readonly Mock<IRepository<Category>> _menuCategoryMock;
        protected readonly Mock<IRepository<MenuItemImage>> _menuItemImageRepositoryMock;
        protected readonly AdminMenuItemService _adminMenuItemService;

        public AdminMenuItemServiceTestBase()
        {
            _menuRepositoryMock = new Mock<IRepository<MenuItem>>();
            _storeRepositoryMock = new Mock<IRepository<Store>>();

            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _userStoreRepositoryMock = new Mock<IRepository<UserStore>>();
            _mapperMock = new Mock<IMapper>();
            _menuCategoryMock = new Mock<IRepository<Category>>();
            _menuItemImageRepositoryMock = new Mock<IRepository<MenuItemImage>>();

            _adminMenuItemService = new AdminMenuItemService(
                _menuRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _userManagerMock.Object,
                _userStoreRepositoryMock.Object,
                _mapperMock.Object,
                _menuCategoryMock.Object,
                _menuItemImageRepositoryMock.Object
            );
        }

        protected MenuItem CreateValidMenuItem(Guid storeId)
        {
            return new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = "Test Menu Item",
                Description = "This is a test menu item.",
                BasePrice = 15.99,
                IsAvailable = true,
                IsActive = true,
                IsDeleted = false,
                StoreId = storeId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "test-user",
                UpdatedAt = null,
                UpdatedBy = null,
                MenuItemVariantGroups = new List<MenuItemVariantGroup>(),
                Images = new List<MenuItemImage>()
            };
        }


        protected MenuItemAdminDTO CreateValidMenuItemAdminDTO(Guid storeId)
        {
            return new MenuItemAdminDTO
            {
                Id = Guid.NewGuid(),
                Name = "Test Menu Item",
                Description = "This is a test menu item.",
                BasePrice = 15.99,
                IsAvailable = true,
                IsActive = true,
                StoreId = storeId
            };
        }

        protected void SetupValidUser(string userId, User user, UserStore? userStore = null)
        {
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userStoreRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStore != null ? new List<UserStore> { userStore } : []);
        }

        protected void SetupValidStore(Guid storeId, Store store)
        {
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync(store);
        }

        protected void SetupMenuQueryable(List<MenuItem> menuItems)
        {
            var mockDbSet = menuItems.AsQueryable().BuildMockDbSet();
            _menuRepositoryMock.Setup(r => r.AsQueryable()).Returns(mockDbSet.Object);
        }

        protected void SetupMapperForCreate(MenuItemAdminDTO dto, MenuItem entity, MenuItemAdminDTO resultDto)
        {
            _mapperMock.Setup(m => m.Map<MenuItem>(dto)).Returns(entity);
            _mapperMock.Setup(m => m.Map<MenuItemAdminDTO>(It.IsAny<MenuItem>())).Returns(resultDto);
        }

        protected void SetupMenuItemNameUniqueness(string name, bool exists)
        {
            var data = exists
                ? new List<MenuItem> { new MenuItem { Name = name } }
                : new List<MenuItem>();

            var queryable = data.AsQueryable().BuildMockDbSet();
            _menuRepositoryMock.Setup(x => x.AsQueryable()).Returns(queryable.Object);
        }

        protected void SetupMapperForCreation(MenuItemAdminDTO dto, MenuItem entity, MenuItemAdminDTO resultDto)
        {
            _mapperMock.Setup(x => x.Map<MenuItem>(dto)).Returns(entity);
            _mapperMock.Setup(x => x.Map<MenuItemAdminDTO>(It.IsAny<MenuItem>())).Returns(resultDto);
        }

        protected void AssertConditionException(Exception ex, string expectedMessage, string? expectedField = null)
        {
            Assert.NotNull(ex);

            var parts = ex.Message.Split('@');

            Assert.True(parts.Length >= 1, "Exception message format is invalid.");
            Assert.Equal(expectedMessage, parts[0]);

            if (expectedField != null)
            {
                Assert.True(parts.Length == 2, "Expected field name was not provided in exception message.");
                Assert.Equal(expectedField, parts[1]);
            }
        }

    }
}