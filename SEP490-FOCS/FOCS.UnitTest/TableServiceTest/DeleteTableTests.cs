using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FOCS.UnitTest.TableServiceTest
{
    public class DeleteTableTests : TableServiceTestBase
    {
        private readonly string _userId = "valid-user";

        [Fact]
        public async Task DeleteTableAsync_ShouldDeleteSuccessfully()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var table = new Table
            {
                Id = tableId,
                IsDeleted = false
            };

            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(table);

            // Act
            var result = await _tableService.DeleteTableAsync(tableId, _userId);

            // Assert
            Assert.True(result);
            Assert.True(table.IsDeleted);
            Assert.Equal(_userId, table.UpdatedBy);
            Assert.True((DateTime.UtcNow - table.UpdatedAt!.Value).TotalSeconds < 5);
            _tableRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteTableAsync_ShouldReturnFalse_WhenTableIsNull()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync((Table?)null);

            // Act
            var result = await _tableService.DeleteTableAsync(tableId, _userId);

            // Assert
            Assert.False(result);
            _tableRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteTableAsync_ShouldReturnFalse_WhenTableIsAlreadyDeleted()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var table = new Table
            {
                Id = tableId,
                IsDeleted = true
            };

            _tableRepositoryMock.Setup(r => r.GetByIdAsync(tableId)).ReturnsAsync(table);

            // Act
            var result = await _tableService.DeleteTableAsync(tableId, _userId);

            // Assert
            Assert.False(result);
            _tableRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteTableAsync_ShouldThrow_WhenUserIdIsEmpty()
        {
            // Arrange
            var tableId = Guid.NewGuid();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.DeleteTableAsync(tableId, "")
            );

            Assert.StartsWith(TableConstants.UserIdEmpty, ex.Message);
        }
    }
}
