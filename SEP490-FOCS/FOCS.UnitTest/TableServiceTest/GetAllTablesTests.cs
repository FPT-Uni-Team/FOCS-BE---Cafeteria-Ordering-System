using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable;
using Moq;

namespace FOCS.UnitTest.TableServiceTest
{
    public class GetAllTablesTests : TableServiceTestBase
    {
        private readonly Guid _storeId = Guid.NewGuid();
        private readonly string _userId = "valid-user";

        private List<Table> GetSampleTables() => new List<Table>
        {
            new Table { Id = Guid.NewGuid(), TableNumber = 1, StoreId = _storeId, IsDeleted = false, Status = TableStatus.Available },
            new Table { Id = Guid.NewGuid(), TableNumber = 2, StoreId = _storeId, IsDeleted = false, Status = TableStatus.Occupied },
            new Table { Id = Guid.NewGuid(), TableNumber = 3, StoreId = _storeId, IsDeleted = false, Status = TableStatus.Available }
        };

        [Fact]
        public async Task GetAllTablesAsync_ShouldReturnPagedResult()
        {
            // Arrange
            var query = new UrlQueryParameters { Page = 1, PageSize = 2 };
            var tables = GetSampleTables();

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(tables.AsQueryable().BuildMock());

            _mapperMock.Setup(m => m.Map<List<TableDTO>>(It.IsAny<List<Table>>()))
                .Returns((List<Table> input) => input.Select(t => new TableDTO { TableNumber = t.TableNumber }).ToList());

            // Act
            var result = await _tableService.GetAllTablesAsync(query, _userId, _storeId);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task GetAllTablesAsync_ShouldThrow_WhenUserIdIsEmpty()
        {
            var query = new UrlQueryParameters();
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _tableService.GetAllTablesAsync(query, "", _storeId)
            );
            Assert.StartsWith(TableConstants.UserIdEmpty, ex.Message);
        }

        [Fact]
        public async Task GetAllTablesAsync_ShouldFilterByTableNumber_WhenSearchByIsTableNumber()
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                SearchBy = "table_number",
                SearchValue = "2",
                Page = 1,
                PageSize = 10
            };

            var tables = GetSampleTables();

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(tables.AsQueryable().BuildMock());

            _mapperMock.Setup(m => m.Map<List<TableDTO>>(It.IsAny<List<Table>>()))
                .Returns((List<Table> input) => input.Select(t => new TableDTO { TableNumber = t.TableNumber }).ToList());

            // Act
            var result = await _tableService.GetAllTablesAsync(query, _userId, _storeId);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(2, result.Items.First().TableNumber);
        }

        [Theory]
        [InlineData("table_number", "desc", 3)]
        [InlineData("status", "asc", 1)]
        public async Task GetAllTablesAsync_ShouldSortCorrectly(string sortBy, string sortOrder, int expectedFirstTableNumber)
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder,
                Page = 1,
                PageSize = 10
            };

            var tables = GetSampleTables();

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(tables.AsQueryable().BuildMock());

            _mapperMock.Setup(m => m.Map<List<TableDTO>>(It.IsAny<List<Table>>()))
                .Returns((List<Table> input) => input.Select(t => new TableDTO { TableNumber = t.TableNumber }).ToList());

            // Act
            var result = await _tableService.GetAllTablesAsync(query, _userId, _storeId);

            // Assert
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(expectedFirstTableNumber, result.Items.First().TableNumber);
        }

        [Fact]
        public async Task GetAllTablesAsync_ShouldApplyStatusFilter()
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Filters = new Dictionary<string, string> { { "status", "Occupied" } },
                Page = 1,
                PageSize = 10
            };

            var tables = GetSampleTables();

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(tables.AsQueryable().BuildMock());

            _mapperMock.Setup(m => m.Map<List<TableDTO>>(It.IsAny<List<Table>>()))
                .Returns((List<Table> input) => input.Select(t => new TableDTO { TableNumber = t.TableNumber }).ToList());

            // Act
            var result = await _tableService.GetAllTablesAsync(query, _userId, _storeId);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(2, result.Items.First().TableNumber);
        }

        [Theory]
        [InlineData("2", 1)]
        [InlineData("", 3)]
        public async Task GetAllTablesAsync_SearchBy_TableNumber(string searchValue, int expectedCount)
        {
            var query = new UrlQueryParameters
            {
                SearchBy = "table_number",
                SearchValue = searchValue,
                Page = 1,
                PageSize = 10
            };

            var tables = GetSampleTables();

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(tables.AsQueryable().BuildMock());

            _mapperMock.Setup(m => m.Map<List<TableDTO>>(It.IsAny<List<Table>>()))
                .Returns((List<Table> input) => input.Select(t => new TableDTO { TableNumber = t.TableNumber }).ToList());

            var result = await _tableService.GetAllTablesAsync(query, _userId, _storeId);

            Assert.Equal(expectedCount, result.Items.Count);
        }

        [Theory]
        [InlineData("table_number", "asc", 1)]
        [InlineData("table_number", "desc", 3)]
        [InlineData("status", "asc", 1)]  // Available < Occupied
        [InlineData("status", "desc", 2)] // Occupied > Available
        [InlineData("invalid", "asc", 1)] // fallback to no sort
        public async Task GetAllTablesAsync_SortBy_Field(string sortBy, string sortOrder, int expectedFirstTableNumber)
        {
            var query = new UrlQueryParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder,
                Page = 1,
                PageSize = 10
            };

            var tables = GetSampleTables();

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(tables.AsQueryable().BuildMock());

            _mapperMock.Setup(m => m.Map<List<TableDTO>>(It.IsAny<List<Table>>()))
                .Returns((List<Table> input) => input.Select(t => new TableDTO { TableNumber = t.TableNumber }).ToList());

            var result = await _tableService.GetAllTablesAsync(query, _userId, _storeId);

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(expectedFirstTableNumber, result.Items.First().TableNumber);
        }

        [Theory]
        [InlineData("Available", 2)]
        [InlineData("Occupied", 1)]
        [InlineData("invalid", 3)]
        public async Task GetAllTablesAsync_FilterBy_Status(string status, int expectedCount)
        {
            var query = new UrlQueryParameters
            {
                Filters = new Dictionary<string, string> { { "status", status } },
                Page = 1,
                PageSize = 10
            };

            var tables = GetSampleTables();

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(tables.AsQueryable().BuildMock());

            _mapperMock.Setup(m => m.Map<List<TableDTO>>(It.IsAny<List<Table>>()))
                .Returns((List<Table> input) => input.Select(t => new TableDTO { TableNumber = t.TableNumber }).ToList());

            var result = await _tableService.GetAllTablesAsync(query, _userId, _storeId);

            Assert.Equal(expectedCount, result.Items.Count);
        }
    }
}
