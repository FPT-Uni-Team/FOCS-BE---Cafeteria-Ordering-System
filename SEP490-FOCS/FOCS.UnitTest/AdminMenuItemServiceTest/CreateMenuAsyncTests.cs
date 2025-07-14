using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.AdminMenuItemServiceTest
{
    public class CreateMenuAsyncTests : AdminMenuItemServiceTestBase
    {
        [Fact]
        public async Task CreateMenuAsync_ShouldReturnDto_WhenInputIsValid()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var dto = CreateValidMenuItemAdminDTO(storeId);

            var createdEntity = new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                IsAvailable = dto.IsAvailable,
                IsActive = dto.IsActive,
                StoreId = dto.StoreId
            };

            var expectedDto = new MenuItemAdminDTO
            {
                Id = createdEntity.Id,
                Name = createdEntity.Name,
                Description = createdEntity.Description,
                BasePrice = createdEntity.BasePrice,
                IsAvailable = createdEntity.IsAvailable,
                IsActive = createdEntity.IsActive,
                StoreId = createdEntity.StoreId
            };

            SetupMenuItemNameUniqueness(dto.Name, false);
            SetupMapperForCreation(dto, createdEntity, expectedDto);

            // Act
            var result = await _adminMenuItemService.CreateMenuAsync(dto, storeId.ToString());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
            Assert.Equal(expectedDto.Description, result.Description);
            Assert.Equal(expectedDto.BasePrice, result.BasePrice);
            Assert.Equal(expectedDto.IsAvailable, result.IsAvailable);
            Assert.Equal(expectedDto.IsActive, result.IsActive);
            Assert.Equal(expectedDto.StoreId, result.StoreId);

            _menuRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MenuItem>()), Times.Once);
            _menuRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateMenuAsync_ShouldThrowException_WhenNameAlreadyExists()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var dto = CreateValidMenuItemAdminDTO(storeId);

            SetupMenuItemNameUniqueness(dto.Name, true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _adminMenuItemService.CreateMenuAsync(dto, storeId.ToString()));

            Assert.Contains("name", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateMenuAsync_ShouldThrow_WhenMapperReturnsNull()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var dto = CreateValidMenuItemAdminDTO(storeId);
            SetupMenuItemNameUniqueness(dto.Name, false);

            _mapperMock.Setup(m => m.Map<MenuItem>(dto)).Returns((MenuItem)null);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() =>
                _adminMenuItemService.CreateMenuAsync(dto, storeId.ToString()));
        }

        [Fact]
        public async Task CreateMenuAsync_ShouldSetAuditFieldsCorrectly()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var dto = CreateValidMenuItemAdminDTO(storeId);

            var menuItem = new MenuItem();
            var expectedDto = new MenuItemAdminDTO();

            SetupMenuItemNameUniqueness(dto.Name, false);
            _mapperMock.Setup(m => m.Map<MenuItem>(dto)).Returns(menuItem);
            _mapperMock.Setup(m => m.Map<MenuItemAdminDTO>(It.IsAny<MenuItem>())).Returns(expectedDto);

            // Act
            await _adminMenuItemService.CreateMenuAsync(dto, storeId.ToString());

            // Assert
            Assert.NotEqual(Guid.Empty, menuItem.Id);
            Assert.False(menuItem.IsDeleted);
            Assert.NotNull(menuItem.CreatedAt);
            Assert.Equal(storeId.ToString(), menuItem.CreatedBy);
        }

    }
}
