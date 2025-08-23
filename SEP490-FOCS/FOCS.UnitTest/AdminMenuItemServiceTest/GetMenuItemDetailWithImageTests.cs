using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.UnitTest.AdminMenuItemServiceTest
{
    public class GetMenuItemDetailWithImageTests : AdminMenuItemServiceTestBase
    {
        [Fact]
        public async Task GetMenuItemDetail_ShouldReturnDto_WhenItemExists()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var menuItemId = Guid.NewGuid();

            var menuItem = CreateValidMenuItem(Guid.Parse(storeId));
            menuItem.Id = menuItemId;
            menuItem.Images = new List<MenuItemImage>
            {
                new MenuItemImage { Url = "http://image1.jpg", IsMain = true },
                new MenuItemImage { Url = "http://image2.jpg", IsMain = false }
            };

            var menuItems = new List<MenuItem> { menuItem }.AsQueryable().BuildMockDbSet();

            _menuRepositoryMock.Setup(x => x.AsQueryable()).Returns(menuItems.Object);

            var expectedDto = new MenuItemDetailAdminDTO
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                BasePrice = menuItem.BasePrice,
                IsAvailable = menuItem.IsAvailable,
                Images = menuItem.Images.Select(i => new UploadedImageResult
                {
                    IsMain = i.IsMain,
                    Url = i.Url
                }).ToList()
            };

            _mapperMock.Setup(m => m.Map<MenuItemDetailAdminDTO>(It.IsAny<MenuItem>()))
                .Returns(expectedDto);

            // Act
            var result = await _adminMenuItemService.GetMenuItemDetail(menuItemId, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(menuItemId, result.Id);
            Assert.Equal(menuItem.Images.Count, result.Images.Count);
        }

        [Fact]
        public async Task GetMenuItemDetail_ShouldReturnNull_WhenItemNotFound()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var menuItemId = Guid.NewGuid();

            var menuItems = new List<MenuItem>().AsQueryable().BuildMockDbSet();

            _menuRepositoryMock.Setup(x => x.AsQueryable()).Returns(menuItems.Object);

            _mapperMock.Setup(m => m.Map<MenuItemDetailAdminDTO>(null)).Returns((MenuItemDetailAdminDTO)null!);

            // Act
            var result = await _adminMenuItemService.GetMenuItemDetail(menuItemId, storeId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMenuItemDetail_ShouldMapCorrectly_WhenItemExists()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var menuItemId = Guid.NewGuid();

            var menuItem = CreateValidMenuItem(Guid.Parse(storeId));
            menuItem.Id = menuItemId;

            var dbSet = new List<MenuItem> { menuItem }.AsQueryable().BuildMockDbSet();

            _menuRepositoryMock.Setup(x => x.AsQueryable()).Returns(dbSet.Object);

            _mapperMock.Setup(m => m.Map<MenuItemDetailAdminDTO>(menuItem))
                .Returns(new MenuItemDetailAdminDTO { Id = menuItem.Id });

            // Act
            var result = await _adminMenuItemService.GetMenuItemDetail(menuItemId, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(menuItemId, result.Id);
            _mapperMock.Verify(m => m.Map<MenuItemDetailAdminDTO>(menuItem), Times.Once);
        }

        [Fact]
        public async Task GetMenuItemDetail_ShouldMatchStoreIdAndMenuItemId()
        {
            // Arrange
            var correctStoreId = Guid.NewGuid();
            var incorrectStoreId = Guid.NewGuid();
            var menuItemId = Guid.NewGuid();

            var item = CreateValidMenuItem(incorrectStoreId);
            item.Id = menuItemId;

            var dbSet = new List<MenuItem> { item }.AsQueryable().BuildMockDbSet();
            _menuRepositoryMock.Setup(r => r.AsQueryable()).Returns(dbSet.Object);

            _mapperMock.Setup(m => m.Map<MenuItemDetailAdminDTO>(It.IsAny<MenuItem>()))
                .Returns((MenuItemDetailAdminDTO)null!);

            // Act
            var result = await _adminMenuItemService.GetMenuItemDetail(menuItemId, correctStoreId.ToString());

            // Assert
            Assert.Null(result);
        }
    }
}
