using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace FOCS.UnitTest.BrandServiceTest
{
    public class GetBrandDetailTest : BrandServiceTestBase
    {
        [Fact]
        public async Task GetBrandDetailAsync_WithValidBrand_ShouldReturnBrandDTO()
        {
            // Arrange
            var existingBrand = CreateValidBrand();
            var expectedDto = CreateValidBrandDTO();

            SetupBrandQueryable(new List<Brand> { existingBrand });

            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(existingBrand))
                .Returns(expectedDto);

            // Act
            var result = await _adminBrandService.GetBrandDetailAsync(_validBrandId, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.Name, result.Name);
            Assert.Equal(expectedDto.DefaultTaxRate, result.DefaultTaxRate);

            // Verify repository and mapper calls
            _mockBrandRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<BrandAdminDTO>(existingBrand), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetBrandDetailAsync_WithInvalidUserId_ShouldThrowArgumentException(string invalidUserId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _adminBrandService.GetBrandDetailAsync(_validBrandId, invalidUserId));

            Assert.Equal("UserId is required(Please login).", exception.Message);

            // Verify no repository operations were called
            _mockBrandRepository.Verify(r => r.AsQueryable(), Times.Never);
            _mockMapper.Verify(m => m.Map<BrandAdminDTO>(It.IsAny<Brand>()), Times.Never);
        }

        [Fact]
        public async Task GetBrandDetailAsync_WithNonExistentBrand_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var existingBrand = CreateValidBrand();

            SetupBrandQueryable(new List<Brand> { existingBrand });

            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(It.IsAny<Brand>()))
                .Returns((BrandAdminDTO)null);

            // Act
            var result = await _adminBrandService.GetBrandDetailAsync(nonExistentId, _validUserId);

            // Assert
            Assert.Null(result);

            // Verify repository was called but mapper was called with null
            _mockBrandRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<BrandAdminDTO>(null), Times.Once);
        }

        [Fact]
        public async Task GetBrandDetailAsync_WithDeletedBrand_ShouldReturnNull()
        {
            // Arrange
            var deletedBrand = CreateValidBrand(true); // isDeleted = true

            SetupBrandQueryable(new List<Brand> { deletedBrand });

            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(It.IsAny<Brand>()))
                .Returns((BrandAdminDTO)null);

            // Act
            var result = await _adminBrandService.GetBrandDetailAsync(_validBrandId, _validUserId);

            // Assert
            Assert.Null(result);

            // Verify repository was called but mapper was called with null (because brand was filtered out)
            _mockBrandRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<BrandAdminDTO>(null), Times.Once);
        }

        [Fact]
        public async Task GetBrandDetailAsync_WithDifferentUserBrand_ShouldReturnNull()
        {
            // Arrange
            var otherUserBrand = CreateValidBrand(false, Guid.NewGuid().ToString());

            SetupBrandQueryable(new List<Brand> { otherUserBrand });

            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(It.IsAny<Brand>()))
                .Returns((BrandAdminDTO)null);

            // Act
            var result = await _adminBrandService.GetBrandDetailAsync(_validBrandId, _validUserId);

            // Assert
            Assert.Null(result);

            // Verify repository was called but mapper was called with null (because brand was filtered out)
            _mockBrandRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<BrandAdminDTO>(null), Times.Once);
        }

        [Fact]
        public async Task GetBrandDetailAsync_ShouldFilterByIdAndNotDeletedAndCreatedBy()
        {
            // Arrange
            var targetBrand = CreateValidBrand(false, _validUserId);
            var deletedBrand = CreateValidBrand(true, _validUserId);
            var otherUserBrand = CreateValidBrand(false, Guid.NewGuid().ToString());
            var differentIdBrand = new Brand
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                CreatedBy = _validUserId
            };

            var brands = new List<Brand> { targetBrand, deletedBrand, otherUserBrand, differentIdBrand };

            SetupBrandQueryable(brands);

            var expectedDto = CreateValidBrandDTO();
            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(targetBrand))
                .Returns(expectedDto);

            // Act
            var result = await _adminBrandService.GetBrandDetailAsync(_validBrandId, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto, result);

            // Verify only the target brand was mapped (others were filtered out)
            _mockMapper.Verify(m => m.Map<BrandAdminDTO>(targetBrand), Times.Once);
        }

        [Fact]
        public async Task GetBrandDetailAsync_WithEmptyRepository_ShouldReturnNull()
        {
            // Arrange
            SetupBrandQueryable(new List<Brand>());

            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(It.IsAny<Brand>()))
                .Returns((BrandAdminDTO)null);

            // Act
            var result = await _adminBrandService.GetBrandDetailAsync(_validBrandId, _validUserId);

            // Assert
            Assert.Null(result);

            _mockBrandRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<BrandAdminDTO>(null), Times.Once);
        }

        [Fact]
        public async Task GetBrandDetailAsync_ShouldUseExactIdComparison()
        {
            // Arrange
            var exactMatchBrand = CreateValidBrand(false, _validUserId);
            var similarIdBrand = new Brand
            {
                Id = Guid.NewGuid(), // Different GUID
                IsDelete = false,
                CreatedBy = _validUserId
            };

            var brands = new List<Brand> { exactMatchBrand, similarIdBrand };

            SetupBrandQueryable(brands);

            var expectedDto = CreateValidBrandDTO();
            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(exactMatchBrand))
                .Returns(expectedDto);

            // Act
            var result = await _adminBrandService.GetBrandDetailAsync(_validBrandId, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto, result);

            // Verify only exact match was processed
            _mockMapper.Verify(m => m.Map<BrandAdminDTO>(exactMatchBrand), Times.Once);
        }

        [Fact]
        public async Task GetBrandDetailAsync_WhenMapperReturnsNull_ShouldReturnNull()
        {
            // Arrange
            var existingBrand = CreateValidBrand();

            SetupBrandQueryable(new List<Brand> { existingBrand });

            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(existingBrand))
                .Returns((BrandAdminDTO)null);

            // Act
            var result = await _adminBrandService.GetBrandDetailAsync(_validBrandId, _validUserId);

            // Assert
            Assert.Null(result);

            _mockBrandRepository.Verify(r => r.AsQueryable(), Times.Once);
            _mockMapper.Verify(m => m.Map<BrandAdminDTO>(existingBrand), Times.Once);
        }
    }
}
