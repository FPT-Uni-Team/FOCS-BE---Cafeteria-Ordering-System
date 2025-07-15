using AutoMapper;
using FOCS.Application.Services;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using MockQueryable.Moq;

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
    }
}
