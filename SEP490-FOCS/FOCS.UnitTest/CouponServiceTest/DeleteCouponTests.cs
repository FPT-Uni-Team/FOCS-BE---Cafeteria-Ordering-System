using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.CouponServiceTest
{
    public class DeleteCouponTests : CouponServiceTestBase
    {
        [Fact]
        public async Task DeleteCouponAsync_ShouldReturnFalse_WhenCouponNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user-id";

            _couponRepositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Coupon?)null);

            // Act
            var result = await _adminCouponService.DeleteCouponAsync(id, userId);

            // Assert
            Assert.False(result);
            _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteCouponAsync_ShouldReturnFalse_WhenCouponAlreadyDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user-id";
            var coupon = new Coupon { Id = id, IsDeleted = true };

            _couponRepositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(coupon);

            // Act
            var result = await _adminCouponService.DeleteCouponAsync(id, userId);

            // Assert
            Assert.False(result);
            _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteCouponAsync_ShouldReturnTrue_AndMarkDeleted_WithAuditFieldsSet()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "admin-user";
            var coupon = new Coupon { Id = id, IsDeleted = false };

            _couponRepositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(coupon);
            _couponRepositoryMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminCouponService.DeleteCouponAsync(id, userId);

            // Assert
            Assert.True(result);
            Assert.True(coupon.IsDeleted);
            Assert.Equal(userId, coupon.UpdatedBy);
            Assert.NotNull(coupon.UpdatedAt);
            Assert.True((DateTime.UtcNow - coupon.UpdatedAt!.Value).TotalSeconds < 5);
            _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCouponAsync_ShouldCallSaveChanges_WhenCouponIsValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "user123";
            var coupon = new Coupon { Id = id, IsDeleted = false };

            _couponRepositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(coupon);
            _couponRepositoryMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminCouponService.DeleteCouponAsync(id, userId);

            // Assert
            Assert.True(result);
            _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
