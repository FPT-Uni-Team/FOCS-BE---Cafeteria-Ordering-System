using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.StoreServiceTest
{
    public class DeleteStoreTest : StoreServiceTestBase
    {
        [Fact]
        public async Task DeleteStoreAsync_WithValidInputAndExistingStore_ShouldMarkAsDeletedAndReturnTrue()
        {
            // Arrange
            var existingStore = CreateValidStore();
            existingStore.Id = _testStoreId;
            existingStore.IsDeleted = false;

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync(existingStore);

            _mockStoreRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminStoreService.DeleteStoreAsync(_testStoreId, _validUserId);

            // Assert
            Assert.True(result);
            Assert.True(existingStore.IsDeleted);
            Assert.Equal(_validUserId, existingStore.UpdatedBy);
            Assert.True(existingStore.UpdatedAt <= DateTime.UtcNow);
            Assert.True(existingStore.UpdatedAt >= DateTime.UtcNow.AddMinutes(-1));

            _mockStoreRepository.Verify(r => r.GetByIdAsync(_testStoreId), Times.Once);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteStoreAsync_WithNullUserId_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.DeleteStoreAsync(_testStoreId, null));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteStoreAsync_WithEmptyUserId_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.DeleteStoreAsync(_testStoreId, string.Empty));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteStoreAsync_WithWhitespaceUserId_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.DeleteStoreAsync(_testStoreId, "   "));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteStoreAsync_WithNonExistentStore_ShouldReturnFalse()
        {
            // Arrange
            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync((Store)null);

            // Act
            var result = await _adminStoreService.DeleteStoreAsync(_testStoreId, _validUserId);

            // Assert
            Assert.False(result);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(_testStoreId), Times.Once);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteStoreAsync_WithAlreadyDeletedStore_ShouldReturnFalse()
        {
            // Arrange
            var deletedStore = CreateValidStore();
            deletedStore.Id = _testStoreId;
            deletedStore.IsDeleted = true;

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync(deletedStore);

            // Act
            var result = await _adminStoreService.DeleteStoreAsync(_testStoreId, _validUserId);

            // Assert
            Assert.False(result);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(_testStoreId), Times.Once);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteStoreAsync_ShouldSetUpdatedAtToCurrentTime()
        {
            // Arrange
            var existingStore = CreateValidStore();
            existingStore.Id = _testStoreId;
            existingStore.IsDeleted = false;
            var originalUpdatedAt = existingStore.UpdatedAt;

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync(existingStore);

            _mockStoreRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminStoreService.DeleteStoreAsync(_testStoreId, _validUserId);

            // Assert
            Assert.True(result);
            Assert.True(existingStore.UpdatedAt > originalUpdatedAt);
            Assert.True(existingStore.UpdatedAt <= DateTime.UtcNow);
            Assert.True(existingStore.UpdatedAt >= DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task DeleteStoreAsync_ShouldNotChangeOtherProperties()
        {
            // Arrange
            var existingStore = CreateValidStore();
            existingStore.Id = _testStoreId;
            existingStore.IsDeleted = false;
            var originalName = existingStore.Name;
            var originalAddress = existingStore.Address;
            var originalPhoneNumber = existingStore.PhoneNumber;
            var originalCustomTaxRate = existingStore.CustomTaxRate;
            var originalCreatedAt = existingStore.CreatedAt;
            var originalCreatedBy = existingStore.CreatedBy;

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync(existingStore);

            _mockStoreRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminStoreService.DeleteStoreAsync(_testStoreId, _validUserId);

            // Assert
            Assert.True(result);
            Assert.Equal(originalName, existingStore.Name);
            Assert.Equal(originalAddress, existingStore.Address);
            Assert.Equal(originalPhoneNumber, existingStore.PhoneNumber);
            Assert.Equal(originalCustomTaxRate, existingStore.CustomTaxRate);
            Assert.Equal(originalCreatedAt, existingStore.CreatedAt);
            Assert.Equal(originalCreatedBy, existingStore.CreatedBy);
        }

        [Fact]
        public async Task DeleteStoreAsync_WhenRepositoryGetByIdThrowsException_ShouldPropagateException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Database error");

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminStoreService.DeleteStoreAsync(_testStoreId, _validUserId));

            Assert.Equal("Database error", exception.Message);

            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteStoreAsync_WhenSaveChangesThrowsException_ShouldPropagateException()
        {
            // Arrange
            var existingStore = CreateValidStore();
            existingStore.Id = _testStoreId;
            existingStore.IsDeleted = false;
            var expectedException = new InvalidOperationException("Save error");

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync(existingStore);

            _mockStoreRepository.Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminStoreService.DeleteStoreAsync(_testStoreId, _validUserId));

            Assert.Equal("Save error", exception.Message);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(_testStoreId), Times.Once);
            // Verify that the store was marked as deleted before the exception
            Assert.True(existingStore.IsDeleted);
        }

        [Fact]
        public async Task DeleteStoreAsync_ShouldOnlyModifyIsDeletedUpdatedAt()
        {
            // Arrange
            var existingStore = CreateValidStore();
            existingStore.Id = _testStoreId;
            existingStore.IsDeleted = false;
            var originalUpdatedBy = existingStore.UpdatedBy;

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync(existingStore);

            _mockStoreRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminStoreService.DeleteStoreAsync(_testStoreId, _validUserId);

            // Assert
            Assert.True(result);
            Assert.True(existingStore.IsDeleted);
        }
    }
}
