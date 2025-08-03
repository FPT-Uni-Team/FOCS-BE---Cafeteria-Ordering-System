using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.BrandServiceTest
{
    public class GetAllBrandsTest : BrandServiceTestBase
    {
        [Fact]
        public async Task GetAllBrandsAsync_WithValidInput_ShouldReturnPagedResult()
        {
            // Arrange
            var query = CreateValidQueryParameters();
            var brands = CreateBrandList(5);
            var brandDTOs = CreateBrandDTOList(5);

            SetupBrandQueryable(brands);

            _mockMapper.Setup(m => m.Map<List<BrandAdminDTO>>(It.IsAny<List<Brand>>()))
                .Returns(brandDTOs);

            // Act
            var result = await _adminBrandService.GetAllBrandsAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(1, result.PageIndex);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(5, result.Items.Count);
            Assert.Equal(brandDTOs, result.Items);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetAllBrandsAsync_WithInvalidUserId_ShouldThrowArgumentException(string invalidUserId)
        {
            // Arrange
            var query = CreateValidQueryParameters();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminBrandService.GetAllBrandsAsync(query, invalidUserId));

            Assert.Equal("UserId is required(Please login).", exception.Message);
        }

        [Fact]
        public async Task GetAllBrandsAsync_ShouldFilterByUserIdAndNotDeleted()
        {
            // Arrange
            var query = CreateValidQueryParameters();
            var brands = new List<Brand>
            {
                CreateValidBrand(false, _validUserId), // Should be included
                CreateValidBrand(true, _validUserId),  // Deleted - should be excluded
                CreateValidBrand(false, Guid.NewGuid().ToString())  // Different user - should be excluded
            };

            SetupBrandQueryable(brands);

            _mockMapper.Setup(m => m.Map<List<BrandAdminDTO>>(It.IsAny<List<Brand>>()))
                .Returns(new List<BrandAdminDTO> { CreateValidBrandDTO() });

            // Act
            var result = await _adminBrandService.GetAllBrandsAsync(query, _validUserId);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task GetAllBrandsAsync_WithNameSearch_ShouldFilterByName()
        {
            // Arrange
            var query = CreateValidQueryParameters();
            query.SearchBy = "name";
            query.SearchValue = "Brand 2";

            var brands = CreateBrandList(3);
            brands[1].Name = "Brand 2 Special"; // This should match

            SetupBrandQueryable(brands);

            _mockMapper.Setup(m => m.Map<List<BrandAdminDTO>>(It.IsAny<List<Brand>>()))
                .Returns(new List<BrandAdminDTO> { CreateValidBrandDTO() });

            // Act
            var result = await _adminBrandService.GetAllBrandsAsync(query, _validUserId);

            // Assert
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetAllBrandsAsync_WithEmptySearchBy_ShouldNotFilter()
        {
            // Arrange
            var query = CreateValidQueryParameters();
            query.SearchBy = "";
            query.SearchValue = "Some Value";

            var brands = CreateBrandList(3);

            SetupBrandQueryable(brands);

            _mockMapper.Setup(m => m.Map<List<BrandAdminDTO>>(It.IsAny<List<Brand>>()))
                .Returns(CreateBrandDTOList(3));

            // Act
            var result = await _adminBrandService.GetAllBrandsAsync(query, _validUserId);

            // Assert
            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task GetAllBrandsAsync_WithEmptySearchValue_ShouldNotFilter()
        {
            // Arrange
            var query = CreateValidQueryParameters();
            query.SearchBy = "name";
            query.SearchValue = "";

            var brands = CreateBrandList(3);

            SetupBrandQueryable(brands);

            _mockMapper.Setup(m => m.Map<List<BrandAdminDTO>>(It.IsAny<List<Brand>>()))
                .Returns(CreateBrandDTOList(3));

            // Act
            var result = await _adminBrandService.GetAllBrandsAsync(query, _validUserId);

            // Assert
            Assert.Equal(3, result.TotalCount);
        }

        [Theory]
        [InlineData("name", "asc")]
        [InlineData("name", "desc")]
        [InlineData("NAME", "ASC")]
        [InlineData("NAME", "DESC")]
        public async Task GetAllBrandsAsync_WithNameSort_ShouldSortByName(string sortBy, string sortOrder)
        {
            // Arrange
            var query = CreateValidQueryParameters();
            query.SortBy = sortBy;
            query.SortOrder = sortOrder;

            var brands = new List<Brand>
            {
                new Brand { Id = Guid.NewGuid(), Name = "C Brand", IsDelete = false, CreatedBy = _validUserId },
                new Brand { Id = Guid.NewGuid(), Name = "A Brand", IsDelete = false, CreatedBy = _validUserId },
                new Brand { Id = Guid.NewGuid(), Name = "B Brand", IsDelete = false, CreatedBy = _validUserId }
            };

            SetupBrandQueryable(brands);

            _mockMapper.Setup(m => m.Map<List<BrandAdminDTO>>(It.IsAny<List<Brand>>()))
                .Returns((List<Brand> b) => b.Select(brand => new BrandAdminDTO { Name = brand.Name }).ToList());

            // Act
            var result = await _adminBrandService.GetAllBrandsAsync(query, _validUserId);

            // Assert
            if (sortOrder.ToLower() == "desc")
            {
                Assert.Equal("C Brand", result.Items.First().Name);
                Assert.Equal("A Brand", result.Items.Last().Name);
            }
            else
            {
                Assert.Equal("A Brand", result.Items.First().Name);
                Assert.Equal("C Brand", result.Items.Last().Name);
            }
        }

        [Theory]
        [InlineData("taxrate", "asc")]
        [InlineData("taxrate", "desc")]
        [InlineData("TAXRATE", "ASC")]
        [InlineData("TAXRATE", "DESC")]
        public async Task GetAllBrandsAsync_WithTaxRateSort_ShouldSortByTaxRate(string sortBy, string sortOrder)
        {
            // Arrange
            var query = CreateValidQueryParameters();
            query.SortBy = sortBy;
            query.SortOrder = sortOrder;

            var brands = new List<Brand>
            {
                new Brand { Id = Guid.NewGuid(), DefaultTaxRate = 0.15, IsDelete = false, CreatedBy = _validUserId },
                new Brand { Id = Guid.NewGuid(), DefaultTaxRate = 0.05, IsDelete = false, CreatedBy = _validUserId },
                new Brand { Id = Guid.NewGuid(), DefaultTaxRate = 0.10, IsDelete = false, CreatedBy = _validUserId }
            };

            SetupBrandQueryable(brands);

            _mockMapper.Setup(m => m.Map<List<BrandAdminDTO>>(It.IsAny<List<Brand>>()))
                .Returns((List<Brand> b) => b.Select(brand => new BrandAdminDTO { DefaultTaxRate = brand.DefaultTaxRate }).ToList());

            // Act
            var result = await _adminBrandService.GetAllBrandsAsync(query, _validUserId);

            // Assert
            if (sortOrder.ToLower() == "desc")
            {
                Assert.Equal(0.15, result.Items.First().DefaultTaxRate);
                Assert.Equal(0.05, result.Items.Last().DefaultTaxRate);
            }
            else
            {
                Assert.Equal(0.05, result.Items.First().DefaultTaxRate);
                Assert.Equal(0.15, result.Items.Last().DefaultTaxRate);
            }
        }

        [Fact]
        public async Task GetAllBrandsAsync_WithUnknownSortBy_ShouldNotSort()
        {
            // Arrange
            var query = CreateValidQueryParameters();
            query.SortBy = "unknown";
            query.SortOrder = "desc";

            var brands = CreateBrandList(3);
            var originalOrder = brands.Select(b => b.Name).ToList();

            SetupBrandQueryable(brands);

            _mockMapper.Setup(m => m.Map<List<BrandAdminDTO>>(It.IsAny<List<Brand>>()))
                .Returns((List<Brand> b) => b.Select(brand => new BrandAdminDTO { Name = brand.Name }).ToList());

            // Act
            var result = await _adminBrandService.GetAllBrandsAsync(query, _validUserId);

            // Assert
            var resultOrder = result.Items.Select(b => b.Name).ToList();
            Assert.Equal(originalOrder, resultOrder);
        }

        [Fact]
        public async Task GetAllBrandsAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var query = CreateValidQueryParameters();
            query.Page = 2;
            query.PageSize = 2;

            var brands = CreateBrandList(5);

            SetupBrandQueryable(brands);

            _mockMapper.Setup(m => m.Map<List<BrandAdminDTO>>(It.IsAny<List<Brand>>()))
                .Returns((List<Brand> b) => b.Select(brand => new BrandAdminDTO { Name = brand.Name }).ToList());

            // Act
            var result = await _adminBrandService.GetAllBrandsAsync(query, _validUserId);

            // Assert
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.PageIndex);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task GetAllBrandsAsync_WithEmptySortBy_ShouldNotSort()
        {
            // Arrange
            var query = CreateValidQueryParameters();
            query.SortBy = "";

            var brands = CreateBrandList(3);

            SetupBrandQueryable(brands);

            _mockMapper.Setup(m => m.Map<List<BrandAdminDTO>>(It.IsAny<List<Brand>>()))
                .Returns(CreateBrandDTOList(3));

            // Act
            var result = await _adminBrandService.GetAllBrandsAsync(query, _validUserId);

            // Assert
            Assert.Equal(3, result.TotalCount);
        }
    }
}
