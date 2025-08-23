using AutoMapper;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.StoreSettingServiceTest
{
    public class GetStoreSettingTest : StoreSettingServiceTestBase
    {
        [Fact]
        public async Task GetStoreSettingAsync_WithExistingStoreSetting_ShouldReturnMappedDTO()
        {
            // Arrange
            var storeSetting = CreateValidStoreSetting();
            var expectedDTO = CreateValidStoreSettingDTO();

            SetupQueryableRepository(new List<StoreSetting> { storeSetting });

            _mockMapper.Setup(m => m.Map<StoreSettingDTO>(storeSetting))
                .Returns(expectedDTO);

            // Act
            var result = await _storeSettingService.GetStoreSettingAsync(_testStoreId, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDTO.OpenTime, result.OpenTime);
            Assert.Equal(expectedDTO.CloseTime, result.CloseTime);
            Assert.Equal(expectedDTO.Currency, result.Currency);
            Assert.Equal(expectedDTO.PaymentConfig, result.PaymentConfig);
            Assert.Equal(expectedDTO.LogoUrl, result.LogoUrl);
            Assert.Equal(expectedDTO.IsSelfService, result.IsSelfService);
            Assert.Equal(expectedDTO.DiscountStrategy, result.DiscountStrategy);

            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<StoreSettingDTO>(storeSetting), Times.Once);
        }

        [Fact]
        public async Task GetStoreSettingAsync_WithNonExistentStoreSetting_ShouldThrowFOCSException()
        {
            // Arrange
            SetupQueryableRepository(new List<StoreSetting>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _storeSettingService.GetStoreSettingAsync(_testStoreId, _validUserId));

            Assert.Equal(Errors.StoreSetting.StoreSettingNotFound + "@", exception.Message);

            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<StoreSettingDTO>(It.IsAny<StoreSetting>()), Times.Never);
        }

        [Fact]
        public async Task GetStoreSettingAsync_WithDeletedStoreSetting_ShouldThrowFOCSException()
        {
            // Arrange
            var deletedStoreSetting = CreateValidStoreSetting();
            deletedStoreSetting.IsDeleted = true;

            SetupQueryableRepository(new List<StoreSetting> { deletedStoreSetting });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _storeSettingService.GetStoreSettingAsync(_testStoreId, _validUserId));

            Assert.Equal(Errors.StoreSetting.StoreSettingNotFound + "@", exception.Message);

            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<StoreSettingDTO>(It.IsAny<StoreSetting>()), Times.Never);
        }

        [Fact]
        public async Task GetStoreSettingAsync_WithDifferentStoreId_ShouldThrowFOCSException()
        {
            // Arrange
            var storeSetting = CreateValidStoreSetting();
            storeSetting.StoreId = Guid.NewGuid(); // Different store ID

            SetupQueryableRepository(new List<StoreSetting> { storeSetting });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _storeSettingService.GetStoreSettingAsync(_testStoreId, _validUserId));

            Assert.Equal(Errors.StoreSetting.StoreSettingNotFound + "@", exception.Message);

            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<StoreSettingDTO>(It.IsAny<StoreSetting>()), Times.Never);
        }

        [Fact]
        public async Task GetStoreSettingAsync_WithNullUserId_ShouldStillWork()
        {
            // Arrange
            var storeSetting = CreateValidStoreSetting();
            var expectedDTO = CreateValidStoreSettingDTO();

            SetupQueryableRepository(new List<StoreSetting> { storeSetting });

            _mockMapper.Setup(m => m.Map<StoreSettingDTO>(storeSetting))
                .Returns(expectedDTO);

            // Act
            var result = await _storeSettingService.GetStoreSettingAsync(_testStoreId, null);

            // Assert
            Assert.NotNull(result);
            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<StoreSettingDTO>(storeSetting), Times.Once);
        }

        [Fact]
        public async Task GetStoreSettingAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Database error");

            _mockStoreSettingRepository.Setup(r => r.AsQueryable())
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _storeSettingService.GetStoreSettingAsync(_testStoreId, _validUserId));

            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task GetStoreSettingAsync_WhenMapperThrowsException_ShouldPropagateException()
        {
            // Arrange
            var storeSetting = CreateValidStoreSetting();
            var expectedException = new AutoMapperMappingException("Mapping error");

            SetupQueryableRepository(new List<StoreSetting> { storeSetting });

            _mockMapper.Setup(m => m.Map<StoreSettingDTO>(storeSetting))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _storeSettingService.GetStoreSettingAsync(_testStoreId, _validUserId));

            Assert.Equal("Mapping error", exception.Message);
        }
    }
}