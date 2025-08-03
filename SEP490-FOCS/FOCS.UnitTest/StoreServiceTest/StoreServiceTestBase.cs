using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.StoreServiceTest
{
    public abstract class StoreServiceTestBase
    {
        protected readonly Mock<IRepository<Store>> _mockStoreRepository;
        protected readonly Mock<IRepository<StoreSetting>> _mockStoreSettingRepository;
        protected readonly Mock<IMapper> _mockMapper;
        protected readonly AdminStoreService _adminStoreService;
        protected readonly string _validUserId = Guid.NewGuid().ToString();
        protected readonly Guid _testStoreId = Guid.NewGuid();

        protected StoreServiceTestBase()
        {
            _mockStoreRepository = new Mock<IRepository<Store>>();
            _mockStoreSettingRepository = new Mock<IRepository<StoreSetting>>();
            _mockMapper = new Mock<IMapper>();

            _adminStoreService = new AdminStoreService(
                _mockStoreRepository.Object,
                _mockStoreSettingRepository.Object,
                _mockMapper.Object);
        }

        protected StoreAdminDTO CreateValidStoreAdminDTO()
        {
            return new StoreAdminDTO
            {
                Name = "Test Store",
                Address = "123 Test Street",
                PhoneNumber = "123-456-7890",
                CustomTaxRate = 0.08
            };
        }

        protected Store CreateValidStore()
        {
            return new Store
            {
                Id = Guid.NewGuid(),
                Name = "Test Store",
                Address = "123 Test Street",
                PhoneNumber = "123-456-7890",
                CustomTaxRate = 0.08,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _validUserId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = _validUserId
            };
        }

        protected void SetupMapperForCreateStore(StoreAdminDTO dto, Store store)
        {
            _mockMapper.Setup(m => m.Map<Store>(dto))
                .Returns(store);

            _mockMapper.Setup(m => m.Map<StoreAdminDTO>(It.IsAny<Store>()))
                .Returns(dto);
        }

        protected void SetupRepositoryForSuccessfulCreate()
        {
            _mockStoreRepository.Setup(r => r.AddAsync(It.IsAny<Store>()))
                .Returns(Task.CompletedTask);

            _mockStoreRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockStoreSettingRepository.Setup(r => r.AddAsync(It.IsAny<StoreSetting>()))
                .Returns(Task.CompletedTask);

            _mockStoreSettingRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);
        }

        protected void SetupStoreQueryable(List<Store> stores)
        {
            var queryable = stores.AsQueryable().BuildMockDbSet();
            _mockStoreRepository.Setup(r => r.AsQueryable())
                .Returns(queryable.Object);
        }
    }
}
