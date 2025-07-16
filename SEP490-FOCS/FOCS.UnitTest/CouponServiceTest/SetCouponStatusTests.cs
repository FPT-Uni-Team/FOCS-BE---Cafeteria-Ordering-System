using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.CouponServiceTest
{
    public class SetCouponStatusTests : CouponServiceTestBase
    {
        [Fact]
        public async Task SetCouponStatusAsync_ShouldReturnFalse_WhenCouponNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user-1";

            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Coupon?)null);

            // Act
            var result = await _adminCouponService.SetCouponStatusAsync(id, true, userId);

            // Assert
            Assert.False(result);
            _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task SetCouponStatusAsync_ShouldReturnFalse_WhenCouponIsDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user-2";
            var coupon = new Coupon { Id = id, IsDeleted = true };

            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(coupon);

            // Act
            var result = await _adminCouponService.SetCouponStatusAsync(id, false, userId);

            // Assert
            Assert.False(result);
            _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task SetCouponStatusAsync_ShouldUpdateStatusAndAuditFields_WhenValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "admin";
            var coupon = new Coupon
            {
                Id = id,
                IsDeleted = false,
                IsActive = false
            };

            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(coupon);
            _couponRepositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminCouponService.SetCouponStatusAsync(id, true, userId);

            // Assert
            Assert.True(result);
            Assert.True(coupon.IsActive);
            Assert.Equal(userId, coupon.UpdatedBy);
            Assert.NotNull(coupon.UpdatedAt);
            Assert.True((DateTime.UtcNow - coupon.UpdatedAt!.Value).TotalSeconds < 5);
            _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SetCouponStatusAsync_ShouldSetIsActiveFalse_WhenRequested()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "auditor";
            var coupon = new Coupon { Id = id, IsDeleted = false, IsActive = true };

            _couponRepositoryMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(coupon);
            _couponRepositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminCouponService.SetCouponStatusAsync(id, false, userId);

            // Assert
            Assert.True(result);
            Assert.False(coupon.IsActive);
            Assert.Equal(userId, coupon.UpdatedBy);
            Assert.NotNull(coupon.UpdatedAt);
        }
    }
}
