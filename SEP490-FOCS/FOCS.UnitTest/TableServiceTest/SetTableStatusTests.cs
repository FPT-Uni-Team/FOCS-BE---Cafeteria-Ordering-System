using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FOCS.UnitTest.TableServiceTest
{
    public class SetTableStatusTests : TableServiceTestBase
    {
        private readonly Guid _storeId = Guid.NewGuid();
        private readonly string _userId = "valid-user";

        private Table GetTestTable(Guid tableId)
        {
            return new Table
            {
                Id = tableId,
                StoreId = _storeId,
                TableNumber = 1,
                Status = TableStatus.Available,
                IsDeleted = false
            };
        }

        [Fact]
        public async Task SetTableStatusAsync_ShouldUpdateStatus_WhenValid()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var table = GetTestTable(tableId);
            var tableList = new List<Table> { table };

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(tableList.AsQueryable().BuildMock());

            _tableRepositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _tableService.SetTableStatusAsync(tableId, TableStatus.Occupied, _userId, _storeId);

            // Assert
            Assert.True(result);
            Assert.Equal(TableStatus.Occupied, table.Status);
            Assert.Equal(_userId, table.UpdatedBy);
            Assert.True((DateTime.UtcNow - table.UpdatedAt!.Value).TotalSeconds < 5);
        }

        [Fact]
        public async Task SetTableStatusAsync_ShouldThrow_WhenUserIdIsEmpty()
        {
            // Arrange
            var tableId = Guid.NewGuid();

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.SetTableStatusAsync(tableId, TableStatus.Available, "", _storeId)
            );

            // Assert
            Assert.StartsWith(TableConstants.UserIdEmpty, ex.Message);
        }

        [Fact]
        public async Task SetTableStatusAsync_ShouldThrow_WhenTableStatusInvalid()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var invalidStatus = (TableStatus)999;

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.SetTableStatusAsync(tableId, invalidStatus, _userId, _storeId)
            );

            // Assert
            Assert.StartsWith(TableConstants.InvalidTableStatus, ex.Message);
        }

        [Fact]
        public async Task SetTableStatusAsync_ShouldThrow_WhenTableNotFound()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var emptyList = new List<Table>();

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(emptyList.AsQueryable().BuildMock());

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.SetTableStatusAsync(tableId, TableStatus.Available, _userId, _storeId)
            );

            // Assert
            Assert.StartsWith(TableConstants.TableEmpty, ex.Message);
        }
    }
}
