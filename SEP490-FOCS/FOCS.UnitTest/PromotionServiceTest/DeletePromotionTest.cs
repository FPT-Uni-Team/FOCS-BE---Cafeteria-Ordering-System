using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.Identity.Client;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.PromotionServiceTest
{
    public class DeletePromotionTest : PromotionServiceTestBase
    {
        [Fact]
        public async Task DeletePromotionAsync_WhenPromotionNotFound_ShouldReturnFalse()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            // Setup GetAvailablePromotionById to return null
            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync((Promotion)null);

            // Act
            var result = await _promotionService.DeletePromotionAsync(promotionId, userId);

            // Assert
            Assert.False(result);
            _promotionRepositoryMock.Verify(x => x.GetByIdAsync(promotionId), Times.Once);
        }

        [Fact]
        public async Task DeletePromotionAsync_WhenPromotionIsDeleted_ShouldReturnFalse()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = new Promotion
            {
                Id = promotionId,
                StoreId = storeId,
                IsDeleted = true,
                IsActive = false
            };

            // Setup GetAvailablePromotionById to return null (since promotion is deleted)
            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            // Act
            var result = await _promotionService.DeletePromotionAsync(promotionId, userId);

            // Assert
            Assert.False(result);
            _promotionRepositoryMock.Verify(x => x.GetByIdAsync(promotionId), Times.Once);
        }

        [Fact]
        public async Task DeletePromotionAsync_WhenUserNotFound_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = new Promotion
            {
                Id = promotionId,
                StoreId = storeId,
                IsDeleted = false,
                IsActive = false
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            // Setup user not found
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.DeletePromotionAsync(promotionId, userId));

            _promotionRepositoryMock.Verify(x => x.GetByIdAsync(promotionId), Times.Once);
            _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeletePromotionAsync_WhenUserUnauthorized_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var unauthorizedStoreId = Guid.NewGuid();

            var promotion = new Promotion
            {
                Id = promotionId,
                StoreId = storeId,
                IsDeleted = false,
                IsActive = false
            };

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = unauthorizedStoreId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.DeletePromotionAsync(promotionId, userId));

            _promotionRepositoryMock.Verify(x => x.GetByIdAsync(promotionId), Times.Once);
            _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeletePromotionAsync_WhenPromotionIsActive_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = new Promotion
            {
                Id = promotionId,
                StoreId = storeId,
                IsDeleted = false,
                IsActive = true // Active promotion cannot be deleted
            };

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.DeletePromotionAsync(promotionId, userId));

            _promotionRepositoryMock.Verify(x => x.GetByIdAsync(promotionId), Times.Once);
            _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeletePromotionAsync_WhenValidAndNoCoupons_ShouldMarkAsDeletedAndReturnTrue()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = new Promotion
            {
                Id = promotionId,
                StoreId = storeId,
                IsDeleted = false,
                IsActive = false
            };

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            // Setup no coupons associated with promotion
            _couponRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(new List<Coupon>());

            // Act
            var result = await _promotionService.DeletePromotionAsync(promotionId, userId);

            // Assert
            Assert.True(result);
            Assert.True(promotion.IsDeleted);
            Assert.Equal(userId, promotion.UpdatedBy);
            Assert.True(promotion.UpdatedAt > DateTime.MinValue);

            _promotionRepositoryMock.Verify(x => x.GetByIdAsync(promotionId), Times.Once);
            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _couponRepositoryMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()), Times.Once);
            _couponRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeletePromotionAsync_WhenValidWithCoupons_ShouldMarkAsDeletedAndUpdateCoupons()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = new Promotion
            {
                Id = promotionId,
                StoreId = storeId,
                IsDeleted = false,
                IsActive = false
            };

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            var coupons = new List<Coupon>
            {
                new Coupon
                {
                    Id = Guid.NewGuid(),
                    PromotionId = promotionId,
                    Code = "COUPON1",
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedBy = "old-user"
                },
                new Coupon
                {
                    Id = Guid.NewGuid(),
                    PromotionId = promotionId,
                    Code = "COUPON2",
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedBy = "old-user"
                }
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            // Setup coupons associated with promotion
            _couponRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(coupons);

            // Act
            var result = await _promotionService.DeletePromotionAsync(promotionId, userId);

            // Assert
            Assert.True(result);
            Assert.True(promotion.IsDeleted);
            Assert.Equal(userId, promotion.UpdatedBy);
            Assert.True(promotion.UpdatedAt > DateTime.MinValue);

            // Verify coupons were updated
            foreach (var coupon in coupons)
            {
                Assert.Null(coupon.PromotionId);
                Assert.Equal(userId, coupon.UpdatedBy);
                Assert.True(coupon.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
            }

            _promotionRepositoryMock.Verify(x => x.GetByIdAsync(promotionId), Times.Once);
            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _couponRepositoryMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()), Times.Once);
            _couponRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeletePromotionAsync_WhenValidWithEmptyCouponList_ShouldMarkAsDeletedAndReturnTrue()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = new Promotion
            {
                Id = promotionId,
                StoreId = storeId,
                IsDeleted = false,
                IsActive = false
            };

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            // Setup empty coupon list (different from null)
            var emptyCoupons = new List<Coupon>();
            _couponRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(emptyCoupons);

            // Act
            var result = await _promotionService.DeletePromotionAsync(promotionId, userId);

            // Assert
            Assert.True(result);
            Assert.True(promotion.IsDeleted);
            Assert.Equal(userId, promotion.UpdatedBy);
            Assert.True(promotion.UpdatedAt > DateTime.MinValue);

            _promotionRepositoryMock.Verify(x => x.GetByIdAsync(promotionId), Times.Once);
            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _couponRepositoryMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()), Times.Once);
            _couponRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeletePromotionAsync_WhenValidWithSingleCoupon_ShouldUpdateCouponCorrectly()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = new Promotion
            {
                Id = promotionId,
                StoreId = storeId,
                IsDeleted = false,
                IsActive = false
            };

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            var originalUpdatedAt = DateTime.UtcNow.AddDays(-1);
            var coupon = new Coupon
            {
                Id = Guid.NewGuid(),
                PromotionId = promotionId,
                Code = "SINGLE_COUPON",
                UpdatedAt = originalUpdatedAt,
                UpdatedBy = "previous-user"
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            _couponRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(new List<Coupon> { coupon });

            // Act
            var result = await _promotionService.DeletePromotionAsync(promotionId, userId);

            // Assert
            Assert.True(result);
            Assert.True(promotion.IsDeleted);
            Assert.Equal(userId, promotion.UpdatedBy);

            // Verify coupon was properly updated
            Assert.Null(coupon.PromotionId);
            Assert.Equal(userId, coupon.UpdatedBy);
            Assert.True(coupon.UpdatedAt > originalUpdatedAt);
            Assert.True(coupon.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));

            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _couponRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeletePromotionAsync_ShouldCallFindAsyncWithCorrectPredicate()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = new Promotion
            {
                Id = promotionId,
                StoreId = storeId,
                IsDeleted = false,
                IsActive = false
            };

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            _couponRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(new List<Coupon>());

            // Act
            await _promotionService.DeletePromotionAsync(promotionId, userId);

            // Assert
            _couponRepositoryMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task DeletePromotionAsync_ShouldSetCorrectAuditFields()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var beforeTestTime = DateTime.UtcNow;

            var promotion = new Promotion
            {
                Id = promotionId,
                StoreId = storeId,
                IsDeleted = false,
                IsActive = false,
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedBy = "old-user"
            };

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            _couponRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(new List<Coupon>());

            // Act
            var result = await _promotionService.DeletePromotionAsync(promotionId, userId);

            // Assert
            Assert.True(result);
            Assert.True(promotion.IsDeleted);
            Assert.Equal(userId, promotion.UpdatedBy);
            Assert.True(promotion.UpdatedAt >= beforeTestTime);
            Assert.True(promotion.UpdatedAt <= DateTime.UtcNow);
        }
    }
}