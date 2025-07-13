using AutoMapper;
using FOCS.Application.Services;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

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
    }
}