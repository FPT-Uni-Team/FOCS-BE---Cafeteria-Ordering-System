using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace FOCS.UnitTest
{
    public class MenuItemUnitTest
    {
        private readonly Mock<IRepository<MenuItem>> _menuRepositoryMock;
        private readonly Mock<IRepository<Category>> _menuCategoryMock;
        private readonly Mock<IRepository<MenuItemImage>> _menuItemImageRepositoryMock;
        private readonly Mock<IRepository<UserStore>> _userStoreRepositoryMock;
        private readonly Mock<IRepository<Store>> _storeRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<MenuItem>> _loggerMock;
        private readonly AdminMenuItemService _adminMenuItemService;

        public MenuItemUnitTest()
        {
            _menuRepositoryMock = new Mock<IRepository<MenuItem>>();
            _menuCategoryMock = new Mock<IRepository<Category>>();
            _menuItemImageRepositoryMock = new Mock<IRepository<MenuItemImage>>();
            _userStoreRepositoryMock = new Mock<IRepository<UserStore>>();
            _storeRepositoryMock = new Mock<IRepository<Store>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<MenuItem>>();
            _userManagerMock = MockUserManager();

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

        private static Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        #region Helpers
        private void SetupMinimalStoreAndUser(Guid storeId, string userId)
        {
            _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId)).ReturnsAsync(new Store { Id = storeId });

            var user = new User { Id = userId };
            _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            var userStores = new List<UserStore>
            {
                new UserStore { UserId = Guid.Parse(userId), StoreId = storeId }
            };
            _userStoreRepositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);
        }
        #endregion

        #region CreateMenu CM-20
        [Theory]
        [InlineData("String Name", "String Description", 1, true, true)]
        [InlineData("String Name", "String Description", 0, true, true)]
        [InlineData("String Name", "String Description", 1, false, true)]
        [InlineData(null, "String Description", 1, true, true)]
        [InlineData("String Name", null, 1, true, true)]
        [InlineData("String Name", "String Description", null, true, true)]
        [InlineData("String Name", "String Description", 1, null, true)]
        [InlineData("String Name", "String Description", 1, true, true)]
        public async Task CreateMenuAsync_SimpleRun_ChecksIfServiceRuns(
            string name,
            string description,
            double basePrice,
            bool isAvailable,
            bool shouldSucceed)
        {
            var storeId = Guid.NewGuid();
            var dto = new MenuItemAdminDTO
            {
                Name = name,
                Description = description,
                BasePrice = basePrice,
                IsAvailable = isAvailable,
                IsActive = true,
                StoreId = storeId
            };

            // Setup exist check
            if (!shouldSucceed && name == "Exist")
            {
                var existing = new List<MenuItem> { new MenuItem { Name = "Exist", IsDeleted = false } }
                    .AsQueryable().BuildMockDbSet().Object;
                _menuRepositoryMock.Setup(r => r.AsQueryable()).Returns(existing);
            }
            else
            {
                _menuRepositoryMock.Setup(r => r.AsQueryable())
                    .Returns(new List<MenuItem>().AsQueryable().BuildMockDbSet().Object);
            }

            _menuRepositoryMock.Setup(r => r.AddAsync(It.IsAny<MenuItem>())).Returns(Task.CompletedTask);
            _menuRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            if (shouldSucceed)
            {
                _mapperMock.Setup(m => m.Map<MenuItem>(It.IsAny<MenuItemAdminDTO>()))
                    .Returns((MenuItemAdminDTO d) => new MenuItem { Id = Guid.NewGuid(), Name = d.Name, StoreId = d.StoreId });
                _mapperMock.Setup(m => m.Map<MenuItemAdminDTO>(It.IsAny<MenuItem>()))
                    .Returns((MenuItem mi) => dto);
            }

            var ex = await Record.ExceptionAsync(() => _adminMenuItemService.CreateMenuAsync(dto, storeId.ToString()));
            if (shouldSucceed)
                Assert.Null(ex);
            else
                Assert.NotNull(ex);
        }
        #endregion

        #region GetAllMenuItem CM-21
        [Theory]
        [InlineData(1, 5, "name", "Chicken", "name", "desc", true)]
        [InlineData(1, 5, "description", "Chicken", "name", "desc", true)]
        [InlineData(1, 5, null, "Chicken", "name", "desc", true)]
        [InlineData(1, 5, "name", null, "name", "desc", true)]
        [InlineData(1, 5, "name", "Chicken", "base_price", "desc", true)]
        [InlineData(1, 5, "name", "Chicken", null, "desc", true)]
        [InlineData(1, 5, "name", "Chicken", "name", "asc", true)]
        [InlineData(1, 5, "name", "Chicken", "name", null, true)]
        public async Task GetAllMenuItemAsync_SimpleRun_ChecksIfServiceRuns(
            int page,
            int pageSize,
            string? searchBy,
            string? searchValue,
            string? sortBy,
            string? sortOrder,
            bool shouldSucceed)
        {
            var storeId = Guid.NewGuid();

            var menus = new List<MenuItem>
            {
                new MenuItem { Id = Guid.NewGuid(), StoreId = storeId, Name = "Burger", Description = "Tasty", BasePrice = 5.5, IsAvailable = true },
                new MenuItem { Id = Guid.NewGuid(), StoreId = storeId, Name = "Fries", Description = "Crispy", BasePrice = 2.0, IsAvailable = true }
            };

            _menuRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(menus.AsQueryable().BuildMockDbSet().Object);

            _mapperMock.Setup(m => m.Map<List<MenuItemAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns((List<MenuItem> src) => src.Select(m => new MenuItemAdminDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    BasePrice = m.BasePrice,
                    IsAvailable = m.IsAvailable,
                    IsActive = true,
                    StoreId = m.StoreId
                }).ToList());

            var query = new UrlQueryParameters
            {
                Page = page,
                PageSize = pageSize,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Filters = new Dictionary<string, string> { { "price", "12" } }
            };

            var ex = await Record.ExceptionAsync(() => _adminMenuItemService.GetAllMenuItemAsync(query, storeId));
            if (shouldSucceed)
                Assert.Null(ex);
            else
                Assert.NotNull(ex);
        }
        #endregion

        #region GetMenuItemDetail CM-22
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        //[InlineData(null, false)]
        public async Task GetMenuDetailAsync_SimpleRun_ChecksIfServiceRuns(string idStr, bool shouldSucceed)
        {
            var id = Guid.Parse(idStr);
            var storeId = Guid.NewGuid();

            if (shouldSucceed)
            {
                var item = new MenuItem { Id = id, StoreId = storeId, Name = "Burger", Description = "Tasty", BasePrice = 5.5 };
                _menuRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MenuItem, bool>>>()))
                    .ReturnsAsync(new List<MenuItem> { item });
                _mapperMock.Setup(m => m.Map<MenuItemAdminDTO>(It.IsAny<MenuItem>()))
                    .Returns((MenuItem m) => new MenuItemAdminDTO { Id = m.Id, Name = m.Name, Description = m.Description, BasePrice = m.BasePrice, IsAvailable = m.IsAvailable, IsActive = true, StoreId = m.StoreId });
            }
            else
            {
                _menuRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MenuItem, bool>>>()))
                    .ReturnsAsync(new List<MenuItem>());
            }

            var ex = await Record.ExceptionAsync(async () =>
            {
                var result = await _adminMenuItemService.GetMenuDetailAsync(id);
                if (shouldSucceed)
                    Assert.NotNull(result);
                else
                    Assert.Null(result);
            });

            Assert.Null(ex);
        }
        #endregion

        #region UpdateMenu CM-24
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "String Name", "String Description", 1, true, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "String Name", "String Description", 0, true, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, "String Description", 1, false, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "String Name", null, 1, true, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "String Name", "String Description", null, true, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "String Name", "String Description", 1, null, false)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", "String Name", "String Description", 1, true, false)]
        public async Task UpdateMenuAsync_SimpleRun_ChecksIfServiceRuns(
            string idStr,
            string name,
            string description,
            double basePrice,
            bool isAvailable,
            bool shouldSucceed)
        {
            var id = Guid.Parse(idStr);
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var dto = new MenuItemAdminDTO
            {
                Id = id,
                Name = name,
                Description = description,
                BasePrice = basePrice,
                IsAvailable = isAvailable,
                IsActive = true,
                StoreId = storeId
            };

            if (shouldSucceed)
            {
                var existing = new MenuItem { Id = id, StoreId = storeId, Name = "Old", IsDeleted = false, BasePrice = 5.0 };
                _menuRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
                _mapperMock.Setup(m => m.Map(It.IsAny<MenuItemAdminDTO>(), It.IsAny<MenuItem>()))
                    .Callback<MenuItemAdminDTO, MenuItem>((d, m) =>
                    {
                        m.Name = d.Name;
                        m.Description = d.Description;
                        m.BasePrice = d.BasePrice;
                        m.IsAvailable = d.IsAvailable;
                    });
                _menuRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            }
            else
            {
                _menuRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((MenuItem?)null);
            }

            var ex = await Record.ExceptionAsync(async () =>
            {
                var result = await _adminMenuItemService.UpdateMenuAsync(id, dto, userId);
                if (shouldSucceed)
                    Assert.True(result);
                else
                    Assert.False(result);
            });

            Assert.Null(ex);
        }
        #endregion

        #region DeleteMenu CM-25
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        public async Task DeleteMenuAsync_SimpleRun_ChecksIfServiceRuns(string idStr, bool shouldSucceed)
        {
            var id = Guid.Parse(idStr);
            var userId = Guid.NewGuid().ToString();

            if (shouldSucceed)
            {
                var existing = new MenuItem { Id = id, StoreId = Guid.NewGuid(), IsDeleted = false };
                _menuRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
                _menuRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            }
            else
            {
                _menuRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((MenuItem?)null);
            }

            var ex = await Record.ExceptionAsync(async () =>
            {
                var result = await _adminMenuItemService.DeleteMenuAsync(id, userId);
                if (shouldSucceed)
                    Assert.True(result);
                else
                    Assert.False(result);
            });

            Assert.Null(ex);
        }
        #endregion
    }
}
