using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.DataProtection;
using MockQueryable.Moq;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace FOCS.UnitTest
{
    public class StoreUnitTest
    {
        private readonly Mock<IRepository<Store>> _mockStoreRepository;
        private readonly Mock<IRepository<StoreSetting>> _mockStoreSettingRepository;
        private readonly Mock<IRepository<Brand>> _mockBrandRepository;
        private readonly Mock<IRepository<PaymentAccount>> _mockPaymentAccountRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IDataProtectionProvider> _mockDataProtectionProvider;
        private readonly Mock<IDataProtector> _mockDataProtector;
        private readonly AdminStoreService _adminStoreService;
        private readonly string _validUserId = Guid.NewGuid().ToString();

        public StoreUnitTest()
        {
            _mockStoreRepository = new Mock<IRepository<Store>>();
            _mockStoreSettingRepository = new Mock<IRepository<StoreSetting>>();
            _mockBrandRepository = new Mock<IRepository<Brand>>();
            _mockPaymentAccountRepository = new Mock<IRepository<PaymentAccount>>();
            _mockMapper = new Mock<IMapper>();
            _mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            _mockDataProtector = new Mock<IDataProtector>();

            _mockDataProtectionProvider.Setup(x => x.CreateProtector("PayOS.Protection"))
                .Returns(_mockDataProtector.Object);

            _adminStoreService = new AdminStoreService(
                _mockStoreRepository.Object,
                _mockPaymentAccountRepository.Object,
                _mockStoreSettingRepository.Object,
                _mockMapper.Object,
                _mockDataProtectionProvider.Object,
                _mockBrandRepository.Object
            );
        }

        #region CreateStore Tests (CM-64)

        [Theory]
        [InlineData(null, "Phuc Address", "0987654321", 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("Phuc Store", null, "0987654321", 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("Phuc Store", "Phuc Address", "966777888", 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("Phuc Store", "Phuc Address", null, 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("Phuc Store", "Phuc Address", "0987654321", 0.0, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("Phuc Store", "Phuc Address", "0987654321", 1.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("Phuc Store", "Phuc Address", "0987654321", null, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task CreateStoreAsync_WithInvalidInput_ShouldValidateFalse(
            string name,
            string address,
            string phoneNumber,
            double customTaxRate,
            bool isActive,
            string brandIdStr)
        {
            // Arrange
            var dto = new StoreAdminDTO
            {
                Name = name,
                Address = address,
                PhoneNumber = phoneNumber,
                CustomTaxRate = customTaxRate,
                IsActive = isActive,
                BrandId = Guid.Parse(brandIdStr)
            };

            // Act
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

            //Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("Phuc Store", "Phuc Address", "0987654321", 0.1, true, "4fa85f64-5717-4562-b3fc-2c963f66afa7")]
        public async Task CreateStoreAsync_WithInvalidInput_ShouldThrowException(
            string name,
            string address,
            string phoneNumber,
            double customTaxRate,
            bool isActive,
            string brandIdStr)
        {
            // Arrange
            Guid? brandId = null;
            if (!string.IsNullOrEmpty(brandIdStr) && Guid.TryParse(brandIdStr, out var parsedId))
            {
                brandId = parsedId;
            }
            var dto = new StoreAdminDTO
            {
                Name = name,
                Address = address,
                PhoneNumber = phoneNumber,
                CustomTaxRate = customTaxRate,
                IsActive = isActive,
                BrandId = brandId.Value
            };

            // Setup brand repository mock
            _mockBrandRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid?>()))
                .ReturnsAsync((Brand)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _adminStoreService.CreateStoreAsync(dto, _validUserId));
        }

        [Theory]
        [InlineData("Phuc Store", "Phuc Address", "0987654321", 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("Phuc Store", "Phuc Address", "0987654321", 0.1, false, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("Phuc Store", "Phuc Address", "0987654321", 0.1, null, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task CreateStoreAsync_WithValidInput_ShouldReturnStoreAdminDTO(
            string name,
            string address,
            string phoneNumber,
            double customTaxRate,
            bool isActive,
            string brandIdStr)
        {
            // Arrange
            Guid? brandId = null;
            if (!string.IsNullOrEmpty(brandIdStr) && Guid.TryParse(brandIdStr, out var parsedId))
            {
                brandId = parsedId;
            }
            var dto = new StoreAdminDTO
            {
                Name = name,
                Address = address,
                PhoneNumber = phoneNumber,
                CustomTaxRate = customTaxRate,
                IsActive = isActive,
                BrandId = brandId.Value
            };

            // Setup brand repository mock
            if (brandIdStr == "3fa85f64-5717-4562-b3fc-2c963f66afa6")
            {
                var brand = new Brand { Id = Guid.Parse(brandIdStr), Name = "Test Brand" };
                _mockBrandRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid?>()))
                    .ReturnsAsync(brand);
            }
            else if (brandIdStr != null && brandIdStr != "3fa85f64-5717-4562-b3fc-2c963f66afa6")
            {
                _mockBrandRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid?>()))
                    .ReturnsAsync((Brand)null);
            }

            var newStore = new Store
            {
                Id = Guid.NewGuid(),
                Name = name,
                Address = address,
                PhoneNumber = phoneNumber,
                CustomTaxRate = customTaxRate,
                IsActive = isActive,
                BrandId = dto.BrandId
            };

            _mockMapper.Setup(x => x.Map<Store>(dto)).Returns(newStore);
            _mockMapper.Setup(x => x.Map<StoreAdminDTO>(newStore)).Returns(dto);
            _mockStoreRepository.Setup(x => x.AddAsync(It.IsAny<Store>())).Returns(Task.CompletedTask);
            _mockStoreRepository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _mockStoreSettingRepository.Setup(x => x.AddAsync(It.IsAny<StoreSetting>())).Returns(Task.CompletedTask);
            _mockStoreSettingRepository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act & Assert
            var result = await _adminStoreService.CreateStoreAsync(dto, _validUserId);
            Assert.NotNull(result);
        }

        #endregion

        #region GetMyStores Tests (CM-65)

        [Theory]
        [InlineData(0, 10, "name", "phuc", "name", "desc")]
        [InlineData(null, 10, "name", "phuc", "name", "desc")]
        [InlineData(1, 0, "name", "phuc", "name", "desc")]
        [InlineData(1, null, "name", "phuc", "name", "desc")]
        public async Task GetAllStoresAsync_WithInvalidPagination_ShouldThrowException(
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
        [InlineData(1, 10, null, "phuc", "name", "desc")]
        [InlineData(1, 10, "name", null, "name", "desc")]
        [InlineData(1, 10, null, null, "name", "desc")]
        [InlineData(1, 10, "name", "phuc", "customtaxrate", "desc")]
        [InlineData(1, 10, "name", "phuc", "customtaxrate", "asc")]
        [InlineData(1, 10, "name", "phuc", null, "desc")]
        [InlineData(1, 10, "name", "phuc", "name", null)]
        public async Task GetAllStoresAsync_VariousParameters_ReturnsExpectedResult(
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

            var Stores = new List<Store>
            {
                new Store
                {
                    Id = Guid.NewGuid(),
                    Name = "Phuc Store",
                    Address = "Test Address",
                    PhoneNumber = "0987654321",
                    CustomTaxRate = 0.1,
                    IsActive = true,
                    IsDeleted = false,
                    Brand = new Brand { Id = Guid.NewGuid(), CreatedBy = _validUserId }
                }
            }.AsQueryable().BuildMockDbSet();

            _mockStoreRepository.Setup(x => x.AsQueryable()).Returns(Stores.Object);

            _mockMapper.Setup(x => x.Map<List<StoreAdminDTO>>(It.IsAny<List<Store>>()))
                .Returns(new List<StoreAdminDTO>());

            // Act & Assert
            var result = await _adminStoreService.GetAllStoresAsync(query, _validUserId);
            Assert.NotNull(result);
        }

        #endregion

        #region Get Store Detail Tests (CM-66)

        [Theory]
        [InlineData("4fa85f64-5717-4562-b3fc-2c963f66afa7")]
        [InlineData("3fa85f64-5717-4562-b3fc")]
        public async Task GetStoreDetailAsync_WithInvalidInput_ShouldThrowException(string id)
        {
            // Arrange

            var stores = new List<Store>().AsQueryable().BuildMockDbSet();
            _mockStoreRepository.Setup(r => r.AsQueryable()).Returns(stores.Object);

            // Act & Assert
            if (Guid.TryParse(id, out var storeId))
            {
                await Assert.ThrowsAsync<Exception>(() => _adminStoreService.GetById(storeId));
            }
            else
            {
                await Assert.ThrowsAsync<FormatException>(() => _adminStoreService.GetById(Guid.Parse(id)));
            }
        }

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task GetStoreDetailAsync_WithValidId_ShouldReturnStoreAdminDTO(string id)
        {
            // Arrange
            var storeId = Guid.Parse(id);
            var store = new Store { Id = storeId, Name = "Test Store", IsDeleted = false, CreatedBy = _validUserId };
            var storeDTO = new StoreAdminDTO { Id = storeId, Name = "Test store" };

            _mockStoreRepository.Setup(r => r.GetByIdAsync(storeId)).ReturnsAsync(store);
            _mockMapper.Setup(m => m.Map<StoreAdminDTO>(store)).Returns(storeDTO);

            // Act
            var result = await _adminStoreService.GetById(storeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(storeDTO.Id, result.Id);
            Assert.Equal(storeDTO.Name, result.Name);
        }
        #endregion

        #region UpdateStore Tests (CM-67)

        [Theory]
        [InlineData("4fa85f64-5717-4562-b3fc-2c963f66afa7", "Phuc Store", "Phuc Address", "0987654321", 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("3fa85f64-5717-4562-b3fc", "Phuc Store", "Phuc Address", "0987654321", 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Store", "Phuc Address", "0987654321", 0.1, true, "4fa85f64-5717-4562-b3fc-2c963f66afa7")]
        public async Task UpdateStoreAsync_WithInvalidInput_ShouldThrowExceptionOrReturnFalse(
            string id,
            string name,
            string address,
            string phoneNumber,
            double customTaxRate,
            bool isActive,
            string brandId)
        {
            // Arrange
            var validStoreId = Guid.TryParse(id, out var storeId);

            var dto = new StoreAdminDTO
            {
                Name = name,
                Address = address,
                PhoneNumber = phoneNumber,
                CustomTaxRate = customTaxRate,
                IsActive = isActive,
                BrandId = Guid.Parse(brandId)
            };

            // Setup store repository mock
            if (id == "3fa85f64-5717-4562-b3fc-2c963f66afa6")
            {
                var existingStore = new Store
                {
                    Id = storeId,
                    Name = "Existing Store",
                    IsDeleted = false
                };
                _mockStoreRepository.Setup(x => x.GetByIdAsync(storeId))
                    .ReturnsAsync(existingStore);
            }
            else if (id != null)
            {
                _mockStoreRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((Store)null);
            }

            // Setup brand repository mock
            var foundBrand = true;
            if (brandId == "3fa85f64-5717-4562-b3fc-2c963f66afa6")
            {
                var brand = new Brand { Id = Guid.Parse(brandId), Name = "Test Brand" };
                _mockBrandRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid?>()))
                    .ReturnsAsync(brand);
            }
            else if (brandId != "3fa85f64-5717-4562-b3fc-2c963f66afa6")
            {
                foundBrand = false;
                _mockBrandRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid?>()))
                    .ReturnsAsync((Brand)null);
            }


            // Act & Assert
            if (validStoreId)
            {
                if (foundBrand)
                {
                    Assert.False(await _adminStoreService.UpdateStoreAsync(storeId, dto, _validUserId));
                }
                else
                {
                    await Assert.ThrowsAsync<Exception>(() => _adminStoreService.UpdateStoreAsync(storeId, dto, _validUserId));
                }
            }
            else
            {
                await Assert.ThrowsAsync<FormatException>(() => _adminStoreService.UpdateStoreAsync(Guid.Parse(id), dto, _validUserId));
            }
        }


        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", null, "Phuc Address", "0987654321", 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Store", null, "0987654321", 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Store", "Phuc Address", "966777888", 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Store", "Phuc Address", null, 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Store", "Phuc Address", "0987654321", 0.0, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Store", "Phuc Address", "0987654321", 1.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Store", "Phuc Address", "0987654321", null, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task UpdateStoreAsync_WithInvalidInput_ShouldValidateFalse(
            string id,
            string name,
            string address,
            string phoneNumber,
            double customTaxRate,
            bool isActive,
            string brandId)
        {
            // Arrange
            var dto = new StoreAdminDTO
            {
                Name = name,
                Address = address,
                PhoneNumber = phoneNumber,
                CustomTaxRate = customTaxRate,
                IsActive = isActive,
                BrandId = Guid.Parse(brandId)
            };

            // Act
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);


            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Store", "Phuc Address", "0987654321", 0.1, true, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Store", "Phuc Address", "0987654321", 0.1, false, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Phuc Store", "Phuc Address", "0987654321", 0.1, null, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task UpdateStoreAsync_WithValidInput_ShouldReturnTrue(
            string idString,
            string name,
            string address,
            string phoneNumber,
            double customTaxRate,
            bool isActive,
            string brandId)
        {
            // Arrange
            var id = Guid.Parse(idString);

            var dto = new StoreAdminDTO
            {
                Name = name,
                Address = address,
                PhoneNumber = phoneNumber,
                CustomTaxRate = customTaxRate,
                IsActive = isActive,
                BrandId = Guid.Parse(brandId)
            };

            var brand = new Brand { Id = id, IsDelete = false };
            _mockBrandRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(brand);
            _mockBrandRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var store = new Store { Id = id, IsDeleted = false };
            _mockStoreRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(store);
            _mockStoreRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map(dto, store));

            // Act
            var result = await _adminStoreService.UpdateStoreAsync(id, dto, _validUserId);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region DeleteStore Tests (CM-68)

        [Theory]
        [InlineData("4fa85f64-5717-4562-b3fc-2c963f66afa7")]
        [InlineData("3fa85f64-5717-4562-b3fc")]

        public async Task DeleteStoreAsync_WithInvalidInput_ShouldThrowException(string id)
        {
            // Arrange
            _mockStoreRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Store)null);

            // Act & Assert
            if (Guid.TryParse(id, out var storeId))
            {
                Assert.False(await _adminStoreService.DeleteStoreAsync(storeId, _validUserId));
            }
            else
            {
                await Assert.ThrowsAsync<FormatException>(() => _adminStoreService.DeleteStoreAsync(Guid.Parse(id), _validUserId));
            }
        }

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task DeleteStoreAsync_WithValidId_ShouldReturnTrue(string idString)
        {
            // Arrange
            var id = Guid.Parse(idString);
            var store = new Store { Id = id, IsDeleted = false };
            _mockStoreRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(store);
            _mockStoreRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminStoreService.DeleteStoreAsync(id, _validUserId);

            // Assert
            Assert.True(result);
            Assert.True(store.IsDeleted);
            Assert.Equal(_validUserId, store.UpdatedBy);
        }

        #endregion
    }
}
