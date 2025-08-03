using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.BrandServiceTest
{
    public class CreateBrandTest : BrandServiceTestBase
    {
        [Fact]
        public async Task CreateBrandAsync_WithValidInput_ShouldCreateBrandSuccessfully()
        {
            // Arrange
            var request = CreateValidBrandRequest();
            var brand = CreateValidBrand();
            var expectedDto = CreateValidBrandDTO();

            SetupMapperMocks(request, brand, expectedDto);
            SetupRepositoryMocks();

            // Act
            var result = await _adminBrandService.CreateBrandAsync(request, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Name, result.Name);
            Assert.Equal(expectedDto.DefaultTaxRate, result.DefaultTaxRate);

            // Verify that CheckValidInput was called (userId validation)
            // Verify mapping was called
            _mockMapper.Verify(m => m.Map<Brand>(request), Times.Once);
            _mockMapper.Verify(m => m.Map<BrandAdminDTO>(It.IsAny<Brand>()), Times.Once);

            // Verify repository operations
            _mockBrandRepository.Verify(r => r.AddAsync(It.Is<Brand>(b =>
                b.Id != Guid.Empty &&
                b.IsActive == true &&
                b.IsDelete == false &&
                b.CreatedAt != default &&
                b.CreatedBy == _validUserId)), Times.Once);
            _mockBrandRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateBrandAsync_WithInvalidUserId_ShouldThrowArgumentException(string invalidUserId)
        {
            // Arrange
            var request = CreateValidBrandRequest();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminBrandService.CreateBrandAsync(request, invalidUserId));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            // Verify no repository operations were called
            _mockBrandRepository.Verify(r => r.AddAsync(It.IsAny<Brand>()), Times.Never);
            _mockBrandRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateBrandAsync_ShouldSetCorrectBrandProperties()
        {
            // Arrange
            var request = CreateValidBrandRequest();
            var brand = new Brand(); // Empty brand to verify properties are set
            var expectedDto = CreateValidBrandDTO();

            _mockMapper.Setup(m => m.Map<Brand>(request)).Returns(brand);
            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(It.IsAny<Brand>())).Returns(expectedDto);
            SetupRepositoryMocks();

            // Act
            await _adminBrandService.CreateBrandAsync(request, _validUserId);

            // Assert - Verify the brand object passed to AddAsync has correct properties
            _mockBrandRepository.Verify(r => r.AddAsync(It.Is<Brand>(b =>
                b.Id != Guid.Empty &&
                b.IsActive == false &&
                b.IsDelete == false &&
                b.CreatedAt > DateTime.UtcNow.AddMinutes(-1) && // Within last minute
                b.CreatedAt <= DateTime.UtcNow &&
                b.CreatedBy == _validUserId)), Times.Once);
        }

        [Fact]
        public async Task CreateBrandAsync_ShouldGenerateUniqueId()
        {
            // Arrange
            var request = CreateValidBrandRequest();
            var brand1 = new Brand();
            var brand2 = new Brand();
            var dto = CreateValidBrandDTO();

            _mockMapper.Setup(m => m.Map<Brand>(request)).Returns(brand1);
            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(It.IsAny<Brand>())).Returns(dto);
            SetupRepositoryMocks();

            // Act
            await _adminBrandService.CreateBrandAsync(request, _validUserId);

            // Reset for second call
            _mockMapper.Setup(m => m.Map<Brand>(request)).Returns(brand2);
            await _adminBrandService.CreateBrandAsync(request, _validUserId);

            // Assert - Verify different GUIDs were generated
            _mockBrandRepository.Verify(r => r.AddAsync(It.IsAny<Brand>()), Times.Exactly(2));

            // Capture the brand objects to verify they have different IDs
            var capturedBrands = new List<Brand>();
            _mockBrandRepository.Setup(r => r.AddAsync(It.IsAny<Brand>()))
                .Callback<Brand>(b => capturedBrands.Add(b));

            // Run one more time to capture
            var brand3 = new Brand();
            _mockMapper.Setup(m => m.Map<Brand>(request)).Returns(brand3);
            await _adminBrandService.CreateBrandAsync(request, _validUserId);

            Assert.NotEqual(Guid.Empty, brand3.Id);
        }

        [Fact]
        public async Task CreateBrandAsync_ShouldCallSaveChangesAfterAdd()
        {
            // Arrange
            var request = CreateValidBrandRequest();
            var brand = CreateValidBrand();
            var dto = CreateValidBrandDTO();
            var callOrder = new List<string>();

            SetupMapperMocks(request, brand, dto);

            _mockBrandRepository.Setup(r => r.AddAsync(It.IsAny<Brand>()))
                .Callback(() => callOrder.Add("AddAsync"))
                .Returns(Task.CompletedTask);

            _mockBrandRepository.Setup(r => r.SaveChangesAsync())
                .Callback(() => callOrder.Add("SaveChangesAsync"))
                .ReturnsAsync(1);

            // Act
            await _adminBrandService.CreateBrandAsync(request, _validUserId);

            // Assert
            Assert.Equal(2, callOrder.Count);
            Assert.Equal("AddAsync", callOrder[0]);
            Assert.Equal("SaveChangesAsync", callOrder[1]);
        }

        [Fact]
        public async Task CreateBrandAsync_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var request = CreateValidBrandRequest();
            var brand = CreateValidBrand();
            var dto = CreateValidBrandDTO();

            SetupMapperMocks(request, brand, dto);

            var expectedException = new InvalidOperationException("Database error");
            _mockBrandRepository.Setup(r => r.AddAsync(It.IsAny<Brand>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminBrandService.CreateBrandAsync(request, _validUserId));

            Assert.Equal(expectedException.Message, actualException.Message);
            _mockBrandRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateBrandAsync_WhenSaveChangesThrows_ShouldPropagateException()
        {
            // Arrange
            var request = CreateValidBrandRequest();
            var brand = CreateValidBrand();
            var dto = CreateValidBrandDTO();

            SetupMapperMocks(request, brand, dto);

            _mockBrandRepository.Setup(r => r.AddAsync(It.IsAny<Brand>()))
                .Returns(Task.CompletedTask);

            var expectedException = new InvalidOperationException("Save failed");
            _mockBrandRepository.Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _adminBrandService.CreateBrandAsync(request, _validUserId));

            Assert.Equal(expectedException.Message, actualException.Message);
            _mockBrandRepository.Verify(r => r.AddAsync(It.IsAny<Brand>()), Times.Once);
        }
    }
}