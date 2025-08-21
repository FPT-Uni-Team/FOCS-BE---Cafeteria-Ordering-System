using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.CouponServiceTest
{
    public class UpdateCouponTests : CouponServiceTestBase
    {
        [Fact]
        public async Task UpdateCouponAsync_ReturnsFalse_WhenCouponMissingOrDeleted()
        {
            var id = Guid.NewGuid();

            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Coupon?)null);
            Assert.False(await _adminCouponService.UpdateCouponAsync(id, new CouponAdminDTO(), "user", Guid.NewGuid().ToString()));

            var deleted = new Coupon { Id = id, IsDeleted = true };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(deleted);
            Assert.False(await _adminCouponService.UpdateCouponAsync(id, new CouponAdminDTO(), "user", Guid.NewGuid().ToString()));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(999)]
        public async Task UpdateCouponAsync_Throws_WhenInvalidCouponType(int invalidType)
        {
            var id = Guid.NewGuid();
            var coupon = new Coupon { Id = id, IsDeleted = false };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(coupon);

            var dto = new CouponAdminDTO { CouponType = (CouponType)invalidType };
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.UpdateCouponAsync(id, dto, "user", Guid.NewGuid().ToString()));

            AssertConditionException(ex, AdminCouponConstants.CheckCouponCodeType, AdminCouponConstants.FieldCouponType);
        }

        [Fact]
        public async Task UpdateCouponAsync_UpdatesEndDate_ForOngoingCoupon()
        {
            var now = DateTime.UtcNow;
            var id = Guid.NewGuid();
            var coupon = new Coupon { Id = id, IsDeleted = false, StartDate = now.AddDays(-1), EndDate = now.AddDays(1) };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(coupon);

            var newEnd = now.AddDays(5);
            var dto = new CouponAdminDTO { CouponType = CouponType.Manual, EndDate = newEnd };

            var result = await _adminCouponService.UpdateCouponAsync(id, dto, "user", Guid.NewGuid().ToString());

            Assert.True(result);
            Assert.Equal(newEnd, coupon.EndDate);
            _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCouponAsync_Throws_WhenOngoingEndDateInPast()
        {
            var now = DateTime.UtcNow;
            var id = Guid.NewGuid();
            var coupon = new Coupon { Id = id, IsDeleted = false, StartDate = now.AddDays(-1), EndDate = now.AddDays(1) };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(coupon);

            var dto = new CouponAdminDTO { CouponType = CouponType.Manual, EndDate = now.AddDays(-5) };
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.UpdateCouponAsync(id, dto, "user", Guid.NewGuid().ToString()));

            AssertConditionException(ex, AdminCouponConstants.CheckUpdateDate, AdminCouponConstants.FieldDate);
        }

        [Fact]
        public async Task UpdateCouponAsync_KeepsCode_ForAutoGenerate()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var id = Guid.NewGuid();
            var storeId = Guid.NewGuid().ToString();

            // Make coupon non-On-going by setting StartDate in the future
            var coupon = new Coupon
            {
                Id = id,
                IsDeleted = false,
                StartDate = now.AddDays(5),  // future
                EndDate = now.AddDays(10),
                Code = "OLD",
                StoreId = Guid.Parse(storeId)
            };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(coupon);
            SetupCouponQueryable(new List<Coupon> { coupon });

            var dto = new CouponAdminDTO
            {
                CouponType = CouponType.AutoGenerate,
                Code = "NEW",       // this should be overwritten
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate
            };

            // Act
            var result = await _adminCouponService.UpdateCouponAsync(id, dto, "user", storeId);

            // Assert
            Assert.True(result);
            Assert.Equal("OLD", dto.Code);  // now dto.Code should be reset to coupon.Code
            _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCouponAsync_Throws_WhenManualCodeEmpty()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var id = Guid.NewGuid();
            var storeId = Guid.NewGuid().ToString();

            // Đặt StartDate > now để non-On-going
            var coupon = new Coupon
            {
                Id = id,
                IsDeleted = false,
                StartDate = now.AddDays(1),      // future
                EndDate = now.AddDays(5),
                Code = "X",
                StoreId = Guid.Parse(storeId)
            };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(coupon);
            SetupCouponQueryable(new List<Coupon> { coupon });

            var dto = new CouponAdminDTO
            {
                CouponType = CouponType.Manual,
                Code = " ",                // empty code
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.UpdateCouponAsync(id, dto, "user", storeId));

            AssertConditionException(
                ex,
                AdminCouponConstants.CheckCouponCodeForManual,
                AdminCouponConstants.FieldCode
            );
        }

        [Fact]
        public async Task UpdateCouponAsync_Throws_WhenDuplicateCodeExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var storeId = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;

            // Make coupon non-On-going by setting StartDate > now
            var coupon = new Coupon
            {
                Id = id,
                IsDeleted = false,
                StartDate = now.AddDays(5),     // bắt đầu sau now => non-ongoing
                EndDate = now.AddDays(10),
                Code = "A",
                StoreId = Guid.Parse(storeId)
            };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(coupon);

            // Mock danh sách coupon có một duplicate code khác ID
            var duplicate = new Coupon { Id = Guid.NewGuid(), Code = "DUP", IsDeleted = false };
            SetupCouponQueryable(new List<Coupon> { coupon, duplicate });

            var dto = new CouponAdminDTO
            {
                CouponType = CouponType.Manual,
                Code = "DUP",               // trùng với duplicate
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.UpdateCouponAsync(id, dto, "user", storeId));

            AssertConditionException(ex, AdminCouponConstants.CheckUpdateUniqueCode, AdminCouponConstants.FieldCode);
        }

        [Fact]
        public async Task UpdateCouponAsync_Throws_WhenDatesInvalid()
        {
            var id = Guid.NewGuid();
            var coupon = new Coupon { Id = id, IsDeleted = false, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(5) };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(coupon);
            SetupCouponQueryable(new List<Coupon> { coupon });

            var dto = new CouponAdminDTO { CouponType = CouponType.Manual, Code = "X", StartDate = DateTime.UtcNow.AddDays(10), EndDate = DateTime.UtcNow.AddDays(5) };
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.UpdateCouponAsync(id, dto, "user", coupon.StoreId.ToString()));

            AssertConditionException(ex, AdminCouponConstants.CheckUpdateDate, AdminCouponConstants.FieldDate);
        }

        [Fact]
        public async Task UpdateCouponAsync_Throws_WhenPromotionNotFoundOrOutOfRange()
        {
            // Arrange: coupon non-ongoing
            var id = Guid.NewGuid();
            var storeId = Guid.NewGuid().ToString();
            var coupon = SetupNonOngoingCoupon(id, out var now, storeId);

            // Case 1: Promotion not found
            var missingPromoId = Guid.NewGuid();
            var dto1 = new CouponAdminDTO
            {
                CouponType = CouponType.Manual,
                Code = "X",
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                PromotionId = missingPromoId,
                StoreId = storeId
            };
            await AssertPromotionExceptionAsync(
                id,
                dto1,
                AdminCouponConstants.CheckPromotion,
                AdminCouponConstants.FieldPromotionId,
                new List<Promotion>()         // repo empty
            );

            // Case 2: Promotion exists but dates out of range
            var validPromoId = Guid.NewGuid();
            var invalidPromo = new Promotion
            {
                Id = validPromoId,
                IsActive = true,
                IsDeleted = false,
                StartDate = now.AddDays(5),
                EndDate = now.AddDays(10)
            };
            var dto2 = new CouponAdminDTO
            {
                CouponType = CouponType.Manual,
                Code = "X",
                StartDate = now.AddDays(1),
                EndDate = now.AddDays(20),
                PromotionId = validPromoId,
                StoreId = storeId
            };
            await AssertPromotionExceptionAsync(
                id,
                dto2,
                AdminCouponConstants.PromotionOutOfDate,
                AdminCouponConstants.FieldPromotionId,
                new List<Promotion> { invalidPromo }
            );
        }

    }
}
