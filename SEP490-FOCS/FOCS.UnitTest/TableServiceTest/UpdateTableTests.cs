using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable;
using Moq;

namespace FOCS.UnitTest.TableServiceTest
{
    public class UpdateTableTests : TableServiceTestBase
    {
        private readonly Guid _storeId = Guid.NewGuid();
        private readonly string _userId = "valid-user";

        [Fact]
        public async Task UpdateTableAsync_ShouldUpdateSuccessfully()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var existingTable = new Table
            {
                Id = tableId,
                TableNumber = 1,
                StoreId = _storeId,
                IsDeleted = false
            };

            var dto = new TableDTO
            {
                TableNumber = 2,
                StoreId = _storeId
            };

            var tables = new List<Table> { existingTable }.AsQueryable();

            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(existingTable);
            _tableRepositoryMock.Setup(r => r.AsQueryable()).Returns(tables.BuildMock());

            _mapperMock.Setup(m => m.Map(dto, existingTable));

            // Act
            var result = await _tableService.UpdateTableAsync(tableId, dto, _userId);

            // Assert
            Assert.True(result);
            _mapperMock.Verify(m => m.Map(dto, existingTable), Times.Once);
            _tableRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTableAsync_ShouldThrow_WhenUserIdIsEmpty()
        {
            // Arrange
            var dto = new TableDTO();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.UpdateTableAsync(Guid.NewGuid(), dto, "")
            );

            Assert.StartsWith(TableConstants.UserIdEmpty, ex.Message);
        }

        [Fact]
        public async Task UpdateTableAsync_ShouldReturnFalse_WhenTableNotFound()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var dto = new TableDTO();

            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync((Table?)null);

            // Act
            var result = await _tableService.UpdateTableAsync(tableId, dto, _userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateTableAsync_ShouldReturnFalse_WhenTableIsDeleted()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var deletedTable = new Table { Id = tableId, IsDeleted = true };
            var dto = new TableDTO();

            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(deletedTable);

            // Act
            var result = await _tableService.UpdateTableAsync(tableId, dto, _userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateTableAsync_ShouldThrow_WhenDuplicateTableNumberExists()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var dto = new TableDTO
            {
                TableNumber = 5,
                StoreId = _storeId
            };

            var existingTable = new Table
            {
                Id = tableId,
                TableNumber = 1,
                StoreId = _storeId,
                IsDeleted = false
            };

            var duplicateTable = new Table
            {
                Id = Guid.NewGuid(),
                TableNumber = 5,
                StoreId = _storeId,
                IsDeleted = false
            };

            var tables = new List<Table> { existingTable, duplicateTable }.AsQueryable();

            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(existingTable);
            _tableRepositoryMock.Setup(r => r.AsQueryable()).Returns(tables.BuildMock());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.UpdateTableAsync(tableId, dto, _userId)
            );

            Assert.StartsWith(TableConstants.UniqueTableNumber, ex.Message);
        }

        [Fact]
        public async Task UpdateTableAsync_ShouldSetUpdatedFieldsCorrectly()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var dto = new TableDTO
            {
                TableNumber = 5,
                StoreId = _storeId
            };

            var table = new Table
            {
                Id = tableId,
                TableNumber = 1,
                StoreId = _storeId,
                IsDeleted = false
            };

            var nowBefore = DateTime.UtcNow;

            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(table);
            _tableRepositoryMock.Setup(r => r.AsQueryable()).Returns(new List<Table> { table }.AsQueryable().BuildMock());

            _mapperMock.Setup(m => m.Map(dto, table)).Callback(() => table.TableNumber = dto.TableNumber);

            // Act
            var result = await _tableService.UpdateTableAsync(tableId, dto, _userId);

            // Assert
            Assert.True(result);
            Assert.Equal(_userId, table.UpdatedBy);
            Assert.True(table.UpdatedAt >= nowBefore);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateTableAsync_ShouldNotCallSaveChanges_WhenTableIsNullOrDeleted(bool isNull)
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var dto = new TableDTO();

            if (isNull)
            {
                _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync((Table?)null);
            }
            else
            {
                var table = new Table { Id = tableId, IsDeleted = true };
                _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(table);
            }

            // Act
            var result = await _tableService.UpdateTableAsync(tableId, dto, _userId);

            // Assert
            Assert.False(result);
            _tableRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateTableAsync_ShouldMapDtoToEntity()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var dto = new TableDTO
            {
                TableNumber = 3,
                StoreId = _storeId
            };

            var table = new Table
            {
                Id = tableId,
                TableNumber = 1,
                StoreId = _storeId,
                IsDeleted = false
            };

            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(table);
            _tableRepositoryMock.Setup(r => r.AsQueryable()).Returns(new List<Table> { table }.AsQueryable().BuildMock());

            _mapperMock.Setup(m => m.Map(dto, table)).Verifiable();

            // Act
            var result = await _tableService.UpdateTableAsync(tableId, dto, _userId);

            // Assert
            Assert.True(result);
            _mapperMock.Verify(m => m.Map(dto, table), Times.Once);
        }

    }
}
