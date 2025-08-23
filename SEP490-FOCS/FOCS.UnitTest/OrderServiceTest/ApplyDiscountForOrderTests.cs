using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.OrderServiceTest
{
    public class ApplyDiscountForOrderTests : OrderServiceTestBase
    {
        [Fact]
        public async Task ApplyDiscountForOrder_ShouldThrow_WhenCouponCodeNull()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var request = new ApplyDiscountOrderRequest
            {
                StoreId = _validStoreId,
                CouponCode = null!
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () =>
                await _orderService.ApplyDiscountForOrder(request, _validUserId, storeId));

            Assert.Equal(Errors.Common.NotFound + "@", ex.Message);
        }

        [Fact]
        public async Task ApplyDiscountForOrder_ShouldThrow_WhenPromotionInvalid()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var request = new ApplyDiscountOrderRequest
            {
                StoreId = _validStoreId,
                CouponCode = "BADCODE"
            };

            // PromotionService trả exception khi coupon không hợp lệ
            _mockPromotionService
                .Setup(p => p.IsValidPromotionCouponAsync(
                    request.CouponCode!,
                    _validUserId,
                    request.StoreId))
                .ThrowsAsync(new Exception(Errors.Common.NotFound));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () =>
                await _orderService.ApplyDiscountForOrder(request, _validUserId, storeId));

            Assert.Equal(Errors.Common.NotFound, ex.Message);
        }

        [Fact]
        public async Task ApplyDiscountForOrder_ShouldThrow_WhenStoreSettingNotFound()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var request = new ApplyDiscountOrderRequest
            {
                StoreId = _validStoreId,
                CouponCode = "GOODCODE"
            };

            // Promotion hợp lệ
            _mockPromotionService
                .Setup(p => p.IsValidPromotionCouponAsync(
                    request.CouponCode!,
                    _validUserId,
                    request.StoreId))
                .Returns(Task.CompletedTask);

            // StoreSetting null
            _mockStoreSettingService
                .Setup(s => s.GetStoreSettingAsync(request.StoreId, _validUserId))
                .ReturnsAsync((StoreSettingDTO)null!);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () =>
                await _orderService.ApplyDiscountForOrder(request, _validUserId, storeId));

            Assert.Equal(Errors.Common.NotFound + "@", ex.Message);
        }


    }
}
