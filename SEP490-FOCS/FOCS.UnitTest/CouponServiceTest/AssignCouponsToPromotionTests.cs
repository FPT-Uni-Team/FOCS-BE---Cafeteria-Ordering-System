using FOCS.Common.Constants;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.CouponServiceTest
{
    public class AssignCouponsToPromotionTests : CouponServiceTestBase
    {
        [Fact]
        public async Task AssignCouponsToPromotionAsync_ShouldThrow_WhenPromotionNotFound()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var couponIds = new List<Guid> { Guid.NewGuid() };

            _promotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId))
                .ReturnsAsync((Promotion?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _adminCouponService.AssignCouponsToPromotionAsync(couponIds, promotionId, "user", Guid.NewGuid()));
            AssertConditionException(
                ex,
                AdminCouponConstants.CheckPromotion,
                AdminCouponConstants.FieldPromotionId);
        }

        [Fact]
        public async Task AssignCouponsToPromotionAsync_ShouldThrow_WhenPromotionDeleted()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var promotion = new Promotion { Id = promotionId, IsDeleted = true };
            _promotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _adminCouponService.AssignCouponsToPromotionAsync(new List<Guid>(), promotionId, "user", Guid.NewGuid()));
            AssertConditionException(
                ex,
                AdminCouponConstants.CheckPromotion,
                AdminCouponConstants.FieldPromotionId);
        }

        [Fact]
        public async Task AssignCouponsToPromotionAsync_ShouldReturnTrue_WhenNoCouponsFound()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var promotion = new Promotion { Id = promotionId, IsDeleted = false, StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1) };
            var storeId = Guid.NewGuid();

            _promotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);
            _couponRepositoryMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(new List<Coupon>());
            _couponRepositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _adminCouponService.AssignCouponsToPromotionAsync(new List<Guid> { Guid.NewGuid() }, promotionId, "user", storeId);

            // Assert
            Assert.True(result);
            _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AssignCouponsToPromotionAsync_ShouldThrow_WhenCouponStoreMismatch()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var storeId = Guid.NewGuid();
            var promotion = new Promotion { Id = promotionId, IsDeleted = false, StartDate = DateTime.UtcNow.AddDays(-5), EndDate = DateTime.UtcNow.AddDays(5) };
            var coupon = new Coupon { Id = Guid.NewGuid(), Code = "C1", IsDeleted = false, StoreId = Guid.NewGuid() };

            _promotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);
            _couponRepositoryMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(new List<Coupon> { coupon });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _adminCouponService.AssignCouponsToPromotionAsync(new List<Guid> { coupon.Id }, promotionId, "user", storeId));
            AssertConditionException(
                ex,
                $"Coupon {coupon.Code} does not belong to this store.",
                AdminCouponConstants.FieldStoreId);
        }

        [Fact]
        public async Task AssignCouponsToPromotionAsync_ShouldThrow_WhenCouponDateOutOfPromotionRange()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var storeId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var promotion = new Promotion { Id = promotionId, IsDeleted = false, StartDate = now, EndDate = now.AddDays(1) };
            var coupon = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "C2",
                IsDeleted = false,
                StoreId = storeId,
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(2)
            };

            _promotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);
            _couponRepositoryMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(new List<Coupon> { coupon });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _adminCouponService.AssignCouponsToPromotionAsync(new List<Guid> { coupon.Id }, promotionId, "user", storeId));
            AssertConditionException(
                ex,
                $"Coupon {coupon.Code} must be within the promotion period.",
                AdminCouponConstants.FieldDate);
        }

        [Fact]
        public async Task AssignCouponsToPromotionAsync_ShouldAssignPromotionIdAndAuditFields_WhenValid()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var storeId = Guid.NewGuid();
            var userId = "user-123";
            var now = DateTime.UtcNow;
            var promotion = new Promotion { Id = promotionId, IsDeleted = false, StartDate = now.AddDays(-2), EndDate = now.AddDays(2) };
            var coupon1 = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "A1",
                IsDeleted = false,
                StoreId = storeId,
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(1)
            };
            var coupon2 = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = "A2",
                IsDeleted = false,
                StoreId = storeId,
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(1)
            };

            _promotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);
            _couponRepositoryMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(new List<Coupon> { coupon1, coupon2 });
            _couponRepositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(2);

            // Act
            var result = await _adminCouponService.AssignCouponsToPromotionAsync(
                new List<Guid> { coupon1.Id, coupon2.Id },
                promotionId,
                userId,
                storeId);

            // Assert
            Assert.True(result);
            foreach (var c in new[] { coupon1, coupon2 })
            {
                Assert.Equal(promotionId, c.PromotionId);
                Assert.Equal(userId, c.UpdatedBy);
                Assert.NotNull(c.UpdatedAt);
                Assert.True((DateTime.UtcNow - c.UpdatedAt!.Value).TotalSeconds < 5);
            }
            _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
