using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable;
using Moq;

namespace FOCS.UnitTest.TableServiceTest
{
    public class CreateTableTests : TableServiceTestBase
    {
        [Fact]
        public async Task CreateTableAsync_ShouldCreateSuccessfully_WhenValidInput()
        {
            // Arrange
            var dto = new TableDTO { TableNumber = 5, StoreId = Guid.NewGuid() };
            var storeId = "valid-store-id";

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(new List<Table>().AsQueryable().BuildMock());

            _mapperMock.Setup(m => m.Map<Table>(dto)).Returns(new Table { TableNumber = dto.TableNumber, StoreId = dto.StoreId });
            _mapperMock.Setup(m => m.Map<TableDTO>(It.IsAny<Table>())).Returns(dto);

            // Act
            var result = await _tableService.CreateTableAsync(dto, storeId);

            // Assert
            Assert.Equal(dto.TableNumber, result.TableNumber);
            _tableRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Table>()), Times.Once);
            _tableRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
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
