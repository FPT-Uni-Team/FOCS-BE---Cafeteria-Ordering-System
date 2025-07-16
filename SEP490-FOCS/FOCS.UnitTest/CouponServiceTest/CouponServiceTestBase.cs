using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.CouponServiceTest
{
    public class CouponServiceTestBase
    {
        protected readonly Mock<IRepository<Coupon>> _couponRepositoryMock;
        protected readonly Mock<IRepository<CouponUsage>> _couponUsageRepositoryMock;
        protected readonly Mock<IRepository<Promotion>> _promotionRepositoryMock;
        protected readonly Mock<IRepository<UserStore>> _userStoreRepositoryMock;
        protected readonly Mock<IRepository<Store>> _storeRepositoryMock;
        protected readonly Mock<UserManager<User>> _userManagerMock;
        protected readonly Mock<IMapper> _mapperMock;
        protected readonly Mock<ILogger<Coupon>> _loggerMock;
        protected readonly AdminCouponService _adminCouponService;

        public CouponServiceTestBase()
        {
            _couponRepositoryMock = new Mock<IRepository<Coupon>>();
            _couponUsageRepositoryMock = new Mock<IRepository<CouponUsage>>();
            _promotionRepositoryMock = new Mock<IRepository<Promotion>>();
            _userStoreRepositoryMock = new Mock<IRepository<UserStore>>();
            _storeRepositoryMock = new Mock<IRepository<Store>>();

            var userStore = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);

            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<Coupon>>();

            _adminCouponService = new AdminCouponService(
                _couponRepositoryMock.Object,
                _couponUsageRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _userStoreRepositoryMock.Object,
                _userManagerMock.Object,
                _loggerMock.Object,
                _promotionRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        protected void SetupCouponQueryable(List<Coupon> coupons)
        {
            _couponRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(coupons.AsQueryable().BuildMockDbSet().Object);

            _mapperMock.Setup(m => m.Map<List<CouponAdminDTO>>(It.IsAny<List<Coupon>>()))
                .Returns((List<Coupon> src) => src.Select(c => new CouponAdminDTO { Code = c.Code }).ToList());
        }

        protected void SetupValidUser(string userId, User user, List<UserStore>? userStores = null)
        {
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

            _userStoreRepositoryMock.Setup(x =>
                x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores ?? new List<UserStore>());
        }

        protected void SetupValidStore(Guid storeId, Store store)
        {
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId)).ReturnsAsync(store);
        }

        protected void SetupValidUserAndStore(string userId, Guid storeId)
        {
            var user = new User { Id = userId };
            var store = new Store { Id = storeId };
            var userStores = new List<UserStore>
            {
                new UserStore
                {
                    UserId = Guid.Parse(userId),
                    StoreId = storeId
                }
            };

            SetupValidUser(userId, user, userStores);
            SetupValidStore(storeId, store);
        }

        protected IQueryable<Coupon> BuildEmptyCouponDbSet()
        {
            return new List<Coupon>().AsQueryable().BuildMockDbSet().Object;
        }

        protected void AssertConditionException(Exception ex, string expectedMessage, string? expectedField = null)
        {
            Assert.NotNull(ex);
            var parts = ex.Message.Split('@');

            Assert.True(parts.Length >= 1, "Exception message format is invalid.");
            Assert.Equal(expectedMessage, parts[0]);

            if (expectedField != null)
            {
                Assert.True(parts.Length == 2, "Expected field name was not provided in exception message.");
                Assert.Equal(expectedField, parts[1]);
            }
        }

        protected Coupon SetupNonOngoingCoupon(Guid id, out DateTime now, string storeId, Guid? promoId = null)
        {
            now = DateTime.UtcNow;
            var coupon = new Coupon
            {
                Id = id,
                IsDeleted = false,
                StartDate = now.AddDays(1),
                EndDate = now.AddDays(5),
                PromotionId = promoId,
                StoreId = Guid.Parse(storeId)
            };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(coupon);
            SetupCouponQueryable(new List<Coupon> { coupon });
            return coupon;
        }

        protected async Task AssertPromotionExceptionAsync(Guid couponId, CouponAdminDTO dto, string expectedError, string expectedField, List<Promotion> promos)
        {
            _promotionRepositoryMock
              .Setup(r => r.AsQueryable())
              .Returns(promos.AsQueryable().BuildMockDbSet().Object);

            var ex = await Assert.ThrowsAsync<Exception>(
                () => _adminCouponService.UpdateCouponAsync(couponId, dto, "user", dto.StoreId));
            AssertConditionException(ex, expectedError, expectedField);
        }

    }
}
