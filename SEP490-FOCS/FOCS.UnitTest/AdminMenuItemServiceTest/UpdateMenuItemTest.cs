using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.AdminMenuItemServiceTest
{
    public class UpdateMenuItemTest : AdminMenuItemServiceTestBase
    {
        [Fact]
        public async Task UpdateMenuAsync_ShouldReturnTrue_WhenItemExistsAndIsUpdated()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var item = CreateValidMenuItem(Guid.NewGuid());

            _menuRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(item);
            _menuRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            var dto = CreateValidMenuItemAdminDTO(item.StoreId);

            // Act
            var result = await _adminMenuItemService.UpdateMenuAsync(id, dto, userId);

            // Assert
            Assert.True(result);
            Assert.Equal(userId, item.UpdatedBy);
            Assert.True((DateTime.UtcNow - item.UpdatedAt!.Value).TotalSeconds < 5);
            _menuRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateMenuAsync_ShouldReturnFalse_WhenItemDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new MenuItemAdminDTO();
            var userId = "some-user";

            _menuRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((MenuItem?)null);

            // Act
            var result = await _adminMenuItemService.UpdateMenuAsync(id, dto, userId);

            // Assert
            Assert.False(result);
            _menuRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateMenuAsync_ShouldReturnFalse_WhenItemIsDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var item = CreateValidMenuItem(Guid.NewGuid());
            item.IsDeleted = true;

            _menuRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(item);

            var dto = new MenuItemAdminDTO();
            var userId = "user";

            // Act
            var result = await _adminMenuItemService.UpdateMenuAsync(id, dto, userId);

            // Assert
            Assert.False(result);
            _menuRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateMenuAsync_ShouldMapDtoToEntity()
        {
            // Arrange
            var id = Guid.NewGuid();
            var item = CreateValidMenuItem(Guid.NewGuid());
            var dto = new MenuItemAdminDTO { Name = "Updated Name" };
            var userId = "mapper-user";

            _menuRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(item);

            _mapperMock.Setup(m => m.Map(dto, item)).Callback<MenuItemAdminDTO, MenuItem>((src, dest) =>
            {
                dest.Name = src.Name;
            });

            // Act
            var result = await _adminMenuItemService.UpdateMenuAsync(id, dto, userId);

            // Assert
            Assert.True(result);
            Assert.Equal("Updated Name", item.Name);
            _mapperMock.Verify(m => m.Map(dto, item), Times.Once);
        }

        [Fact]
        public async Task UpdateMenuAsync_ShouldCallMapperWithCorrectArguments()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var item = CreateValidMenuItem(Guid.NewGuid());
            item.Id = id;

            var dto = CreateValidMenuItemAdminDTO(item.StoreId);

            _menuRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(item);
            _menuRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _adminMenuItemService.UpdateMenuAsync(id, dto, userId);

            // Assert
            _mapperMock.Verify(m => m.Map(dto, item), Times.Once);
        }

        [Fact]
        public async Task UpdateMenuAsync_ShouldSetAuditFields_WhenItemIsUpdated()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var item = CreateValidMenuItem(Guid.NewGuid());
            item.Id = id;

            var dto = CreateValidMenuItemAdminDTO(item.StoreId);

            _menuRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(item);
            _menuRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminMenuItemService.UpdateMenuAsync(id, dto, userId);

            // Assert
            Assert.True(result);
            Assert.Equal(userId, item.UpdatedBy);
            Assert.NotNull(item.UpdatedAt);
            Assert.True((DateTime.UtcNow - item.UpdatedAt!.Value).TotalSeconds < 5);
        }

    }
}
