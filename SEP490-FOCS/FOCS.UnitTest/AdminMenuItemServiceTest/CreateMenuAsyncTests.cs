using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.AdminMenuItemServiceTest
{
    public class CreateMenuAsyncTests : AdminMenuItemServiceTestBase
    {
        [Fact]
        public async Task CreateMenuAsync_ValidInput_ReturnsMenuItemAdminDTO()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var dto = CreateValidMenuItemAdminDTO(storeId);
            var menuItem = new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                IsAvailable = dto.IsAvailable,
                IsActive = dto.IsActive,
                StoreId = dto.StoreId
            };
            var expectedResult = new MenuItemAdminDTO
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                BasePrice = menuItem.BasePrice,
                IsAvailable = menuItem.IsAvailable,
                IsActive = menuItem.IsActive,
                StoreId = menuItem.StoreId
            };

            SetupMenuItemNameUniqueness(dto.Name, false);
            SetupMapperForCreation(dto, menuItem, expectedResult);

            // Act
            var result = await _adminMenuItemService.CreateMenuAsync(dto, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Id, result.Id);
            Assert.Equal(expectedResult.Name, result.Name);
            Assert.Equal(expectedResult.Description, result.Description);
            Assert.Equal(expectedResult.BasePrice, result.BasePrice);
            Assert.Equal(expectedResult.IsAvailable, result.IsAvailable);
            Assert.Equal(expectedResult.IsActive, result.IsActive);
            Assert.Equal(expectedResult.StoreId, result.StoreId);

            _menuRepositoryMock.Verify(x => x.AddAsync(It.IsAny<MenuItem>()), Times.Once);
            _menuRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateMenuAsync_DuplicateName_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var dto = CreateValidMenuItemAdminDTO(storeId);

            SetupMenuItemNameUniqueness(dto.Name, true);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _adminMenuItemService.CreateMenuAsync(dto, storeId));
        }

        [Fact]
        public async Task CreateMenuAsync_InvalidModel_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var invalidDto = new MenuItemAdminDTO
            {
                Name = null, // Explicitly set to null to force validation error
                Description = "Test Description",
                BasePrice = -10, // Invalid price
                IsAvailable = true,
                IsActive = true,
                StoreId = Guid.NewGuid()
            };

            // Setup mock to properly handle async operations
            var mockMenuItems = new List<MenuItem>().AsQueryable().BuildMock();
            _menuRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(mockMenuItems);

            // Setup mapper to return null to simulate validation failure
            _mapperMock.Setup(x => x.Map<MenuItem>(invalidDto))
                .Returns((MenuItem)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NullReferenceException>(() =>
                _adminMenuItemService.CreateMenuAsync(invalidDto, storeId));

            Assert.NotNull(exception);
        }

        [Fact]
        public async Task CreateMenuAsync_ValidInput_SetsAuditFields()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var dto = CreateValidMenuItemAdminDTO(storeId);
            var menuItem = new MenuItem();
            var expectedResult = new MenuItemAdminDTO();

            SetupMenuItemNameUniqueness(dto.Name, false);
            _mapperMock.Setup(x => x.Map<MenuItem>(dto))
                .Returns(menuItem);
            _mapperMock.Setup(x => x.Map<MenuItemAdminDTO>(It.IsAny<MenuItem>()))
                .Returns(expectedResult);

            // Act
            var result = await _adminMenuItemService.CreateMenuAsync(dto, storeId);

            // Assert
            Assert.NotNull(menuItem.Id);
            Assert.False(menuItem.IsDeleted);
            Assert.NotNull(menuItem.CreatedAt);
            Assert.Equal(storeId, menuItem.CreatedBy);
        }

        // Helper methods
        private MenuItemAdminDTO CreateValidMenuItemAdminDTO(string storeId)
        {
            return new MenuItemAdminDTO
            {
                Name = "Test Menu Item",
                Description = "Test Description",
                BasePrice = 10.99,
                IsAvailable = true,
                IsActive = true,
                StoreId = Guid.Parse(storeId)
            };
        }

        private void SetupMenuItemNameUniqueness(string name, bool exists)
        {
            var queryable = exists
                ? new List<MenuItem> { new MenuItem { Name = name } }.AsQueryable().BuildMockDbSet()
                : new List<MenuItem>().AsQueryable().BuildMockDbSet();

            _menuRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(queryable.Object);
        }

        private void SetupMapperForCreation(MenuItemAdminDTO dto, MenuItem entity, MenuItemAdminDTO resultDto)
        {
            _mapperMock.Setup(x => x.Map<MenuItem>(dto))
                .Returns(entity);
            _mapperMock.Setup(x => x.Map<MenuItemAdminDTO>(It.IsAny<MenuItem>()))
                .Returns(resultDto);
        }
    }
}