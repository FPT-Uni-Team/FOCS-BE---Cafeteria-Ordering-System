using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Common.Enums;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest
{
    public class CouponUnitTest
    {
        private readonly Mock<IRepository<Coupon>> _couponRepositoryMock;
        private readonly Mock<IRepository<CouponUsage>> _couponUsageRepositoryMock;
        private readonly Mock<IRepository<Promotion>> _promotionRepositoryMock;
        private readonly Mock<IRepository<UserStore>> _userStoreRepositoryMock;
        private readonly Mock<IRepository<Store>> _storeRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<Coupon>> _loggerMock;
        private readonly AdminCouponService _adminCouponService;

        public CouponUnitTest()
        {
            _couponRepositoryMock = new Mock<IRepository<Coupon>>();
            _couponUsageRepositoryMock = new Mock<IRepository<CouponUsage>>();
            _promotionRepositoryMock = new Mock<IRepository<Promotion>>();
            _userStoreRepositoryMock = new Mock<IRepository<UserStore>>();
            _storeRepositoryMock = new Mock<IRepository<Store>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<Coupon>>();
            _userManagerMock = MockUserManager();

            _adminCouponService = new AdminCouponService(
                _couponRepositoryMock.Object,
                _couponUsageRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _userStoreRepositoryMock.Object,
                _userManagerMock.Object,
                _loggerMock.Object,
                _promotionRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        private static Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        #region Helper
        private void SetupMinimalMocks(Guid storeId, string userId)
        {
            _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId)).ReturnsAsync(new Store { Id = storeId });

            var user = new User { Id = userId };
            _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);

            var userStores = new List<UserStore>
            {
                new UserStore { UserId = Guid.Parse(userId), StoreId = storeId }
            };
            _userStoreRepositoryMock
                .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);

            _couponRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(new List<Coupon>().AsQueryable().BuildMockDbSet().Object);

            _promotionRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(new List<Promotion>().AsQueryable().BuildMockDbSet().Object);

            _couponRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Coupon>())).Returns(Task.CompletedTask);
            _couponRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        }
        #endregion

        #region CreateCoupon CM-11
        [Theory]
        [InlineData("New Coupon", CouponType.Manual, "String Description", DiscountType.Percent, 1, "2025-06-16T07:42:49.021Z", "2025-06-17T07:42:49.021Z", 1, "c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a", true)]
        [InlineData("", CouponType.AutoGenerate, "String Description", DiscountType.FixedAmount, 0, "2025-06-16T07:42:49.021Z", "2025-06-17T07:42:49.021Z", 1, null, true)]
        [InlineData("Promotion", CouponType.Manual, "String Description", DiscountType.Percent, 1, "2025-06-16T07:42:49.021Z", "2025-06-17T07:42:49.021Z", 1, "c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a", true)]
        [InlineData("New Coupon", CouponType.Manual, "String Description", DiscountType.Percent, 1, "2025-06-16T07:42:49.021Z", "2025-06-17T07:42:49.021Z", null, "c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a", true)]
        [InlineData("New Coupon", CouponType.Manual, "String Description", DiscountType.Percent, 1, "2025-06-16T07:42:49.021Z", "2025-06-16T07:42:49.021Z", 1, "c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a", false)]
        [InlineData("", CouponType.Manual, "String Description", DiscountType.Percent, 1, "2025-09-10", "2025-09-11", 1, "c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a", false)]
        public async Task CreateCouponAsync_SimpleRun_ChecksIfServiceRuns(
            string code,
            CouponType couponType,
            string description,
            DiscountType discountType,
            double value,
            string startDateStr,
            string endDateStr,
            int maxUsage,
            string acceptForItems,
            bool shouldSucceed)
        {
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var dto = new CouponAdminDTO
            {
                Code = string.IsNullOrWhiteSpace(code) ? null : code,
                CouponType = couponType,
                Description = description,
                DiscountType = discountType,
                Value = value,
                StartDate = DateTime.Parse(startDateStr),
                EndDate = DateTime.Parse(endDateStr),
                MaxUsage = maxUsage,
                AcceptForItems = string.IsNullOrWhiteSpace(acceptForItems) ? null : new List<string> { acceptForItems },
                IsActive = true
            };

            SetupMinimalMocks(storeId, userId);

            if (shouldSucceed)
            {
                _mapperMock.Setup(m => m.Map<Coupon>(It.IsAny<CouponAdminDTO>()))
                    .Returns((CouponAdminDTO d) => new Coupon { Id = Guid.NewGuid(), Code = string.IsNullOrWhiteSpace(d.Code) ? "AUTO" : d.Code });
                _mapperMock.Setup(m => m.Map<CouponAdminDTO>(It.IsAny<Coupon>()))
                    .Returns((Coupon c) => dto);
            }

            var ex = await Record.ExceptionAsync(() => _adminCouponService.CreateCouponAsync(dto, userId, storeId.ToString()));
            if (shouldSucceed)
                Assert.Null(ex);
            else
                Assert.NotNull(ex);
        }
        #endregion

        #region GetCouponsByStore CM-12
        [Theory]
        [InlineData(1, 5, "Code", "CP1", "code", "asc", true)]
        [InlineData(1, 5, "Description", "Discount", "value", "desc", true)]
        [InlineData(1, 5, "DiscountType", "Percent", "startdate", "asc", true)]
        [InlineData(1, 5, "StartDate", "2025-01-01", "enddate", "desc", true)]
        [InlineData(1, 5, "IsActive", "true", "isactive", "asc", true)]
        [InlineData(1, 5, null, null, null, null, true)]
        public async Task GetAllCouponsAsync_SimpleRun_ChecksIfServiceRuns(
            int page,
            int pageSize,
            string? searchBy,
            string? searchValue,
            string? sortBy,
            string? sortOrder,
            bool shouldSucceed)
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var coupons = new List<Coupon>
            {
                new Coupon { Id = Guid.NewGuid(), StoreId = storeId, Code = "DISCOUNT10" },
                new Coupon { Id = Guid.NewGuid(), StoreId = storeId, Code = "PROMO2025" }
            };

            _couponRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(coupons.AsQueryable().BuildMockDbSet().Object);

            _mapperMock.Setup(m => m.Map<List<CouponAdminDTO>>(It.IsAny<List<Coupon>>()))
                .Returns(coupons.Select(c => new CouponAdminDTO { Code = c.Code }).ToList());

            SetupMinimalMocks(storeId, userId);

            var query = new UrlQueryParameters
            {
                Page = page,
                PageSize = pageSize,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Act
            var ex = await Record.ExceptionAsync(() =>
                _adminCouponService.GetAllCouponsAsync(query, storeId, userId));

            // Assert
            if (shouldSucceed)
                Assert.Null(ex);
            else
                Assert.NotNull(ex);
        }
        #endregion

        #region  GetCouponDetail CM-13
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        public async Task GetCouponByIdAsync_SimpleRun_ChecksIfServiceRuns(
            string couponIdStr,
            bool shouldSucceed)
        {
            var userId = Guid.NewGuid().ToString();
            // Arrange
            var couponId = Guid.Parse(couponIdStr);
            var storeId = Guid.NewGuid();

            // chỉ trả về coupon khi test mong đợi success
            var coupon = shouldSucceed
                ? new Coupon { Id = couponId, StoreId = storeId, Code = "DISCOUNT10" }
                : null;

            _couponRepositoryMock.Setup(r => r.GetByIdAsync(couponId))
                .ReturnsAsync(coupon);

            if (coupon != null)
            {
                _mapperMock.Setup(m => m.Map<CouponAdminDTO>(It.IsAny<Coupon>()))
                    .Returns(new CouponAdminDTO { Code = coupon.Code });
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                SetupMinimalMocks(storeId, userId);
            }
            else
            {
                _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId)).ReturnsAsync(new Store { Id = storeId });
                _userManagerMock.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync((User?)null);
            }

            // Act
            CouponAdminDTO? result = null;
            Exception? ex = await Record.ExceptionAsync(async () =>
            {
                result = await _adminCouponService.GetCouponByIdAsync(couponId, userId);
            });

            // Assert
            if (shouldSucceed)
                Assert.Null(ex);
            else
            {
                Assert.Null(result);
            }
        }
        #endregion

        #region UpdateCoupon CM-14
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "New Coupon", CouponType.Manual, "String Description", DiscountType.Percent, 1, "2025-06-16T07:42:49.021Z", "2025-06-17T07:42:49.021Z", 1, "c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "", CouponType.AutoGenerate, "String Description", DiscountType.FixedAmount, 0, "2025-06-16T07:42:49.021Z", "2025-06-17T07:42:49.021Z", 1, null, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "Promotion", CouponType.Manual, "String Description", DiscountType.Percent, 1, "2025-06-16T07:42:49.021Z", "2025-06-17T07:42:49.021Z", 1, "c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "New Coupon", CouponType.Manual, "String Description", DiscountType.Percent, 1, "2025-06-16T07:42:49.021Z", "2025-06-17T07:42:49.021Z", null, "c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "", CouponType.Manual, "String Description", DiscountType.Percent, 1, "2025-09-10", "2025-09-11", 1, "c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a", false)]
        public async Task UpdateCouponAsync_SimpleRun_ChecksIfServiceRuns(
            string idStr,
            string code,
            CouponType couponType,
            string description,
            DiscountType discountType,
            double value,
            string startDateStr,
            string endDateStr,
            int maxUsage,
            string acceptForItems,
            bool shouldSucceed)
        {
            var couponId = Guid.Parse(idStr);
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var existingCoupon = new Coupon
            {
                Id = couponId,
                Code = "EXISTING",
                CouponType = CouponType.Manual,
                StartDate = DateTime.Parse("2025-06-10T00:00:00Z"),
                EndDate = DateTime.Parse("2025-06-12T00:00:00Z"),
                StoreId = storeId,
                IsDeleted = false
            };

            var dto = new CouponAdminDTO
            {
                Code = string.IsNullOrWhiteSpace(code) ? null : code,
                CouponType = couponType,
                Description = description,
                DiscountType = discountType,
                Value = value,
                StartDate = DateTime.Parse(startDateStr),
                EndDate = DateTime.Parse(endDateStr),
                MaxUsage = maxUsage,
                AcceptForItems = string.IsNullOrWhiteSpace(acceptForItems) ? null : new List<string> { acceptForItems },
                IsActive = true
            };

            SetupMinimalMocks(storeId, userId);

            _couponRepositoryMock.Setup(r => r.GetByIdAsync(couponId))
                .ReturnsAsync(existingCoupon);

            _mapperMock.Setup(m => m.Map(It.IsAny<CouponAdminDTO>(), It.IsAny<Coupon>()))
                .Callback((CouponAdminDTO d, Coupon c) =>
                {
                    if (!string.IsNullOrWhiteSpace(d.Code)) c.Code = d.Code;
                    c.Description = d.Description;
                    c.Value = d.Value;
                    c.StartDate = d.StartDate;
                    c.EndDate = d.EndDate;
                });

            var ex = await Record.ExceptionAsync(() =>
                _adminCouponService.UpdateCouponAsync(couponId, dto, userId, storeId.ToString()));

            if (shouldSucceed)
            {
                Assert.Null(ex);
            }
            else
            {
                Assert.NotNull(ex);
            }
        }
        #endregion

        #region DeleteCoupon CM-15
        [Theory]
        [InlineData("d7c0b9bb-b9b5-4a25-915a-22f3f72a1b2e", true)]
        [InlineData("8a6c55a3-ffbb-42c1-87b9-96c841da5f4b", false)]
        public async Task DeleteCouponAsync_SimpleRun_ChecksIfServiceRuns(
            string couponIdStr,
            bool shouldSucceed)
        {
            var userId = Guid.NewGuid().ToString();
            var couponId = Guid.Parse(couponIdStr);

            // chỉ trả về coupon khi mong đợi success
            var coupon = shouldSucceed
                ? new Coupon { Id = couponId, IsDeleted = false }
                : null;

            _couponRepositoryMock.Setup(r => r.GetByIdAsync(couponId))
                .ReturnsAsync(coupon);

            if (coupon != null)
            {
                _couponRepositoryMock.Setup(r => r.SaveChangesAsync())
                    .ReturnsAsync(1); // giả lập save thành công
            }

            // Act
            bool result = false;
            Exception? ex = await Record.ExceptionAsync(async () =>
            {
                result = await _adminCouponService.DeleteCouponAsync(couponId, userId);
            });

            // Assert
            if (shouldSucceed)
            {
                Assert.Null(ex);
                Assert.True(result);
            }
            else
            {
                Assert.False(result);
            }
        }
        #endregion

        #region SetCouponCondition CM-16
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", CouponConditionType.MinOrderAmount, 1, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", CouponConditionType.MinItemsQuantity, 999, true)]
        //[InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", CouponConditionType.MinItemsQuantity, null, false)] 
        public async Task SetCouponConditionAsync_SimpleRun_ChecksIfServiceRuns(
            string couponIdStr,
            CouponConditionType conditionType,
            int value,
            bool shouldSucceed)
        {
            // Arrange
            var couponId = Guid.Parse(couponIdStr);
            var storeId = Guid.NewGuid();

            var coupon = new Coupon
            {
                Id = couponId,
                StoreId = storeId,
                IsDeleted = false
            };

            var request = new SetCouponConditionRequest
            {
                ConditionType = conditionType,
                Value = (int)value
            };

            _couponRepositoryMock.Setup(r => r.GetByIdAsync(couponId))
                .ReturnsAsync(coupon);

            _couponRepositoryMock.Setup(r => r.Update(It.IsAny<Coupon>()))
                .Verifiable();

            _couponRepositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var ex = await Record.ExceptionAsync(() =>
                _adminCouponService.SetCouponConditionAsync(couponId, request));

            // Assert
            if (shouldSucceed)
            {
                Assert.Null(ex);
                _couponRepositoryMock.Verify(r => r.Update(It.Is<Coupon>(c =>
                    conditionType == CouponConditionType.MinOrderAmount
                        ? c.MinimumOrderAmount == Convert.ToDouble(value)
                        : c.MinimumItemQuantity == Convert.ToInt32(value)
                )), Times.Once);
            }
            else
            {
                Assert.NotNull(ex);
            }
        }
        #endregion

        #region TrackCouponUsage CM-17
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        public async Task TrackCouponUsageAsync_SimpleRun_ChecksIfServiceRuns(string couponIdStr, bool shouldSucceed)
        {
            // Arrange
            var couponId = Guid.Parse(couponIdStr);

            if (shouldSucceed)
            {
                var coupon = new Coupon
                {
                    Id = couponId,
                    Code = "TEST10",
                    IsDeleted = false,
                    IsActive = true,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    MaxUsage = 5
                };

                var usageList = new List<CouponUsage>
                {
                    new CouponUsage { CouponId = couponId, OrderId = Guid.NewGuid(), UsedAt = DateTime.UtcNow }
                };

                _couponRepositoryMock.Setup(r => r.GetByIdAsync(couponId))
                    .ReturnsAsync(coupon);

                _couponUsageRepositoryMock.Setup(r => r.AsQueryable())
                    .Returns(usageList.AsQueryable().BuildMock());
            }
            else
            {
                _couponRepositoryMock.Setup(r => r.GetByIdAsync(couponId))
                    .ReturnsAsync((Coupon?)null);
            }

            // Act & Assert
            if (shouldSucceed)
            {
                var result = await _adminCouponService.TrackCouponUsageAsync(couponId);

                Assert.NotNull(result);
                Assert.True(result.TotalLeft >= 0);
                Assert.NotNull(result.Usages);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(async () =>
                    await _adminCouponService.TrackCouponUsageAsync(couponId));
            }
        }
        #endregion

        #region SetCouponStatus CM-18
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        public async Task SetCouponStatusAsync_SimpleRun_ChecksIfServiceRuns(string couponIdStr, bool shouldSucceed)
        {
            // Arrange
            var couponId = Guid.Parse(couponIdStr);
            var userId = "test-user";

            if (shouldSucceed)
            {
                var coupon = new Coupon
                {
                    Id = couponId,
                    Code = "TEST10",
                    IsDeleted = false,
                    IsActive = false,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    MaxUsage = 10
                };

                _couponRepositoryMock.Setup(r => r.GetByIdAsync(couponId))
                    .ReturnsAsync(coupon);

                _couponRepositoryMock.Setup(r => r.SaveChangesAsync())
                    .ReturnsAsync(1);
            }
            else
            {
                _couponRepositoryMock.Setup(r => r.GetByIdAsync(couponId))
                    .ReturnsAsync((Coupon?)null);
            }

            // Act
            var result = await _adminCouponService.SetCouponStatusAsync(couponId, true, userId);

            // Assert
            if (shouldSucceed)
            {
                Assert.True(result);
                _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            }
            else
            {
                Assert.False(result);
                _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
            }
        }
        #endregion

        #region AssignCouponToPromotion CM-19
        [Theory]
        [InlineData("a74fc46a-c2c0-4055-b02e-d36bbc5033bf", "bb42be6b-a4be-474a-80ad-de365c6286a9", "test-user", "1983a41d-68d1-4b68-b329-ff056da7ab33", true)]
        [InlineData("5cdaf6e6-e702-441a-8c69-a8c7a2159ba6", "bb42be6b-a4be-474a-80ad-de365c6286a9", "test-user", "1983a41d-68d1-4b68-b329-ff056da7ab33", false)]
        [InlineData(null, null, "test-user", "fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        [InlineData(null, null, "test-user", null, false)]
        public async Task AssignCouponsToPromotionAsync_ShouldBehaveAsExpected(
            string couponIdStr,
            string promotionIdStr,
            string userId,
            string storeIdStr,
            bool shouldSucceed)
        {
            // Arrange
            var couponIds = new List<Guid>();
            Guid promotionId = Guid.Empty;
            Guid storeId = Guid.Empty;

            if (couponIdStr != null && promotionIdStr != null && storeIdStr != null)
            {
                var couponId = Guid.Parse(couponIdStr);
                promotionId = Guid.Parse(promotionIdStr);
                storeId = Guid.Parse(storeIdStr);
                couponIds.Add(couponId);

                // Chỉ inline đầu tiên là setup dữ liệu tồn tại
                if (shouldSucceed)
                {
                    var promotion = new Promotion
                    {
                        Id = promotionId,
                        StoreId = storeId,
                        StartDate = DateTime.UtcNow.AddDays(-1),
                        EndDate = DateTime.UtcNow.AddDays(5),
                        IsDeleted = false
                    };

                    var coupon = new Coupon
                    {
                        Id = couponId,
                        StoreId = storeId,
                        Code = "PROMO10",
                        StartDate = promotion.StartDate,
                        EndDate = promotion.EndDate,
                        IsDeleted = false
                    };

                    _promotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId))
                        .ReturnsAsync(promotion);

                    _couponRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                        .ReturnsAsync(new List<Coupon> { coupon });

                    _couponRepositoryMock.Setup(r => r.SaveChangesAsync())
                        .ReturnsAsync(1);
                }
                else
                {
                    // Không tìm thấy promotion hoặc coupon
                    _promotionRepositoryMock.Setup(r => r.GetByIdAsync(promotionId))
                        .ReturnsAsync((Promotion?)null);

                    _couponRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                        .ReturnsAsync(new List<Coupon>());
                }
            }
            else
            {
                // Null → fail
                _promotionRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((Promotion?)null);
            }

            // Act & Assert
            if (shouldSucceed)
            {
                var result = await _adminCouponService.AssignCouponsToPromotionAsync(couponIds, promotionId, userId, storeId);

                Assert.True(result);
                _couponRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(async () =>
                    await _adminCouponService.AssignCouponsToPromotionAsync(couponIds, promotionId, userId, storeId));
            }
        }
        #endregion
    }
}
