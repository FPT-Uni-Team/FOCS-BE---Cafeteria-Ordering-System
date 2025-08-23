using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.BrandServiceTest
{
    public abstract class BrandServiceTestBase
    {
        protected readonly Mock<IRepository<Brand>> _mockBrandRepository;
        protected readonly Mock<IMapper> _mockMapper;
        protected readonly AdminBrandService _adminBrandService;
        protected readonly string _validUserId = Guid.NewGuid().ToString();
        protected readonly Guid _validBrandId = Guid.NewGuid();

        protected BrandServiceTestBase()
        {
            _mockBrandRepository = new Mock<IRepository<Brand>>();
            _mockMapper = new Mock<IMapper>();
            _adminBrandService = new AdminBrandService(_mockBrandRepository.Object, _mockMapper.Object);
        }

        protected CreateAdminBrandRequest CreateValidBrandRequest()
        {
            return new CreateAdminBrandRequest
            {
                Name = "Test Brand",
                DefaultTaxRate = 0.1,
                IsActive = true
            };
        }

        protected Brand CreateValidBrand(bool isDeleted = false, string createdBy = null)
        {
            return new Brand
            {
                Id = _validBrandId,
                Name = "Test Brand",
                DefaultTaxRate = 0.1,
                IsActive = true,
                IsDelete = isDeleted,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy ?? _validUserId
            };
        }

        protected BrandAdminDTO CreateValidBrandDTO()
        {
            return new BrandAdminDTO
            {
                Id = Guid.NewGuid(),
                Name = "Test Brand",
                DefaultTaxRate = 0.1,
                IsActive = true
            };
        }
        protected UrlQueryParameters CreateValidQueryParameters()
        {
            return new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SearchBy = "",
                SearchValue = "",
                SortBy = "",
                SortOrder = ""
            };
        }

        protected List<Brand> CreateBrandList(int count = 3)
        {
            var brands = new List<Brand>();
            for (int i = 1; i <= count; i++)
            {
                brands.Add(new Brand
                {
                    Id = Guid.NewGuid(),
                    Name = $"Brand {i}",
                    DefaultTaxRate = 0.1 + (i * 0.01),
                    IsActive = true,
                    IsDelete = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    CreatedBy = _validUserId
                });
            }
            return brands;
        }

        protected List<BrandAdminDTO> CreateBrandDTOList(int count = 3)
        {
            var dtos = new List<BrandAdminDTO>();
            for (int i = 1; i <= count; i++)
            {
                dtos.Add(new BrandAdminDTO
                {
                    Id = Guid.NewGuid(),
                    Name = $"Brand {i}",
                    DefaultTaxRate = 0.1 + (i * 0.01),
                    IsActive = true,
                });
            }
            return dtos;
        }

        protected void SetupMapperMocks(CreateAdminBrandRequest request, Brand brand, BrandAdminDTO dto)
        {
            _mockMapper.Setup(m => m.Map<Brand>(request)).Returns(brand);
            _mockMapper.Setup(m => m.Map<BrandAdminDTO>(brand)).Returns(dto);
        }

        protected void SetupRepositoryMocks()
        {
            _mockBrandRepository.Setup(r => r.AddAsync(It.IsAny<Brand>())).Returns(Task.CompletedTask);
            _mockBrandRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        }

        protected void SetupBrandQueryable(List<Brand> brands)
        {
            var brandQueryable = brands.AsQueryable().BuildMockDbSet();
            _mockBrandRepository.Setup(b => b.AsQueryable())
                .Returns(brandQueryable.Object);
        }
    }
}