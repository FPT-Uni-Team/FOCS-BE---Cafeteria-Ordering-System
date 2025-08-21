using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using FOCS.UnitTest.CouponServiceTest;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.CouponServiceTest
{
    public class CreateCouponTests : CouponServiceTestBase
    {
        [Fact]
        public async Task CreateCoupon_ShouldSucceed_WhenDataIsValid()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var couponId = Guid.NewGuid();
            var dto = new CouponAdminDTO
            {
                Code = "COUPON123",
                CouponType = CouponType.Manual,
                Description = "Test Coupon",
                DiscountType = DiscountType.Percent,
                Value = 10,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxUsage = 100,
                IsActive = true,
                StoreId = storeId.ToString(),
            };

            SetupValidUserAndStore(userId, storeId);

            var coupon = new Coupon { Id = couponId, Code = dto.Code, StoreId = storeId };
            _couponRepositoryMock.Setup(x => x.AsQueryable()).Returns(BuildEmptyCouponDbSet());
            _mapperMock.Setup(m => m.Map<Coupon>(dto)).Returns(coupon);
            _mapperMock.Setup(m => m.Map<CouponAdminDTO>(coupon)).Returns(dto);
            _couponRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Coupon>())).Returns(Task.CompletedTask);

            var result = await _adminCouponService.CreateCouponAsync(dto, userId, storeId.ToString());

            Assert.NotNull(result);
            Assert.Equal(dto.Code, result.Code);
        }

        [Theory]
        [InlineData("UserNotFound", AdminCouponConstants.FieldUserId, null)]
        public async Task CreateCoupon_ShouldThrow_IfUserOrStoreInvalid(string error, string field, Store? store)
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            if (error == "UserNotFound")
                _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((User?)null);
            else
                SetupValidUser(userId, new User { Id = userId }, new List<UserStore> { new UserStore { StoreId = storeId, UserId = Guid.Parse(userId) } });

            if (error == "StoreNotFound")
                _storeRepositoryMock.Setup(s => s.GetByIdAsync(storeId)).ReturnsAsync(store);

            var dto = new CouponAdminDTO { StoreId = storeId.ToString() };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.CreateCouponAsync(dto, userId, storeId.ToString()));

            AssertConditionException(ex, Errors.Common.UserNotFound, field);
        }

        [Fact]
        public async Task CreateCoupon_ShouldThrow_WhenUserNotAuthorizedForStore()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            SetupValidUser(userId, new User { Id = userId }, new List<UserStore>());
            _storeRepositoryMock.Setup(s => s.GetByIdAsync(storeId)).ReturnsAsync(new Store { Id = storeId });

            var dto = new CouponAdminDTO { StoreId = storeId.ToString() };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.CreateCouponAsync(dto, userId, storeId.ToString()));

            AssertConditionException(ex, Errors.AuthError.UserUnauthor, AdminCouponConstants.FieldStoreId);
        }

        [Fact]
        public async Task CreateCoupon_ShouldGenerateCode_WhenAutoGenerateType()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var dto = new CouponAdminDTO
            {
                CouponType = CouponType.AutoGenerate,
                DiscountType = DiscountType.FixedAmount,
                Value = 20,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(3),
                MaxUsage = 50,
                StoreId = storeId.ToString(),
            };

            SetupValidUserAndStore(userId, storeId);
            _couponRepositoryMock.Setup(x => x.AsQueryable()).Returns(BuildEmptyCouponDbSet());
            _mapperMock.Setup(m => m.Map<Coupon>(dto)).Returns(new Coupon { StoreId = storeId });

            Coupon? capturedCoupon = null;
            _couponRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Coupon>()))
                .Callback<Coupon>(c => capturedCoupon = c)
                .Returns(Task.CompletedTask);

            _mapperMock.Setup(m => m.Map<CouponAdminDTO>(It.IsAny<Coupon>()))
                .Returns(() => new CouponAdminDTO { Code = capturedCoupon?.Code });

            var result = await _adminCouponService.CreateCouponAsync(dto, userId, storeId.ToString());

            Assert.False(string.IsNullOrEmpty(result.Code));
        }

        [Theory]
        [InlineData("EXISTING", CouponType.Manual, AdminCouponConstants.CheckCreateUniqueCode, AdminCouponConstants.FieldCode)]
        public async Task CreateCoupon_ShouldThrow_WhenCodeConflicts(string code, CouponType type, string expectedMsg, string expectedField)
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var dto = new CouponAdminDTO { Code = code, CouponType = type, StoreId = storeId.ToString() };

            SetupValidUserAndStore(userId, storeId);

            var existingCoupon = new Coupon { Code = code, IsDeleted = false };
            _couponRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(new List<Coupon> { existingCoupon }.AsQueryable().BuildMockDbSet().Object);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.CreateCouponAsync(dto, userId, storeId.ToString()));

            AssertConditionException(ex, expectedMsg, expectedField);
        }

        [Fact]
        public async Task CreateCoupon_ShouldThrow_WhenStartDateAfterEndDate()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var dto = new CouponAdminDTO
            {
                Code = "VALID",
                CouponType = CouponType.Manual,
                StoreId = storeId.ToString(),
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(1),
            };

            SetupValidUserAndStore(userId, storeId);
            _couponRepositoryMock.Setup(x => x.AsQueryable()).Returns(BuildEmptyCouponDbSet());

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.CreateCouponAsync(dto, userId, storeId.ToString()));

            AssertConditionException(ex, AdminCouponConstants.CheckCreateDate, AdminCouponConstants.FieldDate);
        }

        [Theory]
        [InlineData((CouponType)999, "", AdminCouponConstants.CheckCouponCodeType, AdminCouponConstants.FieldCouponType)]
        [InlineData(CouponType.AutoGenerate, "SHOULD_NOT_HAVE_THIS", AdminCouponConstants.CheckCouponCodeForAuto, AdminCouponConstants.FieldCode)]
        [InlineData(CouponType.Manual, "", AdminCouponConstants.CheckCouponCodeForManual, AdminCouponConstants.FieldCode)]
        public async Task CreateCoupon_ShouldThrow_WhenCouponTypeOrCodeInvalid(CouponType type, string code, string expectedMsg, string expectedField)
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var dto = new CouponAdminDTO { CouponType = type, Code = code, StoreId = storeId.ToString() };

            SetupValidUserAndStore(userId, storeId);
            _couponRepositoryMock.Setup(x => x.AsQueryable()).Returns(BuildEmptyCouponDbSet());

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.CreateCouponAsync(dto, userId, storeId.ToString()));

            AssertConditionException(ex, expectedMsg, expectedField);
        }

        [Fact]
        public async Task CreateCoupon_ShouldThrow_WhenPromotionNotFound()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var promotionId = Guid.NewGuid();
            var dto = new CouponAdminDTO
            {
                CouponType = CouponType.AutoGenerate,
                StoreId = storeId.ToString(),
                PromotionId = promotionId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            SetupValidUserAndStore(userId, storeId);

            _promotionRepositoryMock.Setup(p => p.AsQueryable())
                .Returns(new List<Promotion>().AsQueryable().BuildMockDbSet().Object);

            _couponRepositoryMock.Setup(x => x.AsQueryable()).Returns(BuildEmptyCouponDbSet());

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.CreateCouponAsync(dto, userId, storeId.ToString()));

            AssertConditionException(ex, AdminCouponConstants.CheckPromotion, AdminCouponConstants.FieldPromotionId);
        }

        [Fact]
        public async Task CreateCoupon_ShouldThrow_WhenDateOutOfPromotionRange()
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var promotionId = Guid.NewGuid();
            var dto = new CouponAdminDTO
            {
                CouponType = CouponType.AutoGenerate,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(6),
                PromotionId = promotionId,
                StoreId = storeId.ToString(),
            };

            var promotion = new Promotion
            {
                Id = promotionId,
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(5)
            };

            SetupValidUserAndStore(userId, storeId);

            _promotionRepositoryMock.Setup(p => p.AsQueryable())
                .Returns(new List<Promotion> { promotion }.AsQueryable().BuildMockDbSet().Object);

            _couponRepositoryMock.Setup(x => x.AsQueryable()).Returns(BuildEmptyCouponDbSet());
            _mapperMock.Setup(m => m.Map<Coupon>(It.IsAny<CouponAdminDTO>())).Returns(new Coupon { StoreId = storeId });

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.CreateCouponAsync(dto, userId, storeId.ToString()));

            AssertConditionException(ex, AdminCouponConstants.PromotionOutOfDate, AdminCouponConstants.FieldPromotionId);
        }
    }
}
