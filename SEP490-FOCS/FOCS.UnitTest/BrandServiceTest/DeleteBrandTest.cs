using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.BrandServiceTest
{
    public class DeleteBrandTest : BrandServiceTestBase
    {
        [Fact]
        public async Task DeleteBrandAsync_WithValidInput_ShouldDeleteBrandSuccessfully()
        {
            // Arrange
            var existingBrand = CreateValidBrand();

            _mockBrandRepository.Setup(r => r.GetByIdAsync(_validBrandId))
                .ReturnsAsync(existingBrand);
            _mockBrandRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminBrandService.DeleteBrandAsync(_validBrandId, _validUserId);

            // Assert
            Assert.True(result);

            // Verify CheckValidInput was called
            // Verify repository operations
            _mockBrandRepository.Verify(r => r.GetByIdAsync(_validBrandId), Times.Once);
            _mockBrandRepository.Verify(r => r.SaveChangesAsync(), Times.Once);

            // Verify soft delete properties were set
            Assert.True(existingBrand.IsDelete);
            Assert.Equal(_validUserId, existingBrand.UpdatedBy);
            Assert.True(existingBrand.UpdatedAt.HasValue);
            Assert.True(existingBrand.UpdatedAt.Value > DateTime.UtcNow.AddMinutes(-1));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteBrandAsync_WithInvalidUserId_ShouldThrowArgumentException(string invalidUserId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminBrandService.DeleteBrandAsync(_validBrandId, invalidUserId));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            // Verify no repository operations were called
            _mockBrandRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockBrandRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteBrandAsync_WithNullBrand_ShouldReturnFalse()
        {
            // Arrange
            _mockBrandRepository.Setup(r => r.GetByIdAsync(_validBrandId))
                .ReturnsAsync((Brand)null);

            // Act
            var result = await _adminBrandService.DeleteBrandAsync(_validBrandId, _validUserId);

            // Assert
            Assert.False(result);

            _mockBrandRepository.Verify(r => r.GetByIdAsync(_validBrandId), Times.Once);
            _mockBrandRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteBrandAsync_WithAlreadyDeletedBrand_ShouldReturnFalse()
        {
            // Arrange
            var deletedBrand = CreateValidBrand(true); // isDeleted = true

            _mockBrandRepository.Setup(r => r.GetByIdAsync(_validBrandId))
                .ReturnsAsync(deletedBrand);

            // Act
            var result = await _adminBrandService.DeleteBrandAsync(_validBrandId, _validUserId);

            // Assert
            Assert.False(result);

            _mockBrandRepository.Verify(r => r.GetByIdAsync(_validBrandId), Times.Once);
            _mockBrandRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteBrandAsync_ShouldSetIsDeleteToTrue()
        {
            // Arrange
            var existingBrand = CreateValidBrand(false); // Not deleted initially

            _mockBrandRepository.Setup(r => r.GetByIdAsync(_validBrandId))
                .ReturnsAsync(existingBrand);
            _mockBrandRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminBrandService.DeleteBrandAsync(_validBrandId, _validUserId);

            // Assert
            Assert.True(result);
            Assert.True(existingBrand.IsDelete);
        }

        [Fact]
        public async Task DeleteBrandAsync_ShouldSetUpdatedAtToCurrentTime()
        {
            // Arrange
            var existingBrand = CreateValidBrand();
            var beforeDelete = DateTime.UtcNow;

            _mockBrandRepository.Setup(r => r.GetByIdAsync(_validBrandId))
                .ReturnsAsync(existingBrand);
            _mockBrandRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminBrandService.DeleteBrandAsync(_validBrandId, _validUserId);

            // Assert
            Assert.True(result);
            Assert.True(existingBrand.UpdatedAt.HasValue);
            Assert.True(existingBrand.UpdatedAt.Value >= beforeDelete);
            Assert.True(existingBrand.UpdatedAt.Value <= DateTime.UtcNow);
        }

        [Fact]
        public async Task DeleteBrandAsync_ShouldSetUpdatedByToUserId()
        {
            // Arrange
            var existingBrand = CreateValidBrand();

            _mockBrandRepository.Setup(r => r.GetByIdAsync(_validBrandId))
                .ReturnsAsync(existingBrand);
            _mockBrandRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminBrandService.DeleteBrandAsync(_validBrandId, _validUserId);

            // Assert
            Assert.True(result);
            Assert.Equal(_validUserId, existingBrand.UpdatedBy);
        }
    }
}