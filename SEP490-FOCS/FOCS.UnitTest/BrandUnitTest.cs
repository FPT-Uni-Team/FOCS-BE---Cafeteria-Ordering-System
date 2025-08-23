using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FOCS.UnitTest
{
    public class BrandUnitTest
    {
        private readonly Mock<IRepository<Brand>> _mockBrandRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly AdminBrandService _adminBrandService;
        private readonly string _validUserId = Guid.NewGuid().ToString();

        public BrandUnitTest()
        {
            _mockBrandRepository = new Mock<IRepository<Brand>>();
            _mockMapper = new Mock<IMapper>();
            _adminBrandService = new AdminBrandService(_mockBrandRepository.Object, _mockMapper.Object);
        }

        #region CM-59: Create Brand Tests

        [Theory]
        [InlineData(null, 0.1, true, "The Name field is required.")]
        [InlineData("Phuc Brand", 0, true, "Default Tax Rate must between 0.01 and 1")]
        [InlineData("Phuc Brand", 1.1, true, "Default Tax Rate must between 0.01 and 1")]
        [InlineData("Phuc Brand", null, null, "Default Tax Rate must between 0.01 and 1")]
        public async Task CreateBrandAsync_WithInvalidInput_ShouldValidateFalse(
            string name,
            double taxRate,
            bool? isActive,
            string expectedMessage)
        {
            // Arrange
            var dto = new CreateAdminBrandRequest
            {
                Name = name,
                DefaultTaxRate = taxRate,
                IsActive = isActive ?? true
            };

            // Act
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

            //Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("Phuc Brand", 0.1, true)]
        [InlineData("Phuc Brand", 0.1, false)]
        [InlineData("Phuc Brand", 0.1, null)]
        public async Task CreateBrandAsync_WithValidInput_ShouldReturnBrandAdminDTO(
            string name,
            double taxRate,
            bool isActive)
        {
            // Arrange
            var dto = new CreateAdminBrandRequest
            {
                Name = name,
                DefaultTaxRate = taxRate,
                IsActive = isActive
            };

            var newBrand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = name,
                DefaultTaxRate = taxRate,
                IsActive = isActive,
                IsDelete = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _validUserId
            };

            var expectedResult = new BrandAdminDTO
            {
                Id = newBrand.Id,
                Name = name,
                DefaultTaxRate = taxRate,
                IsActive = isActive
            };

            _mockMapper.Setup(m => m.Map<Brand>(dto)).Returns(newBrand);
            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(It.IsAny<Brand>())).Returns(expectedResult);
            _mockBrandRepository.Setup(r => r.AddAsync(It.IsAny<Brand>())).Returns(Task.CompletedTask);
            _mockBrandRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminBrandService.CreateBrandAsync(dto, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Name, result.Name);
            Assert.Equal(expectedResult.DefaultTaxRate, result.DefaultTaxRate);
            Assert.Equal(expectedResult.IsActive, result.IsActive);
        }

        #endregion

        #region CM-60: Get My Brands Tests

        [Theory]
        [InlineData(0, 10, "name", "phuc", "name", "desc")]
        [InlineData(null, 10, "name", "phuc", "name", "desc")]
        [InlineData(1, 0, "name", "phuc", "name", "desc")]
        [InlineData(1, null, "name", "phuc", "name", "desc")]
        public async Task GetAllBrandsAsync_WithInvalidPagination_ShouldThrowException(
            int page,
            int pageSize,
            string searchBy,
            string searchValue,
            string sortBy,
            string sortOrder)
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = page,
                PageSize = pageSize,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Act
            var validationContext = new ValidationContext(query);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(query, validationContext, validationResults, true);

            //Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData(1, 10, "name", "phuc", "name", "desc")]
        [InlineData(1, 10, "name", "phuc", "taxrate", "asc")]
        [InlineData(1, 10, "name", "phuc", null, null)]
        [InlineData(1, 10, null, null, "name", "desc")]
        [InlineData(1, 10, null, "phuc", "name", "desc")]
        [InlineData(1, 10, "name", null, "name", "desc")]
        [InlineData(1, 10, "name", "phuc", "name", null)]
        [InlineData(1, 10, "name", "phuc", null, "desc")]
        [InlineData(1, 10, null, null, null, null)]
        public async Task GetAllBrandsAsync_WithValidInput_ShouldReturnPagedResult(
            int page,
            int pageSize,
            string searchBy,
            string searchValue,
            string sortBy,
            string sortOrder)
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = page,
                PageSize = pageSize,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder
            };
            var brandId = Guid.NewGuid();

            var brands = new List<Brand>
            {
                new Brand { Id = brandId, Name = "Phuc Brand", DefaultTaxRate = 0.1, IsActive = true, IsDelete = false, CreatedBy = _validUserId }
            }.AsQueryable().BuildMockDbSet();

            var brandDTOs = new List<BrandAdminDTO>
            {
                new BrandAdminDTO { Id = brandId, Name = "Phuc Brand", DefaultTaxRate = 0.1, IsActive = true }
            };

            _mockBrandRepository.Setup(r => r.AsQueryable()).Returns(brands.Object);
            _mockMapper.Setup(m => m.Map<List<BrandAdminDTO>>(It.IsAny<List<Brand>>())).Returns(brandDTOs);

            // Act
            var result = await _adminBrandService.GetAllBrandsAsync(query, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResult<BrandAdminDTO>>(result);
        }

        #endregion

        #region CM-61: Get Brand Detail Tests

        [Theory]
        [InlineData("4fa85f64-5717-4562-b3fc-2c963f66afa7")]
        [InlineData("3fa85f64-5717-4562-b3fc")]
        public async Task GetBrandDetailAsync_WithInvalidInput_ShouldThrowException(string id)
        {
            // Arrange

            var brands = new List<Brand>().AsQueryable().BuildMockDbSet();
            _mockBrandRepository.Setup(r => r.AsQueryable()).Returns(brands.Object);

            // Act & Assert
            if (Guid.TryParse(id, out var brandId))
            {
                Assert.Null(await _adminBrandService.GetBrandDetailAsync(brandId, _validUserId));
            }
            else
            {
                await Assert.ThrowsAsync<FormatException>(() => _adminBrandService.GetBrandDetailAsync(Guid.Parse(id), _validUserId));
            }
        }

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task GetBrandDetailAsync_WithValidId_ShouldReturnBrandAdminDTO(string id)
        {
            // Arrange
            var brandId = Guid.Parse(id);
            var brand = new Brand { Id = brandId, Name = "Test Brand", IsDelete = false, CreatedBy = _validUserId };
            var brandDTO = new BrandAdminDTO { Id = brandId, Name = "Test Brand" };

            var brands = new List<Brand> { brand }.AsQueryable().BuildMockDbSet();
            _mockBrandRepository.Setup(r => r.AsQueryable()).Returns(brands.Object);
            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(brand)).Returns(brandDTO);

            // Act
            var result = await _adminBrandService.GetBrandDetailAsync(brandId, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(brandDTO.Id, result.Id);
            Assert.Equal(brandDTO.Name, result.Name);
        }

        #endregion

        #region CM-62: Update Brand Tests

        [Theory]
        [InlineData(null, "Phuc Brand", 0.1, true)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", null, 0.1, true)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Brand", 0.0, true)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Brand", 1.1, true)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Brand", null, true)]
        public async Task UpdateBrandAsync_WithInvalidInput_ShouldValidateFalse(
            string id,
            string name,
            double taxRate,
            bool isActive)
        {
            // Arrange
            var dto = new BrandAdminDTO
            {
                Name = name,
                DefaultTaxRate = taxRate,
                IsActive = isActive
            };

            // Act
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);


            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("4fa85f64-5717-4562-b3fc-2c963f66afa7", "Phuc Brand", 0.1, true)]
        [InlineData("3fa85f64-5717-4562-b3fc", "Phuc Brand", 0.1, true)]
        public async Task UpdateBrandAsync_WithInvalidInput_ShouldThrowExceptionOrReturnFalse(
            string id,
            string name,
            double? taxRate,
            bool? isActive)
        {
            // Arrange
            var dto = new BrandAdminDTO
            {
                Name = name,
                DefaultTaxRate = taxRate ?? 0,
                IsActive = isActive ?? true
            };


            // Act & Assert
            if (Guid.TryParse(id, out var brandId))
            {
                Assert.False(await _adminBrandService.UpdateBrandAsync(brandId, dto, _validUserId));
            }
            else
            {
                await Assert.ThrowsAsync<FormatException>(() => _adminBrandService.UpdateBrandAsync(Guid.Parse(id), dto, _validUserId));
            }
        }

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Brand", 0.1, true)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Brand", 0.1, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Brand", 0.1, null)]
        public async Task UpdateBrandAsync_WithValidInput_ShouldReturnTrue(
            string idString,
            string name,
            double taxRate,
            bool isActive)
        {
            // Arrange
            var id = Guid.Parse(idString);
            var dto = new BrandAdminDTO
            {
                Name = name,
                DefaultTaxRate = taxRate,
                IsActive = isActive
            };

            var brand = new Brand { Id = id, IsDelete = false };
            _mockBrandRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(brand);
            _mockBrandRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map(dto, brand));

            // Act
            var result = await _adminBrandService.UpdateBrandAsync(id, dto, _validUserId);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region CM-63: Delete Brand Tests

        [Theory]
        [InlineData("4fa85f64-5717-4562-b3fc-2c963f66afa7")]
        [InlineData("3fa85f64-5717-4562-b3fc")]

        public async Task DeleteBrandAsync_WithInvalidInput_ShouldThrowException(string id)
        {
            // Arrange
            _mockBrandRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Brand)null);

            // Act & Assert
            if (Guid.TryParse(id, out var brandId))
            {
                Assert.False(await _adminBrandService.DeleteBrandAsync(brandId, _validUserId));
            }
            else
            {
                await Assert.ThrowsAsync<FormatException>(() => _adminBrandService.DeleteBrandAsync(Guid.Parse(id), _validUserId));
            }
        }

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task DeleteBrandAsync_WithValidId_ShouldReturnTrue(string idString)
        {
            // Arrange
            var id = Guid.Parse(idString);
            var brand = new Brand { Id = id, IsDelete = false };
            _mockBrandRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(brand);
            _mockBrandRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminBrandService.DeleteBrandAsync(id, _validUserId);

            // Assert
            Assert.True(result);
            Assert.True(brand.IsDelete);
            Assert.Equal(_validUserId, brand.UpdatedBy);
        }

        #endregion


    }
}
