using FOCS.Common.Constants;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Http;
using MockQueryable;
using Moq;

namespace FOCS.UnitTest.TableServiceTest
{
    public class GenerateQrCodeForTableTests : TableServiceTestBase
    {
        private readonly Guid _tableId = Guid.NewGuid();
        private readonly Guid _storeId = Guid.NewGuid();
        private readonly string _userId = "valid-user";

        [Fact]
        public async Task GenerateQrCodeForTableAsync_ShouldReturnQrUrl_WhenValidInput()
        {
            // Arrange
            var table = new Table
            {
                Id = _tableId,
                StoreId = _storeId,
                QrVersion = 0,
                IsDeleted = false
            };
            var expectedUrl = "https://cdn.focs.site/table-qrcode.png";

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(new List<Table> { table }.AsQueryable().BuildMock());

            _cloudinaryServiceMock.Setup(s => s.UploadQrCodeForTable(
                    It.IsAny<IFormFile>(), _storeId.ToString(), _tableId.ToString()))
                .ReturnsAsync(new UploadedImageResult { Url = expectedUrl });

            _tableRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _tableService.GenerateQrCodeForTableAsync(_tableId, _userId, _storeId);

            // Assert
            Assert.Equal(expectedUrl, result);
            Assert.Equal(1, table.QrVersion);
            Assert.Equal(expectedUrl, table.QrCode);
            Assert.Equal(_userId, table.UpdatedBy);
            Assert.True((DateTime.UtcNow - table.UpdatedAt!.Value).TotalSeconds < 5);
        }

        [Fact]
        public async Task GenerateQrCodeForTableAsync_ShouldThrow_WhenUserIdIsEmpty()
        {
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.GenerateQrCodeForTableAsync(_tableId, "", _storeId));

            Assert.StartsWith(TableConstants.UserIdEmpty, ex.Message);
        }

        [Fact]
        public async Task GenerateQrCodeForTableAsync_ShouldThrow_WhenTableNotFound()
        {
            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(new List<Table>().AsQueryable().BuildMock());

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.GenerateQrCodeForTableAsync(_tableId, _userId, _storeId));

            Assert.StartsWith(TableConstants.TableEmpty, ex.Message);
        }

        [Fact]
        public async Task GenerateQrCodeForTableAsync_ShouldUpdateQrVersionCorrectly()
        {
            // Arrange
            var table = new Table
            {
                Id = _tableId,
                StoreId = _storeId,
                QrVersion = 4,
                IsDeleted = false
            };

            var expectedUrl = "https://cdn.focs.site/qr-v5.png";

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(new List<Table> { table }.AsQueryable().BuildMock());

            _cloudinaryServiceMock.Setup(s => s.UploadQrCodeForTable(
                    It.IsAny<IFormFile>(), _storeId.ToString(), _tableId.ToString()))
                .ReturnsAsync(new UploadedImageResult { Url = expectedUrl });

            _tableRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _tableService.GenerateQrCodeForTableAsync(_tableId, _userId, _storeId);

            // Assert
            Assert.Equal(expectedUrl, result);
            Assert.Equal(5, table.QrVersion);
            Assert.Equal(expectedUrl, table.QrCode);
        }

        [Fact]
        public async Task GenerateQrCodeForTableAsync_ShouldThrow_WhenCloudinaryReturnsNull()
        {
            var table = new Table
            {
                Id = _tableId,
                StoreId = _storeId,
                QrVersion = 1,
                IsDeleted = false
            };

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(new List<Table> { table }.AsQueryable().BuildMock());

            _cloudinaryServiceMock.Setup(s => s.UploadQrCodeForTable(
                It.IsAny<IFormFile>(), _storeId.ToString(), _tableId.ToString()))
                .ReturnsAsync((UploadedImageResult)null!); // simulate null result

            _tableRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            await Assert.ThrowsAsync<NullReferenceException>(() =>
                _tableService.GenerateQrCodeForTableAsync(_tableId, _userId, _storeId));
        }
    }
}