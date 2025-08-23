using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.StoreServiceTest
{
    public class CreateStoreTest : StoreServiceTestBase
    {
        [Fact]
        public async Task CreateStoreAsync_WithValidInput_ShouldCreateStoreAndReturnDTO()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var store = CreateValidStore();

            SetupMapperForCreateStore(dto, store);
            SetupRepositoryForSuccessfulCreate();

            // Act
            var result = await _adminStoreService.CreateStoreAsync(dto, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Address, result.Address);
            Assert.Equal(dto.PhoneNumber, result.PhoneNumber);
            Assert.Equal(dto.CustomTaxRate, result.CustomTaxRate);

            // Verify all method calls
            _mockMapper.Verify(m => m.Map<Store>(dto), Times.Once);
            _mockStoreRepository.Verify(r => r.AddAsync(It.IsAny<Store>()), Times.Once);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockStoreSettingRepository.Verify(r => r.AddAsync(It.IsAny<StoreSetting>()), Times.Once);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<StoreAdminDTO>(It.IsAny<Store>()), Times.Once);
        }

        [Fact]
        public async Task CreateStoreAsync_WithNullUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.CreateStoreAsync(dto, null));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            // Verify no repository methods were called
            _mockStoreRepository.Verify(r => r.AddAsync(It.IsAny<Store>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.AddAsync(It.IsAny<StoreSetting>()), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateStoreAsync_WithEmptyUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.CreateStoreAsync(dto, string.Empty));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            // Verify no repository methods were called
            _mockStoreRepository.Verify(r => r.AddAsync(It.IsAny<Store>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.AddAsync(It.IsAny<StoreSetting>()), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateStoreAsync_WithWhitespaceUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.CreateStoreAsync(dto, "   "));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            // Verify no repository methods were called
            _mockStoreRepository.Verify(r => r.AddAsync(It.IsAny<Store>()), Times.Never);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.AddAsync(It.IsAny<StoreSetting>()), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateStoreAsync_ShouldSetCorrectStoreProperties()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var store = CreateValidStore();
            Store capturedStore = null;

            SetupMapperForCreateStore(dto, store);
            SetupRepositoryForSuccessfulCreate();

            // Capture the store that gets added
            _mockStoreRepository.Setup(r => r.AddAsync(It.IsAny<Store>()))
                .Callback<Store>(s => capturedStore = s)
                .Returns(Task.CompletedTask);

            // Act
            await _adminStoreService.CreateStoreAsync(dto, _validUserId);

            // Assert
            Assert.NotNull(capturedStore);
            Assert.NotEqual(Guid.Empty, capturedStore.Id);
            Assert.False(capturedStore.IsDeleted);
            Assert.True(capturedStore.CreatedAt <= DateTime.UtcNow);
            Assert.True(capturedStore.CreatedAt >= DateTime.UtcNow.AddMinutes(-1)); // Allow for small time difference
            Assert.Equal(_validUserId, capturedStore.CreatedBy);
        }

        [Fact]
        public async Task CreateStoreAsync_ShouldCreateStoreSettingWithCorrectProperties()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var store = CreateValidStore();
            StoreSetting capturedStoreSetting = null;

            SetupMapperForCreateStore(dto, store);
            SetupRepositoryForSuccessfulCreate();

            // Capture the store setting that gets added
            _mockStoreSettingRepository.Setup(r => r.AddAsync(It.IsAny<StoreSetting>()))
                .Callback<StoreSetting>(ss => capturedStoreSetting = ss)
                .Returns(Task.CompletedTask);

            // Act
            await _adminStoreService.CreateStoreAsync(dto, _validUserId);

            // Assert
            Assert.NotNull(capturedStoreSetting);
            Assert.NotEqual(Guid.Empty, capturedStoreSetting.StoreId);
            Assert.True(capturedStoreSetting.CreatedAt <= DateTime.UtcNow);
            Assert.True(capturedStoreSetting.CreatedAt >= DateTime.UtcNow.AddMinutes(-1)); // Allow for small time difference
            Assert.Equal(_validUserId, capturedStoreSetting.CreatedBy);
            Assert.True(capturedStoreSetting.UpdatedAt <= DateTime.UtcNow);
            Assert.True(capturedStoreSetting.UpdatedAt >= DateTime.UtcNow.AddMinutes(-1)); // Allow for small time difference
            Assert.Equal(_validUserId, capturedStoreSetting.UpdatedBy);
        }

        [Fact]
        public async Task CreateStoreAsync_ShouldCallSaveChangesAsyncTwice()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var store = CreateValidStore();

            SetupMapperForCreateStore(dto, store);
            SetupRepositoryForSuccessfulCreate();

            // Act
            await _adminStoreService.CreateStoreAsync(dto, _validUserId);

            // Assert
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateStoreAsync_WhenStoreRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var store = CreateValidStore();
            var expectedException = new InvalidOperationException("Database error");

            SetupMapperForCreateStore(dto, store);

            _mockStoreRepository.Setup(r => r.AddAsync(It.IsAny<Store>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminStoreService.CreateStoreAsync(dto, _validUserId));

            Assert.Equal("Database error", exception.Message);

            // Verify store setting repository methods were not called
            _mockStoreSettingRepository.Verify(r => r.AddAsync(It.IsAny<StoreSetting>()), Times.Never);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateStoreAsync_WhenStoreSettingRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dto = CreateValidStoreAdminDTO();
            var store = CreateValidStore();
            var expectedException = new InvalidOperationException("Database error on store setting");

            SetupMapperForCreateStore(dto, store);

            _mockStoreRepository.Setup(r => r.AddAsync(It.IsAny<Store>()))
                .Returns(Task.CompletedTask);
            _mockStoreRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockStoreSettingRepository.Setup(r => r.AddAsync(It.IsAny<StoreSetting>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminStoreService.CreateStoreAsync(dto, _validUserId));

            Assert.Equal("Database error on store setting", exception.Message);

            // Verify store was still added
            _mockStoreRepository.Verify(r => r.AddAsync(It.IsAny<Store>()), Times.Once);
            _mockStoreRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
