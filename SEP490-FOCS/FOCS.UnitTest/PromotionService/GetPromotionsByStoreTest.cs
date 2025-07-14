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
    public class GetPromotionsByStoreTest : PromotionServiceTestBase
    {
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
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var promotions = new List<Promotion>
            {
                CreateValidPromotion(storeId)
            };

            SetupValidUser(userId, user, userStore);
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
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var promotions = new List<Promotion>
            {
                CreateValidPromotion(storeId)
            };

            SetupValidUser(userId, user, userStore);
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
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var promotions = new List<Promotion>
            {
                CreateValidPromotion(storeId)
            };

            SetupValidUser(userId, user, userStore);
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
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            var aPromotion = CreateValidPromotion(storeId);
            aPromotion.Title = "A Promotion";
            var zPromotion = CreateValidPromotion(storeId);
            zPromotion.Title = "Z Promotion";
            var promotions = new List<Promotion>
            {
                aPromotion,
                zPromotion
            };

            SetupValidUser(userId, user, userStore);
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
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            var firstPromotion = CreateValidPromotion(storeId);
            firstPromotion.Title = "First Promotion";
            var secondPromotion = CreateValidPromotion(storeId);
            secondPromotion.Title = "Second Promotion";
            var promotions = new List<Promotion>
            {
                firstPromotion,
                secondPromotion
            };

            SetupValidUser(userId, user, userStore);
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
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var promotions = new List<Promotion>
            {
                CreateValidPromotion(storeId)
            };

            SetupValidUser(userId, user, userStore);
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
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var promotions = new List<Promotion>
            {
                CreateValidPromotion(storeId)
            };

            SetupValidUser(userId, user, userStore);
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
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var promotions = new List<Promotion>
            {
                CreateValidPromotion(storeId)
            };

            SetupValidUser(userId, user, userStore);
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
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var promotions = new List<Promotion>
            {
                CreateValidPromotion(storeId)
            };

            SetupValidUser(userId, user, userStore);
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
    }
}
