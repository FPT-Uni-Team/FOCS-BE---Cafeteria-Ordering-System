using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Http;
using MockQueryable;
using Moq;

namespace FOCS.UnitTest.TableServiceTest
{
    public class CreateTableTests : TableServiceTestBase
    {
        private readonly TableDTO _validDto = new TableDTO { TableNumber = 5, StoreId = Guid.NewGuid() };
        private const string ValidStoreId = "00000000-0000-0000-0000-000000000001";
        private const string ValidUserId = "valid-user-id";

        [Fact]
        public async Task CreateTableAsync_ShouldCreateSuccessfully_WhenValidInput()
        {
            // Arrange
            var dto = _validDto;
            var storeId = ValidStoreId;
            var userId = ValidUserId;

            // EF-style async support on IQueryable
            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(new List<Table>().AsQueryable().BuildMock());

            // QR code stub
            var qrUrl = "https://cdn.focs.site/add-qrcode.png";
            _cloudinaryServiceMock.Setup(s => s.UploadQrCodeForTable(
                    It.IsAny<IFormFile>(), storeId, It.IsAny<string>()))
                .ReturnsAsync(new UploadedImageResult { Url = qrUrl });

            // Stub AddAsync and SaveChangesAsync
            _tableRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Table>()))
                .Returns(Task.CompletedTask);
            _tableRepositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Mapper DTO -> entity
            _mapperMock.Setup(m => m.Map<Table>(dto))
                .Returns(new Table { TableNumber = dto.TableNumber, StoreId = dto.StoreId });
            // Mapper entity -> DTO
            _mapperMock.Setup(m => m.Map<TableDTO>(It.IsAny<Table>()))
                .Returns((Table tbl) => new TableDTO
                {
                    TableNumber = tbl.TableNumber,
                    StoreId = tbl.StoreId,
                    QrCode = tbl.QrCode
                });

            // Act
            var result = await _tableService.CreateTableAsync(dto, storeId, userId);

            // Assert
            Assert.Equal(dto.TableNumber, result.TableNumber);
            Assert.Equal(dto.StoreId, result.StoreId);
            Assert.Equal(qrUrl, result.QrCode);

            _tableRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Table>()), Times.Once());
            VerifySaveChanges(Times.Once());
        }

        [Fact]
        public async Task CreateTableAsync_ShouldThrow_WhenStoreIdIsNull()
        {
            // Arrange
            var dto = new TableDTO { TableNumber = 5, StoreId = Guid.NewGuid() };
            string storeId = null;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _tableService.CreateTableAsync(dto, storeId));
            Assert.StartsWith(TableConstants.UserIdEmpty, ex.Message);
        }

        [Fact]
        public async Task CreateTableAsync_ShouldThrow_WhenStoreIdIsEmpty()
        {
            // Arrange
            var dto = new TableDTO { TableNumber = 5, StoreId = Guid.NewGuid() };
            var storeId = "";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _tableService.CreateTableAsync(dto, storeId));
            Assert.StartsWith(TableConstants.UserIdEmpty, ex.Message);
        }

        [Fact]
        public async Task CreateTableAsync_ShouldThrow_WhenTableNumberAlreadyExists()
        {
            // Arrange
            var dto = new TableDTO { TableNumber = 1, StoreId = Guid.NewGuid() };
            var storeId = "user-abc";

            var existing = new List<Table>
        {
            new Table { TableNumber = 1, StoreId = dto.StoreId, IsDeleted = false }
        };

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(existing.AsQueryable().BuildMock());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _tableService.CreateTableAsync(dto, storeId));
            Assert.StartsWith(TableConstants.UniqueTableNumber, ex.Message);
        }
    }
}
