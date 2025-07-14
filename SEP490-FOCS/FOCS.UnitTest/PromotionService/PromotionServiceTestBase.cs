using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;

namespace FOCS.Tests.Application.Services
{
    public class PromotionServiceTestBase
    {
        protected readonly Mock<IRepository<Promotion>> _promotionRepositoryMock;
        protected readonly Mock<IRepository<Coupon>> _couponRepositoryMock;
        protected readonly Mock<IRepository<UserStore>> _userStoreRepositoryMock;
        protected readonly Mock<IRepository<CouponUsage>> _couponUsageRepositoryMock;
        protected readonly Mock<IRepository<PromotionItemCondition>> _promotionItemConditionRepositoryMock;
        protected readonly Mock<IRepository<Store>> _storeRepositoryMock;
        protected readonly Mock<IRepository<MenuItem>> _menuItemRepositoryMock;
        protected readonly Mock<IMapper> _mapperMock;
        protected readonly Mock<UserManager<User>> _userManagerMock;
        protected readonly Mock<IPricingService> _pricingServiceMock;
        protected readonly PromotionService _promotionService;

        public PromotionServiceTestBase()
        {
            _promotionRepositoryMock = new Mock<IRepository<Promotion>>();
            _couponRepositoryMock = new Mock<IRepository<Coupon>>();
            _userStoreRepositoryMock = new Mock<IRepository<UserStore>>();
            _couponUsageRepositoryMock = new Mock<IRepository<CouponUsage>>();
            _promotionItemConditionRepositoryMock = new Mock<IRepository<PromotionItemCondition>>();
            _storeRepositoryMock = new Mock<IRepository<Store>>();
            _menuItemRepositoryMock = new Mock<IRepository<MenuItem>>();
            _mapperMock = new Mock<IMapper>();
            _pricingServiceMock = new Mock<IPricingService>();

            // Mock UserManager
            var userStore = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            _promotionService = new PromotionService(
                _promotionRepositoryMock.Object,
                _promotionItemConditionRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _menuItemRepositoryMock.Object,
                _couponRepositoryMock.Object,
                _couponUsageRepositoryMock.Object,
                _userManagerMock.Object,
                _mapperMock.Object,
                _userStoreRepositoryMock.Object,
                _pricingServiceMock.Object
            );
        }

        // Helper methods
        protected Promotion CreateValidPromotion(Guid storeId)
        {
            return new Promotion
            {
                Id = Guid.NewGuid(),
                Title = "Test Promotion",
                StoreId = storeId,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                PromotionType = PromotionType.FixedAmount,
                DiscountValue = 10,
                IsActive = true,
                IsDeleted = false,
                PromotionItemConditions = new List<PromotionItemCondition>(),
                Coupons = new List<Coupon>()
            };
        }

        protected PromotionDTO CreateValidPromotionDTO(List<Guid> couponIds = null)
        {
            return new PromotionDTO
            {
                Id = Guid.NewGuid(),
                Title = "Test Promotion",
                PromotionType = PromotionType.FixedAmount,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountValue = 10,
                IsActive = true,
                CouponIds = couponIds ?? new List<Guid>(),
                AcceptForItems = new List<Guid>()
            };
        }

        protected void SetupPromotionQueryable(List<Promotion> promotions)
        {
            var promotionQueryable = promotions.AsQueryable().BuildMockDbSet();
            _promotionRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(promotionQueryable.Object);
        }

        protected void SetupValidUser(string userId, User user, UserStore? userStore = null)
        {
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userStoreRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStore != null ? new List<UserStore> { userStore } : []);
        }

        protected void SetupValidStore(Guid storeId, Store store)
        {
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync(store);
        }

        protected void SetupValidCoupon(Guid couponId, Guid storeId, Coupon coupon)
        {
            var couponQueryable = new List<Coupon> { coupon }.AsQueryable().BuildMockDbSet();
            _couponRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(couponQueryable.Object);
        }

        protected void SetupValidPromotionUniqueness(PromotionDTO dto, Guid storeId)
        {
            var titleCheckQueryable = new List<Promotion>().AsQueryable().BuildMockDbSet();
            var overlapCheckQueryable = new List<Promotion>().AsQueryable().BuildMockDbSet();

            _promotionRepositoryMock.SetupSequence(x => x.AsQueryable())
                .Returns(titleCheckQueryable.Object)
                .Returns(overlapCheckQueryable.Object);
        }

        protected void SetupMapperForCreation(PromotionDTO dto, Promotion entity, PromotionDTO resultDto)
        {
            _mapperMock.Setup(x => x.Map<Promotion>(dto))
                .Returns(entity);
            _mapperMock.Setup(x => x.Map<PromotionDTO>(It.IsAny<Promotion>()))
                .Returns(resultDto);
        }
    }
}