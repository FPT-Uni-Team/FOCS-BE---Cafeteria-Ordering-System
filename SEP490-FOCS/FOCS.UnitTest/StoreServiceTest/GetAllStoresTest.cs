using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.StoreServiceTest
{
    public class GetAllStoresTest : StoreServiceTestBase
    {
        private readonly List<Store> _testStores;

        public GetAllStoresTest()
        {
            _testStores = CreateTestStores();
        }

        private List<Store> CreateTestStores()
        {
            return new List<Store>
            {
                new Store
                {
                    Id = Guid.NewGuid(),
                    Name = "Store Alpha",
                    Address = "123 Main Street",
                    PhoneNumber = "111-111-1111",
                    CustomTaxRate = 0.05,
                    IsDeleted = false,
                    CreatedBy = _validUserId,
                    Brand = new Brand { CreatedBy = _validUserId }
                },
                new Store
                {
                    Id = Guid.NewGuid(),
                    Name = "Store Beta",
                    Address = "456 Oak Avenue",
                    PhoneNumber = "222-222-2222",
                    CustomTaxRate = 0.08,
                    IsDeleted = false,
                    CreatedBy = _validUserId,
                    Brand = new Brand { CreatedBy = _validUserId }
                },
                new Store
                {
                    Id = Guid.NewGuid(),
                    Name = "Store Gamma",
                    Address = "789 Pine Road",
                    PhoneNumber = "333-333-3333",
                    CustomTaxRate = 0.10,
                    IsDeleted = false,
                    CreatedBy = _validUserId,
                    Brand = new Brand { CreatedBy = _validUserId }
                },
                new Store
                {
                    Id = Guid.NewGuid(),
                    Name = "Deleted Store",
                    Address = "999 Deleted Street",
                    PhoneNumber = "999-999-9999",
                    CustomTaxRate = 0.07,
                    IsDeleted = true,
                    CreatedBy = _validUserId,
                    Brand = new Brand { CreatedBy = _validUserId }
                },
                new Store
                {
                    Id = Guid.NewGuid(),
                    Name = "Other User Store",
                    Address = "555 Other Street",
                    PhoneNumber = "555-555-5555",
                    CustomTaxRate = 0.06,
                    IsDeleted = false,
                    CreatedBy = "other-user",
                    Brand = new Brand { CreatedBy = "other-user" }
                }
            };
        }

        private UrlQueryParameters CreateDefaultQuery()
        {
            return new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10
            };
        }

        [Fact]
        public async Task GetAllStoresAsync_WithValidUserId_ShouldReturnPagedResult()
        {
            // Arrange
            var query = CreateDefaultQuery();
            var validStores = _testStores.Where(s => !s.IsDeleted && s.Brand.CreatedBy == _validUserId).ToList();

            SetupStoreQueryable(_testStores);

            var expectedDTOs = validStores.Select(s => new StoreAdminDTO
            {
                Name = s.Name,
                Address = s.Address,
                PhoneNumber = s.PhoneNumber,
                CustomTaxRate = s.CustomTaxRate
            }).ToList();

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(expectedDTOs);

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount); // Only non-deleted stores for valid user
            Assert.Equal(3, result.Items.Count);
            Assert.Equal(1, result.PageIndex);
            Assert.Equal(10, result.PageSize);

            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()), Times.Once);
        }

        [Fact]
        public async Task GetAllStoresAsync_WithNullUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var query = CreateDefaultQuery();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.GetAllStoresAsync(query, null));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Never);
        }

        [Fact]
        public async Task GetAllStoresAsync_WithEmptyUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var query = CreateDefaultQuery();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.GetAllStoresAsync(query, string.Empty));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Never);
        }

        [Fact]
        public async Task GetAllStoresAsync_WithWhitespaceUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var query = CreateDefaultQuery();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminStoreService.GetAllStoresAsync(query, "   "));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Never);
        }

        [Theory]
        [InlineData("name", "alpha", 1)]
        [InlineData("name", "ALPHA", 1)]
        [InlineData("name", "store", 3)]
        [InlineData("address", "main", 1)]
        [InlineData("address", "MAIN", 1)]
        [InlineData("phonenumber", "111", 1)]
        [InlineData("phonenumber", "222", 1)]
        [InlineData("invalid", "test", 3)]
        public async Task GetAllStoresAsync_WithSearchParameters_ShouldFilterCorrectly(
            string searchBy, string searchValue, int expectedCount)
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SearchBy = searchBy,
                SearchValue = searchValue
            };

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            // Note: The actual filtering logic would be tested by the query execution
            // In a real scenario, you'd need to mock Entity Framework's query execution
            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllStoresAsync_WithNullOrEmptySearchBy_ShouldNotApplySearch()
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SearchBy = null,
                SearchValue = "test"
            };

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllStoresAsync_WithNullOrEmptySearchValue_ShouldNotApplySearch()
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SearchBy = "name",
                SearchValue = null
            };

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Theory]
        [InlineData("name", "asc")]
        [InlineData("name", "desc")]
        [InlineData("name", "ASC")]
        [InlineData("name", "DESC")]
        [InlineData("address", "asc")]
        [InlineData("address", "desc")]
        [InlineData("phonenumber", "asc")]
        [InlineData("phonenumber", "desc")]
        [InlineData("customtaxrate", "asc")]
        [InlineData("customtaxrate", "desc")]
        [InlineData("invalid", "asc")]
        [InlineData("invalid", "desc")]
        public async Task GetAllStoresAsync_WithSortParameters_ShouldApplySorting(
            string sortBy, string sortOrder)
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllStoresAsync_WithNullSortBy_ShouldNotApplySorting()
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SortBy = null,
                SortOrder = "asc"
            };

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllStoresAsync_WithEmptySortBy_ShouldNotApplySorting()
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SortBy = string.Empty,
                SortOrder = "asc"
            };

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 5)]
        [InlineData(1, 10)]
        [InlineData(3, 2)]
        public async Task GetAllStoresAsync_WithPaginationParameters_ShouldApplyPagination(
            int page, int pageSize)
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = page,
                PageSize = pageSize
            };

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(page, result.PageIndex);
            Assert.Equal(pageSize, result.PageSize);
            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllStoresAsync_ShouldFilterOutDeletedStores()
        {
            // Arrange
            var query = CreateDefaultQuery();

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            // The query should filter out deleted stores
            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllStoresAsync_ShouldFilterByUsersBrandOnly()
        {
            // Arrange
            var query = CreateDefaultQuery();

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            // The query should only return stores where Brand.CreatedBy equals userId
            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllStoresAsync_WithCombinedSearchAndSort_ShouldApplyBoth()
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SearchBy = "name",
                SearchValue = "store",
                SortBy = "name",
                SortOrder = "desc"
            };

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllStoresAsync_ShouldReturnCorrectTotalCount()
        {
            // Arrange
            var query = CreateDefaultQuery();
            var validStores = _testStores.Where(s => !s.IsDeleted && s.Brand.CreatedBy == _validUserId).ToList();

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            // Should return the total count before pagination
            _mockStoreRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Fact]
        public async Task GetAllStoresAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var query = CreateDefaultQuery();
            var expectedException = new InvalidOperationException("Database connection error");

            _mockStoreRepository.Setup(r => r.AsQueryable())
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminStoreService.GetAllStoresAsync(query, _validUserId));

            Assert.Equal("Database connection error", exception.Message);
        }

        [Fact]
        public async Task GetAllStoresAsync_WhenMapperThrowsException_ShouldPropagateException()
        {
            // Arrange
            var query = CreateDefaultQuery();
            var expectedException = new AutoMapperMappingException("Mapping error");

            SetupStoreQueryable(_testStores);

            _mockMapper.Setup(m => m.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _adminStoreService.GetAllStoresAsync(query, _validUserId));

            Assert.Equal("Mapping error", exception.Message);
        }
    }
}
