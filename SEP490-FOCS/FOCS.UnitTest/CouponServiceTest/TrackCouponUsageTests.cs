using FOCS.Application.DTOs.AdminDTO;
using FOCS.Common.Constants;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.CouponServiceTest
{
    public class TrackCouponUsageTests : CouponServiceTestBase
    {
        [Fact]
        public async Task TrackCouponUsageAsync_ShouldThrow_WhenCouponNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Coupon?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _adminCouponService.TrackCouponUsageAsync(id));

            AssertConditionException(
                ex,
                AdminCouponConstants.CouponStatusNotFound,
                AdminCouponConstants.FieldCouponId);
        }

        [Fact]
        public async Task TrackCouponUsageAsync_ShouldThrow_WhenCouponDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var coupon = new Coupon { Id = id, IsDeleted = true };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(coupon);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _adminCouponService.TrackCouponUsageAsync(id));

            AssertConditionException(
                ex,
                AdminCouponConstants.CouponStatusNotFound,
                AdminCouponConstants.FieldCouponId);
        }

        [Fact]
        public async Task TrackCouponUsageAsync_ShouldThrow_WhenCouponInactive()
        {
            // Arrange
            var id = Guid.NewGuid();
            var coupon = new Coupon { Id = id, IsDeleted = false, IsActive = false };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(coupon);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _adminCouponService.TrackCouponUsageAsync(id));

            AssertConditionException(
                ex,
                AdminCouponConstants.TrackNotFound,
                AdminCouponConstants.FieldCouponId);
        }

        [Theory]
        [InlineData(-10, -5)] // now > EndDate
        [InlineData(5, 10)]   // now < StartDate
        public async Task TrackCouponUsageAsync_ShouldThrow_WhenOutsideDateRange(int startOffsetMinutes, int endOffsetMinutes)
        {
            // Arrange
            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var coupon = new Coupon
            {
                Id = id,
                IsDeleted = false,
                IsActive = true,
                Code = "TESTCODE",
                StartDate = now.AddMinutes(startOffsetMinutes),
                EndDate = now.AddMinutes(endOffsetMinutes)
            };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(coupon);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _adminCouponService.TrackCouponUsageAsync(id));

            AssertConditionException(
                ex,
                $"Coupon {coupon.Code} must be within the promotion period.",
                AdminCouponConstants.FieldDate);
        }

        [Fact]
        public async Task TrackCouponUsageAsync_ShouldThrow_WhenUsageLimitExceeded()
        {
            // Arrange
            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var coupon = new Coupon
            {
                Id = id,
                IsDeleted = false,
                IsActive = true,
                Code = "LIMIT",
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(1),
                MaxUsage = 2
            };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(coupon);

            var usages = new List<CouponUsage>
            {
                new CouponUsage { CouponId = id, OrderId = Guid.NewGuid(), UsedAt = now.AddHours(-1) },
                new CouponUsage { CouponId = id, OrderId = Guid.NewGuid(), UsedAt = now.AddHours(-2) }
            };
            _couponUsageRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(usages.AsQueryable().BuildMockDbSet().Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _adminCouponService.TrackCouponUsageAsync(id));

            AssertConditionException(
                ex,
                AdminCouponConstants.CouponUsageLimitExceed,
                AdminCouponConstants.FieldCouponId);
        }

        [Fact]
        public async Task TrackCouponUsageAsync_ShouldReturnDto_WhenValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var coupon = new Coupon
            {
                Id = id,
                IsDeleted = false,
                IsActive = true,
                Code = "OK",
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(1),
                MaxUsage = 5
            };
            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(coupon);

            var usages = new List<CouponUsage>
            {
                new CouponUsage { CouponId = id, OrderId = Guid.NewGuid(), UsedAt = now.AddHours(-1) },
                new CouponUsage { CouponId = id, OrderId = Guid.NewGuid(), UsedAt = now.AddHours(-2) }
            };
            _couponUsageRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(usages.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _adminCouponService.TrackCouponUsageAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalLeft);
            Assert.Equal(2, result.Usages.Count);
            Assert.All(result.Usages, u => Assert.IsType<TrackCouponUsageDTO.UsageInfo>(u));
        }
    }
}
