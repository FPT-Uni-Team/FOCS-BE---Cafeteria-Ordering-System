using AutoMapper;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.StoreSettingServiceTest
{
    public class UpdateStoreSettingTest : StoreSettingServiceTestBase
    {
        [Fact]
        public async Task UpdateStoreSettingAsync_WithValidInputAndExistingSetting_ShouldUpdateAndReturnTrue()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var existingSetting = CreateValidStoreSetting();

            SetupQueryableRepository(new List<StoreSetting> { existingSetting });

            _mockMapper.Setup(m => m.Map(dto, existingSetting))
                .Callback<StoreSettingDTO, StoreSetting>((src, dest) =>
                {
                    dest.OpenTime = src.OpenTime;
                    dest.CloseTime = src.CloseTime;
                    dest.Currency = src.Currency;
                    dest.PaymentConfig = src.PaymentConfig;
                    dest.LogoUrl = src.LogoUrl;
                    dest.IsSelfService = src.IsSelfService;
                    dest.discountStrategy = src.DiscountStrategy;
                });

            _mockStoreSettingRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _storeSettingService.UpdateStoreSettingAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.True(result);
            Assert.Equal(_validUserId, existingSetting.UpdatedBy);
            Assert.True(existingSetting.UpdatedAt <= DateTime.UtcNow);
            Assert.True(existingSetting.UpdatedAt >= DateTime.UtcNow.AddMinutes(-1));

            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map(dto, existingSetting), Times.Once);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateStoreSettingAsync_WithNonExistentSetting_ShouldReturnFalse()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();

            SetupQueryableRepository(new List<StoreSetting>());

            // Act
            var result = await _storeSettingService.UpdateStoreSettingAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.False(result);

            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map(It.IsAny<StoreSettingDTO>(), It.IsAny<StoreSetting>()), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStoreSettingAsync_WithDeletedSetting_ShouldReturnFalse()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var deletedSetting = CreateValidStoreSetting();
            deletedSetting.IsDeleted = true;

            SetupQueryableRepository(new List<StoreSetting> { deletedSetting });

            // Act
            var result = await _storeSettingService.UpdateStoreSettingAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.False(result);

            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map(It.IsAny<StoreSettingDTO>(), It.IsAny<StoreSetting>()), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStoreSettingAsync_WithDifferentStoreId_ShouldReturnFalse()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var existingSetting = CreateValidStoreSetting();
            existingSetting.StoreId = Guid.NewGuid(); // Different store ID

            SetupQueryableRepository(new List<StoreSetting> { existingSetting });

            // Act
            var result = await _storeSettingService.UpdateStoreSettingAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.False(result);

            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map(It.IsAny<StoreSettingDTO>(), It.IsAny<StoreSetting>()), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStoreSettingAsync_ShouldSetUpdatedAtToCurrentTime()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var existingSetting = CreateValidStoreSetting();
            var originalUpdatedAt = existingSetting.UpdatedAt;

            SetupQueryableRepository(new List<StoreSetting> { existingSetting });

            _mockMapper.Setup(m => m.Map(dto, existingSetting));
            _mockStoreSettingRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _storeSettingService.UpdateStoreSettingAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.True(result);
            Assert.True(existingSetting.UpdatedAt > originalUpdatedAt);
            Assert.True(existingSetting.UpdatedAt <= DateTime.UtcNow);
            Assert.True(existingSetting.UpdatedAt >= DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task UpdateStoreSettingAsync_WhenRepositoryQueryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var expectedException = new InvalidOperationException("Database query error");

            _mockStoreSettingRepository.Setup(r => r.AsQueryable())
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _storeSettingService.UpdateStoreSettingAsync(_testStoreId, dto, _validUserId));

            Assert.Equal("Database query error", exception.Message);
            _mockMapper.Verify(m => m.Map(It.IsAny<StoreSettingDTO>(), It.IsAny<StoreSetting>()), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStoreSettingAsync_WhenSaveChangesThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var existingSetting = CreateValidStoreSetting();
            var expectedException = new InvalidOperationException("Database save error");

            SetupQueryableRepository(new List<StoreSetting> { existingSetting });

            _mockMapper.Setup(m => m.Map(dto, existingSetting));
            _mockStoreSettingRepository.Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _storeSettingService.UpdateStoreSettingAsync(_testStoreId, dto, _validUserId));

            Assert.Equal("Database save error", exception.Message);
            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map(dto, existingSetting), Times.Once);
        }

        [Fact]
        public async Task UpdateStoreSettingAsync_WhenMapperThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var existingSetting = CreateValidStoreSetting();
            var expectedException = new AutoMapperMappingException("Mapping error");

            SetupQueryableRepository(new List<StoreSetting> { existingSetting });

            _mockMapper.Setup(m => m.Map(dto, existingSetting))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _storeSettingService.UpdateStoreSettingAsync(_testStoreId, dto, _validUserId));

            Assert.Equal("Mapping error", exception.Message);
            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStoreSettingAsync_ShouldHandleNullStoreSettingCorrectly()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var existingSetting = CreateValidStoreSetting();
            // Simulate the case where storeSetting is null after query

            SetupQueryableRepository(new List<StoreSetting>());

            // Act
            var result = await _storeSettingService.UpdateStoreSettingAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.False(result);
            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map(It.IsAny<StoreSettingDTO>(), It.IsAny<StoreSetting>()), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}