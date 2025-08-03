using AutoMapper;
using FOCS.Application.Services;
using FOCS.Common.Enums;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.StoreSettingServiceTest
{
    public abstract class StoreSettingServiceTestBase
    {
        protected readonly Mock<IRepository<StoreSetting>> _mockStoreSettingRepository;
        protected readonly Mock<IRepository<UserStore>> _mockUserStoreRepository;
        protected readonly Mock<UserManager<User>> _mockUserManager;
        protected readonly Mock<IMapper> _mockMapper;
        protected readonly StoreSettingService _storeSettingService;
        protected readonly string _validUserId = Guid.NewGuid().ToString();
        protected readonly Guid _testStoreId = Guid.NewGuid();

        protected StoreSettingServiceTestBase()
        {
            _mockStoreSettingRepository = new Mock<IRepository<StoreSetting>>();
            _mockUserStoreRepository = new Mock<IRepository<UserStore>>();
            _mockUserManager = CreateMockUserManager();
            _mockMapper = new Mock<IMapper>();

            _storeSettingService = new StoreSettingService(
                _mockStoreSettingRepository.Object,
                _mockUserStoreRepository.Object,
                _mockUserManager.Object,
                _mockMapper.Object);
        }

        private Mock<UserManager<User>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        protected StoreSettingDTO CreateValidStoreSettingDTO()
        {
            return new StoreSettingDTO
            {
                OpenTime = new TimeSpan(8, 0, 0),
                CloseTime = new TimeSpan(22, 0, 0),
                Currency = "USD",
                PaymentConfig = PaymentConfig.Momo,
                LogoUrl = "https://example.com/logo.png",
                IsSelfService = false,
                DiscountStrategy = DiscountStrategy.CouponThenPromotion
            };
        }

        protected StoreSetting CreateValidStoreSetting()
        {
            return new StoreSetting
            {
                Id = Guid.NewGuid(),
                StoreId = _testStoreId,
                OpenTime = new TimeSpan(8, 0, 0),
                CloseTime = new TimeSpan(22, 0, 0),
                Currency = "USD",
                PaymentConfig = PaymentConfig.Momo,
                LogoUrl = "https://example.com/logo.png",
                IsSelfService = false,
                discountStrategy = DiscountStrategy.CouponThenPromotion,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _validUserId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = _validUserId
            };
        }

        protected void SetupQueryableRepository(List<StoreSetting> storeSettings)
        {
            var queryable = storeSettings.AsQueryable().BuildMockDbSet();
            _mockStoreSettingRepository.Setup(r => r.AsQueryable())
                .Returns(queryable.Object);
        }
    }
}