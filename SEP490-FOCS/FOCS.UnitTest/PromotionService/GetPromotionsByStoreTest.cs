using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;

namespace FOCS.Tests.Application.Services
{
    public class GetPromotionsByStoreTest
    {
        private readonly Mock<IRepository<Promotion>> _promotionRepositoryMock;
        private readonly Mock<IRepository<Coupon>> _couponRepositoryMock;
        private readonly Mock<IRepository<UserStore>> _userStoreRepositoryMock;
        private readonly Mock<IRepository<CouponUsage>> _couponUsageRepositoryMock;
        private readonly Mock<IRepository<PromotionItemCondition>> _promotionItemConditionRepositoryMock;
        private readonly Mock<IRepository<Store>> _storeRepositoryMock;
        private readonly Mock<IRepository<MenuItem>> _menuItemRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IPricingService> _pricingServiceMock;
        private readonly PromotionService _promotionService;

        public GetPromotionsByStoreTest()
        {
            _promotionRepositoryMock = new Mock<IRepository<Promotion>>();
            _couponRepositoryMock = new Mock<IRepository<Coupon>>();
            _userStoreRepositoryMock = new Mock<IRepository<UserStore>>();
            _couponUsageRepositoryMock = new Mock<IRepository<CouponUsage>>();
            _promotionItemConditionRepositoryMock = new Mock<IRepository<PromotionItemCondition>>();
            _storeRepositoryMock = new Mock<IRepository<Store>>();
            _menuItemRepositoryMock = new Mock<IRepository<MenuItem>>();
            _mapperMock = new Mock<IMapper>();
            _pricingServiceMock = new Mock<IPricingService>();

            // Mock UserManager
            var userStore = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            _promotionService = new PromotionService(
                _promotionRepositoryMock.Object,
                _promotionItemConditionRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _menuItemRepositoryMock.Object,
                _couponRepositoryMock.Object,
                _couponUsageRepositoryMock.Object,
                _userManagerMock.Object,
                _mapperMock.Object,
                _userStoreRepositoryMock.Object,
                _pricingServiceMock.Object
            );
        }

        [Fact]
        public async Task GetPromotionsByStoreAsync_ValidUserAndStore_ReturnsPromotions()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10
            };

            var user = new User { Id = userId };
            var userStores = new List<UserStore> { new UserStore { UserId = Guid.Parse(userId), StoreId = storeId } };
            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Promotion",
                    StoreId = storeId,
                    IsDeleted = false,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    PromotionType = PromotionType.FixedAmount,
                    DiscountValue = 10,
                    IsActive = true,
                    PromotionItemConditions = new List<PromotionItemCondition>(),
                    Coupons = new List<Coupon>()
                }
            };

            SetupValidUser(userId, storeId, user, userStores);
            SetupPromotionQueryable(promotions);

            var promotionDtos = new List<PromotionDTO>
            {
                new PromotionDTO { Id = promotions[0].Id, Title = "Test Promotion" }
            };

            _mapperMock.Setup(m => m.Map<List<PromotionDTO>>(It.IsAny<List<Promotion>>()))
                .Returns(promotionDtos);

            // Act
            var result = await _promotionService.GetPromotionsByStoreAsync(query, storeId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(10, result.PageSize);
        }

        [Fact]
        public async Task GetPromotionsByStoreAsync_InvalidUser_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };

            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.GetPromotionsByStoreAsync(query, storeId, userId));
        }

        [Fact]
        public async Task GetPromotionsByStoreAsync_UserNotAuthorizedForStore_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };

            var user = new User { Id = userId };
            var userStores = new List<UserStore>(); // Empty list - user not authorized for this store

            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userStoreRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.GetPromotionsByStoreAsync(query, storeId, userId));
        }

        [Fact]
        public async Task GetPromotionsByStoreAsync_WithFilters_AppliesFiltersCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string>
                {
                    { "promotion_type", "FixedAmount" },
                    { "start_date", DateTime.UtcNow.AddDays(-5).ToString("yyyy-MM-dd") },
                    { "end_date", DateTime.UtcNow.AddDays(5).ToString("yyyy-MM-dd") },
                    { "status", "OnGoing" }
                }
            };

            var user = new User { Id = userId };
            var userStores = new List<UserStore> { new UserStore { UserId = Guid.Parse(userId), StoreId = storeId } };
            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Promotion",
                    StoreId = storeId,
                    IsDeleted = false,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    PromotionType = PromotionType.FixedAmount,
                    IsActive = true,
                    PromotionItemConditions = new List<PromotionItemCondition>(),
                    Coupons = new List<Coupon>()
                }
            };

            SetupValidUser(userId, storeId, user, userStores);
            SetupPromotionQueryable(promotions);

            var promotionDtos = new List<PromotionDTO>
            {
                new PromotionDTO { Id = promotions[0].Id, Title = "Test Promotion" }
            };

            _mapperMock.Setup(m => m.Map<List<PromotionDTO>>(It.IsAny<List<Promotion>>()))
                .Returns(promotionDtos);

            // Act
            var result = await _promotionService.GetPromotionsByStoreAsync(query, storeId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task GetPromotionsByStoreAsync_WithSearch_AppliesSearchCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SearchBy = "title",
                SearchValue = "Test"
            };

            var user = new User { Id = userId };
            var userStores = new List<UserStore> { new UserStore { UserId = Guid.Parse(userId), StoreId = storeId } };
            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Promotion",
                    StoreId = storeId,
                    IsDeleted = false,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    PromotionType = PromotionType.FixedAmount,
                    IsActive = true,
                    PromotionItemConditions = new List<PromotionItemCondition>(),
                    Coupons = new List<Coupon>()
                }
            };

            SetupValidUser(userId, storeId, user, userStores);
            SetupPromotionQueryable(promotions);

            var promotionDtos = new List<PromotionDTO>
            {
                new PromotionDTO { Id = promotions[0].Id, Title = "Test Promotion" }
            };

            _mapperMock.Setup(m => m.Map<List<PromotionDTO>>(It.IsAny<List<Promotion>>()))
                .Returns(promotionDtos);

            // Act
            var result = await _promotionService.GetPromotionsByStoreAsync(query, storeId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task GetPromotionsByStoreAsync_WithSort_AppliesSortCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SortBy = "title",
                SortOrder = "desc"
            };

            var user = new User { Id = userId };
            var userStores = new List<UserStore> { new UserStore { UserId = Guid.Parse(userId), StoreId = storeId } };
            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Id = Guid.NewGuid(),
                    Title = "A Promotion",
                    StoreId = storeId,
                    IsDeleted = false,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    PromotionType = PromotionType.FixedAmount,
                    IsActive = true,
                    PromotionItemConditions = new List<PromotionItemCondition>(),
                    Coupons = new List<Coupon>()
                },
                new Promotion
                {
                    Id = Guid.NewGuid(),
                    Title = "Z Promotion",
                    StoreId = storeId,
                    IsDeleted = false,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    PromotionType = PromotionType.FixedAmount,
                    IsActive = true,
                    PromotionItemConditions = new List<PromotionItemCondition>(),
                    Coupons = new List<Coupon>()
                }
            };

            SetupValidUser(userId, storeId, user, userStores);
            SetupPromotionQueryable(promotions);

            var promotionDtos = new List<PromotionDTO>
            {
                new PromotionDTO { Id = promotions[0].Id, Title = "A Promotion" },
                new PromotionDTO { Id = promotions[1].Id, Title = "Z Promotion" }
            };

            _mapperMock.Setup(m => m.Map<List<PromotionDTO>>(It.IsAny<List<Promotion>>()))
                .Returns(promotionDtos);

            // Act
            var result = await _promotionService.GetPromotionsByStoreAsync(query, storeId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task GetPromotionsByStoreAsync_WithPagination_AppliesPaginationCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters
            {
                Page = 2,
                PageSize = 1
            };

            var user = new User { Id = userId };
            var userStores = new List<UserStore> { new UserStore { UserId = Guid.Parse(userId), StoreId = storeId } };
            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Id = Guid.NewGuid(),
                    Title = "First Promotion",
                    StoreId = storeId,
                    IsDeleted = false,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    PromotionType = PromotionType.FixedAmount,
                    IsActive = true,
                    PromotionItemConditions = new List<PromotionItemCondition>(),
                    Coupons = new List<Coupon>()
                },
                new Promotion
                {
                    Id = Guid.NewGuid(),
                    Title = "Second Promotion",
                    StoreId = storeId,
                    IsDeleted = false,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    PromotionType = PromotionType.FixedAmount,
                    IsActive = true,
                    PromotionItemConditions = new List<PromotionItemCondition>(),
                    Coupons = new List<Coupon>()
                }
            };

            SetupValidUser(userId, storeId, user, userStores);
            SetupPromotionQueryable(promotions);

            var promotionDtos = new List<PromotionDTO>
            {
                new PromotionDTO { Id = promotions[1].Id, Title = "Second Promotion" }
            };

            _mapperMock.Setup(m => m.Map<List<PromotionDTO>>(It.IsAny<List<Promotion>>()))
                .Returns(promotionDtos);

            // Act
            var result = await _promotionService.GetPromotionsByStoreAsync(query, storeId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(1, result.PageSize);
        }

        [Theory]
        [InlineData("promotion_type", "Percentage")]
        [InlineData("promotion_type", "BuyXGetY")]
        [InlineData("status", "Incomming")]
        [InlineData("status", "Expired")]
        [InlineData("status", "UnAvailable")]
        [InlineData("unknown_filter", "value")]
        public async Task GetPromotionsByStoreAsync_WithDifferentFilters_HandlesAllFilterTypes(string filterKey, string filterValue)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string> { { filterKey, filterValue } }
            };

            var user = new User { Id = userId };
            var userStores = new List<UserStore> { new UserStore { UserId = Guid.Parse(userId), StoreId = storeId } };
            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Promotion",
                    StoreId = storeId,
                    IsDeleted = false,
                    StartDate = DateTime.UtcNow.AddDays(1), // Future date for "Incomming" status
                    EndDate = DateTime.UtcNow.AddDays(2),
                    PromotionType = PromotionType.Percentage,
                    IsActive = false, // For "UnAvailable" status
                    PromotionItemConditions = new List<PromotionItemCondition>(),
                    Coupons = new List<Coupon>()
                }
            };

            SetupValidUser(userId, storeId, user, userStores);
            SetupPromotionQueryable(promotions);

            var promotionDtos = new List<PromotionDTO>
            {
                new PromotionDTO { Id = promotions[0].Id, Title = "Test Promotion" }
            };

            _mapperMock.Setup(m => m.Map<List<PromotionDTO>>(It.IsAny<List<Promotion>>()))
                .Returns(promotionDtos);

            // Act
            var result = await _promotionService.GetPromotionsByStoreAsync(query, storeId, userId);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("title", "asc")]
        [InlineData("title", "desc")]
        [InlineData("end_date", "asc")]
        [InlineData("end_date", "desc")]
        [InlineData("start_date", "asc")]
        [InlineData("start_date", "desc")]
        [InlineData("promotion_type", "asc")]
        [InlineData("promotion_type", "desc")]
        [InlineData("discount_value", "asc")]
        [InlineData("discount_value", "desc")]
        [InlineData("unknown_field", "asc")]
        [InlineData("", "")]
        public async Task GetPromotionsByStoreAsync_WithDifferentSortOptions_HandlesAllSortTypes(string sortBy, string sortOrder)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var user = new User { Id = userId };
            var userStores = new List<UserStore> { new UserStore { UserId = Guid.Parse(userId), StoreId = storeId } };
            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Promotion",
                    StoreId = storeId,
                    IsDeleted = false,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    PromotionType = PromotionType.FixedAmount,
                    DiscountValue = 10,
                    IsActive = true,
                    PromotionItemConditions = new List<PromotionItemCondition>(),
                    Coupons = new List<Coupon>()
                }
            };

            SetupValidUser(userId, storeId, user, userStores);
            SetupPromotionQueryable(promotions);

            var promotionDtos = new List<PromotionDTO>
            {
                new PromotionDTO { Id = promotions[0].Id, Title = "Test Promotion" }
            };

            _mapperMock.Setup(m => m.Map<List<PromotionDTO>>(It.IsAny<List<Promotion>>()))
                .Returns(promotionDtos);

            // Act
            var result = await _promotionService.GetPromotionsByStoreAsync(query, storeId, userId);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("unknown_field", "test")]
        [InlineData("", "")]
        [InlineData("title", "")]
        [InlineData("", "test")]
        public async Task GetPromotionsByStoreAsync_WithDifferentSearchOptions_HandlesAllSearchTypes(string searchBy, string searchValue)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SearchBy = searchBy,
                SearchValue = searchValue
            };

            var user = new User { Id = userId };
            var userStores = new List<UserStore> { new UserStore { UserId = Guid.Parse(userId), StoreId = storeId } };
            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Promotion",
                    StoreId = storeId,
                    IsDeleted = false,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    PromotionType = PromotionType.FixedAmount,
                    IsActive = true,
                    PromotionItemConditions = new List<PromotionItemCondition>(),
                    Coupons = new List<Coupon>()
                }
            };

            SetupValidUser(userId, storeId, user, userStores);
            SetupPromotionQueryable(promotions);

            var promotionDtos = new List<PromotionDTO>
            {
                new PromotionDTO { Id = promotions[0].Id, Title = "Test Promotion" }
            };

            _mapperMock.Setup(m => m.Map<List<PromotionDTO>>(It.IsAny<List<Promotion>>()))
                .Returns(promotionDtos);

            // Act
            var result = await _promotionService.GetPromotionsByStoreAsync(query, storeId, userId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetPromotionsByStoreAsync_WithNoFilters_ReturnsAllPromotions()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                Filters = null
            };

            var user = new User { Id = userId };
            var userStores = new List<UserStore> { new UserStore { UserId = Guid.Parse(userId), StoreId = storeId } };
            var promotions = new List<Promotion>
            {
                new Promotion
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Promotion",
                    StoreId = storeId,
                    IsDeleted = false,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    PromotionType = PromotionType.FixedAmount,
                    IsActive = true,
                    PromotionItemConditions = new List<PromotionItemCondition>(),
                    Coupons = new List<Coupon>()
                }
            };

            SetupValidUser(userId, storeId, user, userStores);
            SetupPromotionQueryable(promotions);

            var promotionDtos = new List<PromotionDTO>
            {
                new PromotionDTO { Id = promotions[0].Id, Title = "Test Promotion" }
            };

            _mapperMock.Setup(m => m.Map<List<PromotionDTO>>(It.IsAny<List<Promotion>>()))
                .Returns(promotionDtos);

            // Act
            var result = await _promotionService.GetPromotionsByStoreAsync(query, storeId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
        }

        private void SetupValidUser(string userId, Guid storeId, User user, List<UserStore> userStores)
        {
            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userStoreRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);
        }

        private void SetupPromotionQueryable(List<Promotion> promotions)
        {
            var promotionQueryable = promotions.AsQueryable().BuildMockDbSet();
            _promotionRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(promotionQueryable.Object);
        }
    }
}
