using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Enums;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;

namespace FOCS.UnitTest.CouponServiceTest
{
    public class GetCouponByIdTests : CouponServiceTestBase
    {
        [Fact]
        public async Task GetCouponByIdAsync_ShouldReturnCoupon_WhenCouponExists()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var couponId = Guid.NewGuid();

            var coupon = new Coupon
            {
                Id = couponId,
                Code = "DISCOUNT10",
                Value = 10,
                IsActive = true,
                IsDeleted = false,
                StoreId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                DiscountType = DiscountType.Percent,
                MaxUsage = 100,
                CountUsed = 10
            };

            var mock = new List<Coupon> { coupon }.AsQueryable().BuildMockDbSet();
            _couponRepositoryMock.Setup(x => x.AsQueryable()).Returns(mock.Object);

            _mapperMock.Setup(x => x.Map<CouponAdminDTO>(coupon)).Returns(new CouponAdminDTO
            {
                Id = couponId,
                Code = coupon.Code,
                Value = coupon.Value,
                IsActive = coupon.IsActive,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                DiscountType = coupon.DiscountType
            });

            // Act
            var result = await _adminCouponService.GetCouponByIdAsync(couponId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(couponId, result.Id);
            Assert.Equal("DISCOUNT10", result.Code);
        }

        [Fact]
        public async Task GetCouponByIdAsync_ShouldReturnNull_WhenCouponNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var couponId = Guid.NewGuid();

            var emptyData = new List<Coupon>().AsQueryable().BuildMockDbSet();
            _couponRepositoryMock.Setup(x => x.AsQueryable()).Returns(emptyData.Object);

            // Act
            var result = await _adminCouponService.GetCouponByIdAsync(couponId, userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCouponByIdAsync_ShouldReturnNull_WhenCouponIsDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var couponId = Guid.NewGuid();

            var coupon = new Coupon
            {
                Id = couponId,
                Code = "DELETED",
                IsDeleted = true
            };

            var mock = new List<Coupon> { coupon }.AsQueryable().BuildMockDbSet();
            _couponRepositoryMock.Setup(x => x.AsQueryable()).Returns(mock.Object);

            // Act
            var result = await _adminCouponService.GetCouponByIdAsync(couponId, userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCouponByIdAsync_ShouldMapCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var couponId = Guid.NewGuid();

            var coupon = new Coupon
            {
                Id = couponId,
                Code = "MAPTEST",
                Value = 50,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(3),
                DiscountType = DiscountType.FixedAmount,
                IsActive = true,
                IsDeleted = false
            };

            var mock = new List<Coupon> { coupon }.AsQueryable().BuildMockDbSet();
            _couponRepositoryMock.Setup(x => x.AsQueryable()).Returns(mock.Object);

            _mapperMock.Setup(x => x.Map<CouponAdminDTO>(coupon)).Returns(new CouponAdminDTO
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Value = coupon.Value,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                DiscountType = coupon.DiscountType,
                IsActive = coupon.IsActive
            });

            // Act
            var result = await _adminCouponService.GetCouponByIdAsync(couponId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("MAPTEST", result.Code);
            Assert.Equal(50, result.Value);
            Assert.Equal(DiscountType.FixedAmount, result.DiscountType);
        }
    }
}
