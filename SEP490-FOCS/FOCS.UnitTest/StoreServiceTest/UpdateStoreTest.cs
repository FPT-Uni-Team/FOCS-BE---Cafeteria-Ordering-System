using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.StoreServiceTest
{
    public class UpdateStoreTest : StoreServiceTestBase
    {
        [Fact]
        public async Task UpdateStoreAsync_WithValidInputAndExistingStore_ShouldUpdateStoreAndReturnTrue()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var existingStore = CreateValidStore();
            existingStore.Id = _testStoreId;
            existingStore.IsDeleted = false;

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync(existingStore);

            _mockMapper.Setup(m => m.Map(dto, existingStore))
                .Callback<StoreAdminDTO, Store>((src, dest) =>
                {
                    dest.Name = src.Name;
                    dest.Address = src.Address;
                    dest.PhoneNumber = src.PhoneNumber;
                    dest.CustomTaxRate = src.CustomTaxRate;
                });

            _mockStoreRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminStoreService.UpdateStoreAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.True(result);
            Assert.Equal(_validUserId, existingStore.UpdatedBy);
            Assert.True(existingStore.UpdatedAt <= DateTime.UtcNow);
            Assert.True(existingStore.UpdatedAt >= DateTime.UtcNow.AddMinutes(-1));

            _mockStoreRepository.Verify(r => r.GetByIdAsync(_testStoreId), Times.Once);
            _mockMapper.Verify(m => m.Map(dto, existingStore), Times.Once);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateStoreAsync_WithNullUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.UpdateStoreAsync(_testStoreId, dto, null));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStoreAsync_WithEmptyUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.UpdateStoreAsync(_testStoreId, dto, string.Empty));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStoreAsync_WithWhitespaceUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.UpdateStoreAsync(_testStoreId, dto, "   "));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStoreAsync_WithNonExistentStore_ShouldReturnFalse()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync((Store)null);

            // Act
            var result = await _adminStoreService.UpdateStoreAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.False(result);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(_testStoreId), Times.Once);
            _mockMapper.Verify(m => m.Map(It.IsAny<StoreAdminDTO>(), It.IsAny<Store>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStoreAsync_WithDeletedStore_ShouldReturnFalse()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var deletedStore = CreateValidStore();
            deletedStore.Id = _testStoreId;
            deletedStore.IsDeleted = true;

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync(deletedStore);

            // Act
            var result = await _adminStoreService.UpdateStoreAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.False(result);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(_testStoreId), Times.Once);
            _mockMapper.Verify(m => m.Map(It.IsAny<StoreAdminDTO>(), It.IsAny<Store>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStoreAsync_ShouldSetUpdatedAtToCurrentTime()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var existingStore = CreateValidStore();
            existingStore.Id = _testStoreId;
            existingStore.IsDeleted = false;
            var originalUpdatedAt = existingStore.UpdatedAt;

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync(existingStore);

            _mockMapper.Setup(m => m.Map(dto, existingStore));
            _mockStoreRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminStoreService.UpdateStoreAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.True(result);
            Assert.True(existingStore.UpdatedAt > originalUpdatedAt);
            Assert.True(existingStore.UpdatedAt <= DateTime.UtcNow);
            Assert.True(existingStore.UpdatedAt >= DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task UpdateStoreAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var expectedException = new InvalidOperationException("Database error");

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminStoreService.UpdateStoreAsync(_testStoreId, dto, _validUserId));

            Assert.Equal("Database error", exception.Message);

            _mockMapper.Verify(m => m.Map(It.IsAny<StoreAdminDTO>(), It.IsAny<Store>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStoreAsync_WhenSaveChangesThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var existingStore = CreateValidStore();
            existingStore.Id = _testStoreId;
            existingStore.IsDeleted = false;
            var expectedException = new InvalidOperationException("Save error");

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync(existingStore);

            _mockMapper.Setup(m => m.Map(dto, existingStore));

            _mockStoreRepository.Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminStoreService.UpdateStoreAsync(_testStoreId, dto, _validUserId));

            Assert.Equal("Save error", exception.Message);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(_testStoreId), Times.Once);
            _mockMapper.Verify(m => m.Map(dto, existingStore), Times.Once);
        }

        [Fact]
        public async Task UpdateStoreAsync_WhenMapperThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var existingStore = CreateValidStore();
            existingStore.Id = _testStoreId;
            existingStore.IsDeleted = false;
            var expectedException = new AutoMapperMappingException("Mapping error");

            _mockStoreRepository.Setup(r => r.GetByIdAsync(_testStoreId))
                .ReturnsAsync(existingStore);

            _mockMapper.Setup(m => m.Map(dto, existingStore))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _adminStoreService.UpdateStoreAsync(_testStoreId, dto, _validUserId));

            Assert.Equal("Mapping error", exception.Message);

            _mockStoreRepository.Verify(r => r.GetByIdAsync(_testStoreId), Times.Once);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
