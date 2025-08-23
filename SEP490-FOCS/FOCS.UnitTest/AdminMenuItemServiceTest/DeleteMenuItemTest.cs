using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.AdminMenuItemServiceTest
{
    public class DeleteMenuItemTest : AdminMenuItemServiceTestBase
    {
        [Fact]
        public async Task DeleteMenuAsync_ShouldReturnFalse_WhenItemNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user-id";

            _menuRepositoryMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((MenuItem?)null);

            // Act
            var result = await _adminMenuItemService.DeleteMenuAsync(id, userId);

            // Assert
            Assert.False(result);
            _menuRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteMenuAsync_ShouldReturnFalse_WhenItemAlreadyDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user-id";
            var item = CreateValidMenuItem(Guid.NewGuid());
            item.IsDeleted = true;

            _menuRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(item);

            // Act
            var result = await _adminMenuItemService.DeleteMenuAsync(id, userId);

            // Assert
            Assert.False(result);
            _menuRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteMenuAsync_ShouldReturnTrue_WhenItemDeletedSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "admin-user";
            var item = CreateValidMenuItem(Guid.NewGuid());
            item.Id = id;

            _menuRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(item);
            _menuRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminMenuItemService.DeleteMenuAsync(id, userId);

            // Assert
            Assert.True(result);
            Assert.True(item.IsDeleted);
            Assert.Equal(userId, item.UpdatedBy);
            Assert.NotNull(item.UpdatedAt);
            Assert.True((DateTime.UtcNow - item.UpdatedAt!.Value).TotalSeconds < 5);
            _menuRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteMenuAsync_ShouldSetAuditFieldsCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "auditor";
            var item = CreateValidMenuItem(Guid.NewGuid());
            item.Id = id;

            _menuRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(item);
            _menuRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminMenuItemService.DeleteMenuAsync(id, userId);

            // Assert
            Assert.True(result);
            Assert.Equal(userId, item.UpdatedBy);
            Assert.NotNull(item.UpdatedAt);
            Assert.True(item.IsDeleted);
        }

        [Fact]
        public async Task DeleteMenuAsync_ShouldCallSaveChanges_WhenItemIsValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user123";
            var item = CreateValidMenuItem(Guid.NewGuid());

            _menuRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(item);
            _menuRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminMenuItemService.DeleteMenuAsync(id, userId);

            // Assert
            Assert.True(result);
            _menuRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
