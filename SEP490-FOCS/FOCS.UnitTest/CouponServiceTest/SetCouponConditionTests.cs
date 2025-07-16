using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace FOCS.UnitTest.CouponServiceTest
{
    public class SetCouponConditionTests : CouponServiceTestBase
    {
        [Fact]
        public async Task SetCouponConditionAsync_ShouldThrow_WhenCouponNotFound()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            _couponRepositoryMock.Setup(x => x.GetByIdAsync(couponId))
                                 .ReturnsAsync((Coupon?)null);

            var request = new SetCouponConditionRequest
            {
                ConditionType = CouponConditionType.MinOrderAmount,
                Value = 100
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.SetCouponConditionAsync(couponId, request));

            AssertConditionException(ex, Errors.Common.NotFound, Errors.FieldName.CouponId);
        }

        [Fact]
        public async Task SetCouponConditionAsync_ShouldSetMinimumOrderAmount()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var coupon = new Coupon { Id = couponId };

            _couponRepositoryMock.Setup(x => x.GetByIdAsync(couponId))
                                 .ReturnsAsync(coupon);

            var request = new SetCouponConditionRequest
            {
                ConditionType = CouponConditionType.MinOrderAmount,
                Value = 200
            };

            // Act
            await _adminCouponService.SetCouponConditionAsync(couponId, request);

            // Assert
            Assert.Equal(200, coupon.MinimumOrderAmount);
            _couponRepositoryMock.Verify(x => x.Update(It.Is<Coupon>(c => c.MinimumOrderAmount == 200)), Times.Once);
            _couponRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SetCouponConditionAsync_ShouldSetMinimumItemQuantity()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var coupon = new Coupon { Id = couponId };

            _couponRepositoryMock.Setup(x => x.GetByIdAsync(couponId))
                                 .ReturnsAsync(coupon);

            var request = new SetCouponConditionRequest
            {
                ConditionType = CouponConditionType.MinItemsQuantity,
                Value = 3
            };

            // Act
            await _adminCouponService.SetCouponConditionAsync(couponId, request);

            // Assert
            Assert.Equal(3, coupon.MinimumItemQuantity);
            _couponRepositoryMock.Verify(x => x.Update(It.Is<Coupon>(c => c.MinimumItemQuantity == 3)), Times.Once);
            _couponRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SetCouponConditionAsync_ShouldLogError_WhenInvalidMinItemQuantity()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var coupon = new Coupon { Id = couponId };

            _couponRepositoryMock.Setup(x => x.GetByIdAsync(couponId))
                                 .ReturnsAsync(coupon);

            var request = new SetCouponConditionRequest
            {
                ConditionType = CouponConditionType.MinItemsQuantity,
                Value = double.NaN // invalid for int.TryParse
            };

            // Act
            await _adminCouponService.SetCouponConditionAsync(couponId, request);

            // Assert
            _couponRepositoryMock.Verify(x => x.Update(It.IsAny<Coupon>()), Times.Never);
            _couponRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
            _loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid MinimumItemQuantity value")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SetCouponConditionAsync_ShouldDoNothing_WhenConditionTypeIsUnknown()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var coupon = new Coupon { Id = couponId };

            _couponRepositoryMock.Setup(x => x.GetByIdAsync(couponId)).ReturnsAsync(coupon);

            var request = new SetCouponConditionRequest
            {
                ConditionType = (CouponConditionType)999, // unknown enum
                Value = 123
            };

            // Act
            await _adminCouponService.SetCouponConditionAsync(couponId, request);

            // Assert
            Assert.Null(coupon.MinimumItemQuantity);
            Assert.Null(coupon.MinimumOrderAmount);
            _couponRepositoryMock.Verify(x => x.Update(It.IsAny<Coupon>()), Times.Once);
            _couponRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SetCouponConditionAsync_ShouldLogError_WhenSaveChangesThrows()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var coupon = new Coupon { Id = couponId };

            _couponRepositoryMock.Setup(x => x.GetByIdAsync(couponId)).ReturnsAsync(coupon);

            _couponRepositoryMock.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception("Save error"));

            var request = new SetCouponConditionRequest
            {
                ConditionType = CouponConditionType.MinOrderAmount,
                Value = 999
            };

            // Act
            await _adminCouponService.SetCouponConditionAsync(couponId, request);

            // Assert
            _couponRepositoryMock.Verify(x => x.Update(It.Is<Coupon>(c => c.MinimumOrderAmount == 999)), Times.Once);
            _loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Save error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

    }
}
