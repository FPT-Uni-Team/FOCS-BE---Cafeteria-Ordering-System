using AutoMapper;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.StoreSettingServiceTest
{
    public class CreateStoreSettingTest : StoreSettingServiceTestBase
    {
        [Fact]
        public async Task CreateStoreSettingAsync_WithValidInput_ShouldCreateAndReturnStoreSetting()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var newStoreSetting = CreateValidStoreSetting();

            // Setup: No existing store setting
            SetupQueryableRepository(new List<StoreSetting>());

            _mockMapper.Setup(m => m.Map<StoreSetting>(dto))
                .Returns(newStoreSetting);

            _mockMapper.Setup(m => m.Map<StoreSettingDTO>(It.IsAny<StoreSetting>()))
                .Returns(dto);

            _mockStoreSettingRepository.Setup(r => r.AddAsync(It.IsAny<StoreSetting>()))
                .Returns(Task.CompletedTask);

            _mockStoreSettingRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _storeSettingService.CreateStoreSettingAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.OpenTime, result.OpenTime);
            Assert.Equal(dto.CloseTime, result.CloseTime);
            Assert.Equal(dto.Currency, result.Currency);

            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<StoreSetting>(dto), Times.Once);
            _mockStoreSettingRepository.Verify(r => r.AddAsync(It.IsAny<StoreSetting>()), Times.Once);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<StoreSettingDTO>(It.IsAny<StoreSetting>()), Times.Once);
        }

        [Fact]
        public async Task CreateStoreSettingAsync_WithExistingStoreSetting_ShouldThrowFOCSException()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var existingStoreSetting = CreateValidStoreSetting();

            SetupQueryableRepository(new List<StoreSetting> { existingStoreSetting });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _storeSettingService.CreateStoreSettingAsync(_testStoreId, dto, _validUserId));

            Assert.Equal(Errors.StoreSetting.SettingExist + "@", exception.Message);

            _mockStoreSettingRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<StoreSetting>(It.IsAny<StoreSettingDTO>()), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.AddAsync(It.IsAny<StoreSetting>()), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateStoreSettingAsync_ShouldSetCorrectProperties()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var newStoreSetting = CreateValidStoreSetting();
            StoreSetting capturedStoreSetting = null;

            SetupQueryableRepository(new List<StoreSetting>());

            _mockMapper.Setup(m => m.Map<StoreSetting>(dto))
                .Returns(newStoreSetting);

            _mockMapper.Setup(m => m.Map<StoreSettingDTO>(It.IsAny<StoreSetting>()))
                .Returns(dto);

            _mockStoreSettingRepository.Setup(r => r.AddAsync(It.IsAny<StoreSetting>()))
                .Callback<StoreSetting>(ss => capturedStoreSetting = ss)
                .Returns(Task.CompletedTask);

            _mockStoreSettingRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _storeSettingService.CreateStoreSettingAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.NotNull(capturedStoreSetting);
            Assert.NotEqual(Guid.Empty, capturedStoreSetting.Id);
            Assert.Equal(_testStoreId, capturedStoreSetting.StoreId);
            Assert.Equal(_validUserId, capturedStoreSetting.CreatedBy);
            Assert.True(capturedStoreSetting.CreatedAt <= DateTime.UtcNow);
            Assert.True(capturedStoreSetting.CreatedAt >= DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task CreateStoreSettingAsync_WithDeletedExistingSetting_ShouldCreateNewOne()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var newStoreSetting = CreateValidStoreSetting();
            var deletedStoreSetting = CreateValidStoreSetting();
            deletedStoreSetting.IsDeleted = true;

            SetupQueryableRepository(new List<StoreSetting> { deletedStoreSetting });

            _mockMapper.Setup(m => m.Map<StoreSetting>(dto))
                .Returns(newStoreSetting);

            _mockMapper.Setup(m => m.Map<StoreSettingDTO>(It.IsAny<StoreSetting>()))
                .Returns(dto);

            _mockStoreSettingRepository.Setup(r => r.AddAsync(It.IsAny<StoreSetting>()))
                .Returns(Task.CompletedTask);

            _mockStoreSettingRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _storeSettingService.CreateStoreSettingAsync(_testStoreId, dto, _validUserId);

            // Assert
            Assert.NotNull(result);
            _mockStoreSettingRepository.Verify(r => r.AddAsync(It.IsAny<StoreSetting>()), Times.Once);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateStoreSettingAsync_WhenRepositoryAddThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var newStoreSetting = CreateValidStoreSetting();
            var expectedException = new InvalidOperationException("Database add error");

            SetupQueryableRepository(new List<StoreSetting>());

            _mockMapper.Setup(m => m.Map<StoreSetting>(dto))
                .Returns(newStoreSetting);

            _mockStoreSettingRepository.Setup(r => r.AddAsync(It.IsAny<StoreSetting>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _storeSettingService.CreateStoreSettingAsync(_testStoreId, dto, _validUserId));

            Assert.Equal("Database add error", exception.Message);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateStoreSettingAsync_WhenSaveChangesThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var newStoreSetting = CreateValidStoreSetting();
            var expectedException = new InvalidOperationException("Database save error");

            SetupQueryableRepository(new List<StoreSetting>());

            _mockMapper.Setup(m => m.Map<StoreSetting>(dto))
                .Returns(newStoreSetting);

            _mockStoreSettingRepository.Setup(r => r.AddAsync(It.IsAny<StoreSetting>()))
                .Returns(Task.CompletedTask);

            _mockStoreSettingRepository.Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _storeSettingService.CreateStoreSettingAsync(_testStoreId, dto, _validUserId));

            Assert.Equal("Database save error", exception.Message);
            _mockStoreSettingRepository.Verify(r => r.AddAsync(It.IsAny<StoreSetting>()), Times.Once);
        }

        [Fact]
        public async Task CreateStoreSettingAsync_WhenMapperThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dto = CreateValidStoreSettingDTO();
            var expectedException = new AutoMapperMappingException("Mapping error");

            SetupQueryableRepository(new List<StoreSetting>());

            _mockMapper.Setup(m => m.Map<StoreSetting>(dto))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _storeSettingService.CreateStoreSettingAsync(_testStoreId, dto, _validUserId));

            Assert.Equal("Mapping error", exception.Message);
            _mockStoreSettingRepository.Verify(r => r.AddAsync(It.IsAny<StoreSetting>()), Times.Never);
        }
    }
}