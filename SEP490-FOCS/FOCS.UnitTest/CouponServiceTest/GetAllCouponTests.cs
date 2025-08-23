using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.CouponServiceTest
{
    public class GetAllCouponTests : CouponServiceTestBase
    {
        [Fact]
        public async Task GetAllCouponsAsync_ShouldThrow_WhenUserNotAuthorizedForStore()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var user = new User { Id = userId };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userStoreRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                                    .ReturnsAsync(new List<UserStore>());

            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.GetAllCouponsAsync(query, storeId, userId));

            AssertConditionException(ex, Errors.AuthError.UserUnauthor, AdminCouponConstants.FieldStoreId);
        }

        [Fact]
        public async Task GetAllCouponsAsync_ShouldThrow_WhenStoreNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            SetupValidUser(userId, new User { Id = userId }, new List<UserStore> { new UserStore { UserId = Guid.Parse(userId), StoreId = storeId } });

            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                                .ReturnsAsync((Store?)null);

            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.GetAllCouponsAsync(query, storeId, userId));

            AssertConditionException(ex, Errors.Common.StoreNotFound, AdminCouponConstants.FieldStoreId);
        }
        
        [Fact]
        public async Task GetAllCouponsAsync_ShouldReturnPagedResult_WithMappedDTOs()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            SetupValidUserAndStore(userId, storeId);

            var coupons = new List<Coupon>
            {
                new Coupon { Id = Guid.NewGuid(), Code = "ABC", StoreId = storeId },
                new Coupon { Id = Guid.NewGuid(), Code = "XYZ", StoreId = storeId }
            };

            var queryable = coupons.AsQueryable().BuildMockDbSet().Object;

            _couponRepositoryMock.Setup(x => x.AsQueryable()).Returns(queryable);

            _mapperMock.Setup(m => m.Map<List<CouponAdminDTO>>(It.IsAny<List<Coupon>>()))
                       .Returns((List<Coupon> src) => src.Select(c => new CouponAdminDTO { Code = c.Code }).ToList());

            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };

            // Act
            var result = await _adminCouponService.GetAllCouponsAsync(query, storeId, userId);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal("ABC", result.Items[0].Code);
            Assert.Equal("XYZ", result.Items[1].Code);
        }

        [Theory]
        [InlineData("code", "DIS")]
        [InlineData("description", "Super")]
        [InlineData("discounttype", "Percent")]
        public async Task GetAllCouponsAsync_ShouldSearchByField(string searchBy, string searchValue)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            SetupValidUserAndStore(userId, storeId);

            var coupons = new List<Coupon>
            {
                new Coupon { Code = "DIS10", Description = "Super sale", DiscountType = DiscountType.Percent, StoreId = storeId },
                new Coupon { Code = "FIX5", Description = "Normal discount", DiscountType = DiscountType.FixedAmount, StoreId = storeId }
            };

            SetupCouponQueryable(coupons);

            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SearchBy = searchBy,
                SearchValue = searchValue
            };

            // Act
            var result = await _adminCouponService.GetAllCouponsAsync(query, storeId, userId);

            // Assert
            Assert.Single(result.Items);
        }

        [Theory]
        [InlineData("discount_type", "Percent")]
        [InlineData("is_active", "true")]
        [InlineData("start_date", "2024-01-01")]
        [InlineData("end_date", "2026-01-01")]
        [InlineData("status", "On_going")]
        [InlineData("promotion_id", "valid-guid")]
        [InlineData("promotion_status", "UnAvailable")]
        public async Task GetAllCouponsAsync_ShouldApplyFilters(string key, string value)
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            SetupValidUserAndStore(userId, storeId);

            var promoId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var coupons = new List<Coupon>
            {
                new Coupon
                {
                    Code = "A",
                    Description = "Filtered",
                    DiscountType = DiscountType.Percent,
                    StoreId = storeId,
                    IsActive = true,
                    StartDate = now.AddDays(-1),
                    EndDate = now.AddDays(1),
                    MaxUsage = 10,
                    CountUsed = 0,
                    PromotionId = promoId,
                    Promotion = new Promotion
                    {
                        Id = promoId,
                        IsActive = false,
                        IsDeleted = true,
                        StartDate = now.AddDays(-10),
                        EndDate = now.AddDays(10)
                    }
                }
            };

            if (key == "promotion_id") value = promoId.ToString();

            SetupCouponQueryable(coupons);

            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string> { { key, value } }
            };

            var result = await _adminCouponService.GetAllCouponsAsync(query, storeId, userId);
            Assert.Single(result.Items);
        }

        [Theory]
        [InlineData("code", "asc")]
        [InlineData("code", "desc")]
        [InlineData("value", "asc")]
        [InlineData("value", "desc")]
        [InlineData("startdate", "asc")]
        [InlineData("startdate", "desc")]
        [InlineData("enddate", "asc")]
        [InlineData("enddate", "desc")]
        [InlineData("isactive", "asc")]
        [InlineData("isactive", "desc")]
        public async Task GetAllCouponsAsync_ShouldSortByField(string sortBy, string sortOrder)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            SetupValidUserAndStore(userId, storeId);

            var coupons = new List<Coupon>
            {
                new Coupon
                {
                    Code = "B",
                    Value = 5,
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(10),
                    IsActive = true,
                    StoreId = storeId
                },
                new Coupon
                {
                    Code = "A",
                    Value = 10,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(5),
                    IsActive = false,
                    StoreId = storeId
                }
            };

            SetupCouponQueryable(coupons);

            _mapperMock.Setup(m => m.Map<List<CouponAdminDTO>>(It.IsAny<List<Coupon>>()))
                .Returns((List<Coupon> src) => src.Select(c => new CouponAdminDTO
                {
                    Code = c.Code,
                    Value = c.Value,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive
                }).ToList());

            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Act
            var result = await _adminCouponService.GetAllCouponsAsync(query, storeId, userId);

            // Assert
            Assert.Equal(2, result.Items.Count);

            var first = result.Items[0];
            var second = result.Items[1];

            bool isCorrectOrder = sortBy.ToLower() switch
            {
                "code" => string.Compare(first.Code, second.Code, StringComparison.Ordinal) < 0,
                "value" => first.Value < second.Value,
                "startdate" => first.StartDate < second.StartDate,
                "enddate" => first.EndDate < second.EndDate,
                "isactive" => !first.IsActive && second.IsActive,
                _ => true
            };

            if (sortOrder.ToLower() == "desc")
                isCorrectOrder = !isCorrectOrder;

            Assert.True(isCorrectOrder, $"Expected {sortBy} in {sortOrder} order but got wrong order.");
        }

        [Fact]
        public async Task GetAllCouponsAsync_ShouldPaginateCorrectly()
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            SetupValidUserAndStore(userId, storeId);

            var coupons = Enumerable.Range(1, 25).Select(i =>
                new Coupon { Code = $"C{i}", StoreId = storeId }).ToList();

            SetupCouponQueryable(coupons);

            var query = new UrlQueryParameters
            {
                Page = 2,
                PageSize = 10
            };

            var result = await _adminCouponService.GetAllCouponsAsync(query, storeId, userId);

            Assert.Equal(10, result.Items.Count);
            Assert.Equal(25, result.TotalCount);
            Assert.Equal(2, result.PageIndex);
        }
    }
}
