using FOCS.Common.Constants;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Http;
using MockQueryable;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var result = await _tableService.GenerateQrCodeForTableAsync(
                actionType: null,
                tableId: _tableId,
                userId: _userId,
                storeId: _storeId);

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
            await Assert.ThrowsAsync<Exception>(async () =>
                await _tableService.GenerateQrCodeForTableAsync(
                    actionType: null,
                    tableId: _tableId,
                    userId: string.Empty,
                    storeId: _storeId));
        }

        [Fact]
        public async Task GenerateQrCodeForTableAsync_ShouldThrow_WhenTableNotFound()
        {
            // No tables in repository
            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(new List<Table>().AsQueryable().BuildMock());

            await Assert.ThrowsAsync<Exception>(async () =>
                await _tableService.GenerateQrCodeForTableAsync(
                    actionType: null,
                    tableId: _tableId,
                    userId: _userId,
                    storeId: _storeId));
        }

        [Fact]
        public async Task GenerateQrCodeForTableAsync_ShouldUpdateQrVersionCorrectly()
        {
            // Arrange existing table with version 4
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
            var result = await _tableService.GenerateQrCodeForTableAsync(
                actionType: null,
                tableId: _tableId,
                userId: _userId,
                storeId: _storeId);

            // Assert
            Assert.Equal(expectedUrl, result);
            Assert.Equal(5, table.QrVersion);
            Assert.Equal(expectedUrl, table.QrCode);
        }

        [Fact]
        public async Task GenerateQrCodeForTableAsync_ShouldThrow_WhenCloudinaryReturnsNull_OnUpdate()
        {
            // Arrange
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
                .ReturnsAsync((UploadedImageResult)null!);
            _tableRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
                await _tableService.GenerateQrCodeForTableAsync(
                    actionType: null,
                    tableId: _tableId,
                    userId: _userId,
                    storeId: _storeId));
        }

        [Fact]
        public async Task GenerateQrCodeForTableAsync_ShouldReturnQrUrl_WhenActionTypeIsAdd()
        {
            // Arrange: no table needed, Add path bypasses repo
            var expectedUrl = "https://cdn.focs.site/add-qrcode.png";
            _cloudinaryServiceMock.Setup(s => s.UploadQrCodeForTable(
                    It.IsAny<IFormFile>(), _storeId.ToString(), _tableId.ToString()))
                .ReturnsAsync(new UploadedImageResult { Url = expectedUrl });

            // Act
            var result = await _tableService.GenerateQrCodeForTableAsync(
                actionType: "Add",
                tableId: _tableId,
                userId: _userId,
                storeId: _storeId);

            // Assert
            Assert.Equal(expectedUrl, result);
            _tableRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task GenerateQrCodeForTableAsync_ShouldThrow_WhenUserIdIsEmpty_OnAdd()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _tableService.GenerateQrCodeForTableAsync(
                    actionType: "Add",
                    tableId: _tableId,
                    userId: string.Empty,
                    storeId: _storeId));
        }

        [Fact]
        public async Task GenerateQrCodeForTableAsync_ShouldThrow_WhenCloudinaryReturnsNull_OnAdd()
        {
            // Arrange: simulate null result for Add
            _cloudinaryServiceMock.Setup(s => s.UploadQrCodeForTable(
                It.IsAny<IFormFile>(), _storeId.ToString(), _tableId.ToString()))
                .ReturnsAsync((UploadedImageResult)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
                await _tableService.GenerateQrCodeForTableAsync(
                    actionType: "Add",
                    tableId: _tableId,
                    userId: _userId,
                    storeId: _storeId));
        }
    }
}
