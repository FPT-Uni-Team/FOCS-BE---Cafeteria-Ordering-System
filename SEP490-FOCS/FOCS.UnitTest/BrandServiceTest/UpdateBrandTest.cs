using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.BrandServiceTest
{
    public class UpdateBrandTest : BrandServiceTestBase
    {
        [Fact]
        public async Task UpdateBrandAsync_WithValidInput_ShouldUpdateBrandSuccessfully()
        {
            // Arrange
            var brandDto = CreateValidBrandDTO();
            var existingBrand = CreateValidBrand();

            _mockBrandRepository.Setup(r => r.GetByIdAsync(_validBrandId))
                .ReturnsAsync(existingBrand);
            _mockBrandRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map(brandDto, existingBrand));

            // Act
            var result = await _adminBrandService.UpdateBrandAsync(_validBrandId, brandDto, _validUserId);

            // Assert
            Assert.True(result);

            // Verify CheckValidInput was called
            // Verify repository operations
            _mockBrandRepository.Verify(r => r.GetByIdAsync(_validBrandId), Times.Once);
            _mockBrandRepository.Verify(r => r.SaveChangesAsync(), Times.Once);

            // Verify mapper was called
            _mockMapper.Verify(m => m.Map(brandDto, existingBrand), Times.Once);

            // Verify UpdatedAt and UpdatedBy were set
            Assert.Equal(_validUserId, existingBrand.UpdatedBy);
            Assert.True(existingBrand.UpdatedAt.HasValue);
            Assert.True(existingBrand.UpdatedAt.Value > DateTime.UtcNow.AddMinutes(-1));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateBrandAsync_WithInvalidUserId_ShouldThrowArgumentException(string invalidUserId)
        {
            // Arrange
            var brandDto = CreateValidBrandDTO();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminBrandService.UpdateBrandAsync(_validBrandId, brandDto, invalidUserId));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            // Verify no repository operations were called
            _mockBrandRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockBrandRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateBrandAsync_WithNullBrand_ShouldReturnFalse()
        {
            // Arrange
            var brandDto = CreateValidBrandDTO();

            _mockBrandRepository.Setup(r => r.GetByIdAsync(_validBrandId))
                .ReturnsAsync((Brand)null);

            // Act
            var result = await _adminBrandService.UpdateBrandAsync(_validBrandId, brandDto, _validUserId);

            // Assert
            Assert.False(result);

            _mockBrandRepository.Verify(r => r.GetByIdAsync(_validBrandId), Times.Once);
            _mockBrandRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockMapper.Verify(m => m.Map(It.IsAny<BrandAdminDTO>(), It.IsAny<Brand>()), Times.Never);
        }

        [Fact]
        public async Task UpdateBrandAsync_WithDeletedBrand_ShouldReturnFalse()
        {
            // Arrange
            var brandDto = CreateValidBrandDTO();
            var deletedBrand = CreateValidBrand(true); // isDeleted = true

            _mockBrandRepository.Setup(r => r.GetByIdAsync(_validBrandId))
                .ReturnsAsync(deletedBrand);

            // Act
            var result = await _adminBrandService.UpdateBrandAsync(_validBrandId, brandDto, _validUserId);

            // Assert
            Assert.False(result);

            _mockBrandRepository.Verify(r => r.GetByIdAsync(_validBrandId), Times.Once);
            _mockBrandRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockMapper.Verify(m => m.Map(It.IsAny<BrandAdminDTO>(), It.IsAny<Brand>()), Times.Never);
        }

        [Fact]
        public async Task UpdateBrandAsync_ShouldSetUpdatedAtToCurrentTime()
        {
            // Arrange
            var brandDto = CreateValidBrandDTO();
            var existingBrand = CreateValidBrand();
            var beforeUpdate = DateTime.UtcNow;

            _mockBrandRepository.Setup(r => r.GetByIdAsync(_validBrandId))
                .ReturnsAsync(existingBrand);
            _mockBrandRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminBrandService.UpdateBrandAsync(_validBrandId, brandDto, _validUserId);

            // Assert
            Assert.True(result);
            Assert.True(existingBrand.UpdatedAt.HasValue);
            Assert.True(existingBrand.UpdatedAt.Value >= beforeUpdate);
            Assert.True(existingBrand.UpdatedAt.Value <= DateTime.UtcNow);
        }
    }
}
