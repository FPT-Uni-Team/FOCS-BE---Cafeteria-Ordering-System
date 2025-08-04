using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.PaymentServiceTest
{
    public class UpdateConfigPaymentTests : PaymentServiceTestBase
    {
        [Fact]
        public async Task UpdateConfigPayment_ShouldReturnTrueAndEncryptAndSave_WhenSettingExists()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var setting = new StoreSetting
            {
                StoreId = storeId,
                PayOSClientId = "oldClient",
                PayOSApiKey = "oldApi",
                PayOSChecksumKey = "oldChecksum"
            };
            SetupStoreSetting(setting);

            var request = new UpdateConfigPaymentRequest
            {
                PayOSClientId = "newClient",
                PayOSApiKey = "newApi",
                PayOSChecksumKey = "newChecksum"
            };

            // Act
            var result = await _adminStoreService.UpdateConfigPayment(request, storeId.ToString());

            // Assert
            Assert.True(result);

            // 1) Update() và SaveChangesAsync() phải được gọi
            _storeSettingRepoMock.Verify(x => x.Update(setting), Times.Once);
            _storeSettingRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            // 2) Dùng SecretProtector để decrypt và so sánh với gốc
            var protector = new SecretProtector(_dpProvider);
            Assert.Equal(request.PayOSClientId, protector.Decrypt(setting.PayOSClientId));
            Assert.Equal(request.PayOSApiKey, protector.Decrypt(setting.PayOSApiKey));
            Assert.Equal(request.PayOSChecksumKey, protector.Decrypt(setting.PayOSChecksumKey));
        }

        [Fact]
        public async Task UpdateConfigPayment_ShouldReturnFalse_WhenSettingNotFound()
        {
            // Arrange: không có setting nào
            SetupStoreSetting(null);

            var request = new UpdateConfigPaymentRequest
            {
                PayOSClientId = "c1",
                PayOSApiKey = "k1",
                PayOSChecksumKey = "h1"
            };
            var storeId = Guid.NewGuid().ToString();

            // Act
            var result = await _adminStoreService.UpdateConfigPayment(request, storeId);

            // Assert
            Assert.False(result);

            _storeSettingRepoMock.Verify(x => x.Update(It.IsAny<StoreSetting>()), Times.Never);
            _storeSettingRepoMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateConfigPayment_ShouldReturnFalse_WhenSaveChangesThrows()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var setting = new StoreSetting { StoreId = storeId };
            SetupStoreSetting(setting);

            var request = new UpdateConfigPaymentRequest
            {
                PayOSClientId = "c2",
                PayOSApiKey = "k2",
                PayOSChecksumKey = "h2"
            };

            // Mock SaveChangesAsync ném lỗi
            _storeSettingRepoMock
                .Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _adminStoreService.UpdateConfigPayment(request, storeId.ToString());

            // Assert
            Assert.False(result);

            // Update vẫn được gọi
            _storeSettingRepoMock.Verify(x => x.Update(It.Is<StoreSetting>(s => s == setting)), Times.Once);
            _storeSettingRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateConfigPayment_ShouldReturnFalse_WhenStoreIdIsInvalidGuid()
        {
            // Arrange
            const string badStoreId = "not-a-guid";
            var request = new UpdateConfigPaymentRequest
            {
                PayOSClientId = "c3",
                PayOSApiKey = "k3",
                PayOSChecksumKey = "h3"
            };

            // Act
            var result = await _adminStoreService.UpdateConfigPayment(request, badStoreId);

            // Assert
            Assert.False(result);

            _storeSettingRepoMock.Verify(x => x.Update(It.IsAny<StoreSetting>()), Times.Never);
            _storeSettingRepoMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }
    }
}
