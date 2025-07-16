using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Constants;
using FOCS.Common.Exceptions;
using FOCS.Order.Infrastucture.Entities;
using FOCS.UnitTest.CouponServiceTest;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.CouponServiceTest
{
    public class GetCouponsByListIdTests : CouponServiceTestBase
    {
        [Fact]
        public async Task ShouldReturnMappedCoupons_WhenCouponsExist()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var storeGuid = Guid.Parse(storeId);
            var userId = Guid.NewGuid().ToString();
            var couponIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var coupons = couponIds.Select(id => new Coupon
            {
                Id = id,
                Code = "CODE_" + id,
                IsDeleted = false,
                StoreId = storeGuid
            }).ToList();

            SetupValidUserAndStore(userId, storeGuid);

            _couponRepositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(coupons);

            _mapperMock
                .Setup(m => m.Map<List<CouponAdminDTO>>(coupons))
                .Returns(coupons.Select(c => new CouponAdminDTO { Id = c.Id, Code = c.Code }).ToList());

            // Act
            var result = await _adminCouponService.GetCouponsByListIdAsync(couponIds, storeId, userId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("CODE_" + couponIds[0], result[0].Code);
        }

        [Fact]
        public async Task GetCouponsByListIdAsync_ShouldReturnEmptyList_WhenCouponIdsIsNull()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            SetupValidUserAndStore(userId, storeId);

            // Act
            var result = await _adminCouponService.GetCouponsByListIdAsync(null, storeId.ToString(), userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCouponsByListIdAsync_ShouldReturnEmptyList_WhenCouponIdsIsEmpty()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            SetupValidUserAndStore(userId, storeId);

            // Act
            var result = await _adminCouponService.GetCouponsByListIdAsync(
                new List<Guid>(),
                storeId.ToString(),
                userId
            );

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCouponsByListIdAsync_ShouldThrow_WhenStoreIdIsInvalid()
        {
            // Arrange
            var invalidStoreId = "invalid-guid";
            var userId = Guid.NewGuid().ToString();
            var couponIds = new List<Guid> { Guid.NewGuid() };

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.GetCouponsByListIdAsync(couponIds, invalidStoreId, userId));

            // Assert
            AssertConditionException(ex, Errors.Common.InvalidGuidFormat, AdminCouponConstants.FieldStoreId);
        }

        [Fact]
        public async Task GetCouponsByListIdAsync_ShouldThrow_WhenNoCouponsFound()
        {
            // Arrange
            var storeGuid = Guid.NewGuid();
            var storeId = storeGuid.ToString();
            var userId = Guid.NewGuid().ToString();
            var couponIds = new List<Guid> { Guid.NewGuid() };

            SetupValidUserAndStore(userId, storeGuid);

            _couponRepositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                .ReturnsAsync(new List<Coupon>());

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.GetCouponsByListIdAsync(couponIds, storeId, userId));

            // Assert
            AssertConditionException(ex, AdminCouponConstants.GetCouponsByListIdNotFound, AdminCouponConstants.FieldListCouponId);
        }

    }
}