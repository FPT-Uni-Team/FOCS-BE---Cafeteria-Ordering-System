using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FOCS.UnitTest.TableServiceTest
{
    public class GetTableByIdTests : TableServiceTestBase
    {
        [Fact]
        public async Task GetTableByIdAsync_ShouldThrow_WhenUserIdIsEmpty()
        {
            // Arrange
            var tableId = Guid.NewGuid();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.GetTableByIdAsync(tableId, "")
            );
            Assert.StartsWith(TableConstants.UserIdEmpty, ex.Message);
        }

        [Fact]
        public async Task GetTableByIdAsync_ShouldThrow_WhenTableNotFound()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync((Table)null!);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.GetTableByIdAsync(tableId, "valid-user")
            );
            Assert.StartsWith(Errors.Common.NotFound, ex.Message); // from Errors.Common.NotFound
        }

        [Fact]
        public async Task GetTableByIdAsync_ShouldThrow_WhenTableIsDeleted()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var deletedTable = new Table { Id = tableId, IsDeleted = true };
            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(deletedTable);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.GetTableByIdAsync(tableId, "valid-user")
            );

            Assert.StartsWith(Errors.Common.NotFound, ex.Message);
            Assert.Contains("id", ex.Message);
        }

        [Fact]
        public async Task GetTableByIdAsync_ShouldReturnTableDTO_WhenValid()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var table = new Table
            {
                Id = tableId,
                IsDeleted = false,
                TableNumber = 5
            };

            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(table);

            _mapperMock.Setup(m => m.Map<TableDTO>(table)).Returns(new TableDTO
            {
                Id = tableId,
                TableNumber = 5
            });

            // Act
            var result = await _tableService.GetTableByIdAsync(tableId, "valid-user");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result!.TableNumber);
            Assert.Equal(tableId, result.Id);
        }

        [Fact]
        public async Task GetTableByIdAsync_ShouldReturnNull_WhenMapperReturnsNull()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var table = new Table { Id = tableId, IsDeleted = false };

            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(table);
            _mapperMock.Setup(m => m.Map<TableDTO>(table)).Returns((TableDTO?)null);

            // Act
            var result = await _tableService.GetTableByIdAsync(tableId, "valid-user");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTableByIdAsync_ShouldMapAllPropertiesCorrectly()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var table = new Table
            {
                Id = tableId,
                TableNumber = 9,
                Status = TableStatus.Reserved,
                StoreId = Guid.NewGuid(),
                IsDeleted = false
            };

            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(table);

            _mapperMock.Setup(m => m.Map<TableDTO>(table)).Returns(new TableDTO
            {
                Id = tableId,
                TableNumber = 9,
                Status = TableStatus.Reserved,
                StoreId = table.StoreId
            });

            // Act
            var result = await _tableService.GetTableByIdAsync(tableId, "valid-user");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(9, result!.TableNumber);
            Assert.Equal(TableStatus.Reserved, result.Status);
            Assert.Equal(table.StoreId, result.StoreId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetTableByIdAsync_ShouldThrow_WhenUserIdIsNullOrWhiteSpace(string? userId)
        {
            // Arrange
            var tableId = Guid.NewGuid();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.GetTableByIdAsync(tableId, userId!)
            );
            Assert.StartsWith(TableConstants.UserIdEmpty, ex.Message);
        }

    }
}
