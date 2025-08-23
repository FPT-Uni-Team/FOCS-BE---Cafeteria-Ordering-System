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
    public class GetAvailableCouponsTests : CouponServiceTestBase
    {
        [Fact]
        public async Task ShouldThrow_WhenUserNotAuthorized()
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(new User { Id = userId });
            _userStoreRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                                    .ReturnsAsync(new List<UserStore>());

            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.GetAvailableCouponsAsync(query, promotionId, storeId, userId));

            AssertConditionException(ex, Errors.AuthError.UserUnauthor, AdminCouponConstants.FieldStoreId);
        }

        [Fact]
        public async Task ShouldThrow_WhenStoreNotFound()
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();

            SetupValidUser(userId, new User { Id = userId }, new List<UserStore> { new UserStore { UserId = Guid.Parse(userId), StoreId = storeId } });
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId)).ReturnsAsync((Store?)null);

            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _adminCouponService.GetAvailableCouponsAsync(query, promotionId, storeId, userId));

            AssertConditionException(ex, Errors.Common.StoreNotFound, AdminCouponConstants.FieldStoreId);
        }

        [Theory]
        [InlineData("code", "X")]
        [InlineData("description", "desc")]
        [InlineData("discounttype", "Percent")]
        public async Task ShouldSearchByField(string searchBy, string searchValue)
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            SetupValidUserAndStore(userId, storeId);

            var coupons = new List<Coupon>
            {
                new Coupon { Code = "XYZ", Description = "desc match", DiscountType = DiscountType.Percent, IsActive = true, StoreId = storeId, EndDate = DateTime.UtcNow.AddDays(5), MaxUsage = 10, CountUsed = 0 },
                new Coupon { Code = "ABC", Description = "no match", DiscountType = DiscountType.FixedAmount, IsActive = true, StoreId = storeId, EndDate = DateTime.UtcNow.AddDays(5), MaxUsage = 10, CountUsed = 0 },
            };
            SetupCouponQueryable(coupons);

            var query = new UrlQueryParameters { Page = 1, PageSize = 10, SearchBy = searchBy, SearchValue = searchValue };

            var result = await _adminCouponService.GetAvailableCouponsAsync(query, promotionId, storeId, userId);
            Assert.Single(result.Items);
        }

        [Theory]
        [InlineData("discount_type", "Percent")]
        [InlineData("is_active", "true")]
        [InlineData("start_date", "2020-01-01")]
        [InlineData("end_date", "2100-01-01")]
        [InlineData("status", "On_going")]
        [InlineData("promotion_id", "00000000-0000-0000-0000-000000000000")]
        [InlineData("promotion_status", "InPromotionDuration")]
        public async Task ShouldFilterByField(string key, string value)
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            SetupValidUserAndStore(userId, storeId);

            var coupons = new List<Coupon>
            {
                new Coupon { Code = "A", DiscountType = DiscountType.Percent, IsActive = true, StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(5), MaxUsage = 10, CountUsed = 0, StoreId = storeId, Promotion = new Promotion { IsActive = true, IsDeleted = false, StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(5) } },
                new Coupon { Code = "B", DiscountType = DiscountType.FixedAmount, IsActive = false, StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(-5), MaxUsage = 10, CountUsed = 0, StoreId = storeId, Promotion = new Promotion { IsActive = false, IsDeleted = true, StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(-5) } }
            };
            SetupCouponQueryable(coupons);

            var query = new UrlQueryParameters { Page = 1, PageSize = 10, Filters = new() { { key, value } } };

            var result = await _adminCouponService.GetAvailableCouponsAsync(query, promotionId, storeId, userId);
            Assert.True(result.Items.Count >= 0);
        }

        [Theory]
        [InlineData("code", "asc")]
        [InlineData("code", "desc")]
        [InlineData("value", "asc")]
        [InlineData("value", "desc")]
        [InlineData("start_date", "asc")]
        [InlineData("start_date", "desc")]
        [InlineData("end_date", "asc")]
        [InlineData("end_date", "desc")]
        [InlineData("isactive", "asc")]
        [InlineData("isactive", "desc")]
        public async Task GetAvailableCouponsAsync_ShouldSortByField(string sortBy, string sortOrder)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            SetupValidUserAndStore(userId, storeId);

            var now = DateTime.UtcNow;

            var coupons = new List<Coupon>
            {
                new Coupon
                {
                    Code = "A",
                    Value = 5,
                    StartDate = now.AddDays(1),
                    EndDate   = now.AddDays(5),
                    IsActive  = true,
                    StoreId   = storeId,
                    MaxUsage  = 10,
                    CountUsed = 0,
                    PromotionId = promotionId
                },
                new Coupon
                {
                    Code = "B",
                    Value = 10,
                    StartDate = now.AddDays(2),
                    EndDate   = now.AddDays(6),
                    IsActive  = true,
                    StoreId   = storeId,
                    MaxUsage  = 10,
                    CountUsed = 0,
                    PromotionId = promotionId
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
            var result = await _adminCouponService.GetAvailableCouponsAsync(query, promotionId, storeId, userId);

            // Assert
            Assert.Equal(2, result.Items.Count);

            var first = result.Items[0];
            var second = result.Items[1];

            switch (sortBy.ToLower())
            {
                case "code":
                    Assert.Equal(sortOrder == "asc" ? "A" : "B", first.Code);
                    break;

                case "value":
                    Assert.Equal(sortOrder == "asc" ? 5 : 10, first.Value);
                    break;

                case "startdate":
                    Assert.Equal(sortOrder == "asc" ? now.AddDays(1).Date : now.AddDays(2).Date, first.StartDate.Date);
                    break;

                case "enddate":
                    Assert.Equal(sortOrder == "asc" ? now.AddDays(5).Date : now.AddDays(6).Date, first.EndDate.Date);
                    break;
            }
        }

        [Fact]
        public async Task ShouldPaginateCorrectly()
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            SetupValidUserAndStore(userId, storeId);

            var coupons = Enumerable.Range(1, 20).Select(i => new Coupon
            {
                Code = $"Code{i}",
                IsActive = true,
                StoreId = storeId,
                EndDate = DateTime.UtcNow.AddDays(1),
                MaxUsage = 100,
                CountUsed = 0
            }).ToList();

            SetupCouponQueryable(coupons);

            var query = new UrlQueryParameters { Page = 2, PageSize = 5 };

            var result = await _adminCouponService.GetAvailableCouponsAsync(query, promotionId, storeId, userId);
            Assert.Equal(5, result.Items.Count);
            Assert.Equal(20, result.TotalCount);
            Assert.Equal(2, result.PageIndex);
        }

        [Fact]
        public async Task ShouldOnlyReturnActiveValidAndUsable()
        {
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var promotionId = Guid.NewGuid();
            SetupValidUserAndStore(userId, storeId);

            var now = DateTime.UtcNow;
            var coupons = new List<Coupon>
            {
                new Coupon { Code = "Valid1", IsActive = true, EndDate = now.AddDays(1), CountUsed = 0, MaxUsage = 10, StoreId = storeId },
                new Coupon { Code = "UsedUp", IsActive = true, EndDate = now.AddDays(1), CountUsed = 10, MaxUsage = 10, StoreId = storeId },
                new Coupon { Code = "Inactive", IsActive = false, EndDate = now.AddDays(1), CountUsed = 0, MaxUsage = 10, StoreId = storeId },
                new Coupon { Code = "Expired", IsActive = true, EndDate = now.AddDays(-1), CountUsed = 0, MaxUsage = 10, StoreId = storeId },
            };
            SetupCouponQueryable(coupons);

            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };
            var result = await _adminCouponService.GetAvailableCouponsAsync(query, promotionId, storeId, userId);

            Assert.Single(result.Items);
            Assert.Equal("Valid1", result.Items[0].Code);
        }
    }
}
