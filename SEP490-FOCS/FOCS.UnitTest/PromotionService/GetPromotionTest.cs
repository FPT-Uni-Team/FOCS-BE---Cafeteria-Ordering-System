using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using System.Linq.Expressions;

namespace FOCS.Tests.Application.Services
{
    public class GetPromotionTest : PromotionServiceTestBase
    {
        [Fact]
        public async Task GetPromotionAsync_ValidPromotionAndUser_ReturnsPromotionDTO()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            var expectedPromotionDTO = new PromotionDTO
            {
                Id = promotionId,
                Title = "Test Promotion",
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                PromotionType = PromotionType.FixedAmount,
                DiscountValue = 10,
                IsActive = true
            };

            var promotions = new List<Promotion> { promotion };
            SetupPromotionQueryable(promotions);
            SetupValidUser(userId, user, userStore);

            _mapperMock.Setup(m => m.Map<PromotionDTO>(It.Is<Promotion>(p => p.Id == promotionId && !p.IsDeleted)))
                .Returns(expectedPromotionDTO);

            // Act
            var result = await _promotionService.GetPromotionAsync(promotionId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(promotionId, result.Id);
            Assert.Equal("Test Promotion", result.Title);
            Assert.Equal(PromotionType.FixedAmount, result.PromotionType);
            Assert.Equal(10, result.DiscountValue);
            Assert.True(result.IsActive);

            // Verify all mocks were called
            _promotionRepositoryMock.Verify(r => r.AsQueryable(), Times.Once);
            _userManagerMock.Verify(um => um.FindByIdAsync(userId), Times.Once);
            _userStoreRepositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Order.Infrastucture.Entities.UserStore, bool>>>()), Times.Once);
            _mapperMock.Verify(m => m.Map<PromotionDTO>(It.IsAny<Promotion>()), Times.Once);
        }

        [Fact]
        public async Task GetPromotionAsync_PromotionNotFound_ThrowsException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            // Setup promotion query to return null (promotion not found)
            var promotions = new List<Promotion>();
            SetupPromotionQueryable(promotions);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.GetPromotionAsync(promotionId, userId));

            Assert.Equal(Errors.Common.NotFound + "@" + Errors.FieldName.Id, exception.Message);

            // Verify repository was called but user validation was not (since promotion wasn't found)
            _promotionRepositoryMock.Verify(r => r.AsQueryable(), Times.Once);
            _userManagerMock.Verify(um => um.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetPromotionAsync_PromotionIsDeleted_ThrowsException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var deletedPromotion = CreateValidPromotion(storeId);
            deletedPromotion.IsDeleted = true;
            deletedPromotion.Id = promotionId;
            var promotions = new List<Promotion> { deletedPromotion };

            SetupPromotionQueryable(promotions);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.GetPromotionAsync(promotionId, userId));

            Assert.Equal(Errors.Common.NotFound + "@" + Errors.FieldName.Id, exception.Message);

            // Verify repository was called but user validation was not (since promotion was deleted)
            _promotionRepositoryMock.Verify(r => r.AsQueryable(), Times.Once);
            _userManagerMock.Verify(um => um.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetPromotionAsync_UserNotFound_ThrowsException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;

            var promotions = new List<Promotion> { promotion };
            SetupPromotionQueryable(promotions);

            // Setup user manager to return null (user not found)
            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.GetPromotionAsync(promotionId, userId));

            Assert.Equal(Errors.Common.UserNotFound + "@" + Errors.FieldName.UserId, exception.Message);

            // Verify calls
            _promotionRepositoryMock.Verify(r => r.AsQueryable(), Times.Once);
            _userManagerMock.Verify(um => um.FindByIdAsync(userId), Times.Once);
            _userStoreRepositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task GetPromotionAsync_UserNotAuthorizedForStore_ThrowsException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            var user = new User { Id = userId };
            var promotions = new List<Promotion> { promotion };

            SetupPromotionQueryable(promotions);
            SetupValidUser(userId, user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.GetPromotionAsync(promotionId, userId));

            Assert.Equal(Errors.AuthError.UserUnauthor + "@" + Errors.FieldName.UserId, exception.Message);

            // Verify calls
            _promotionRepositoryMock.Verify(r => r.AsQueryable(), Times.Once);
            _userManagerMock.Verify(um => um.FindByIdAsync(userId), Times.Once);
            _userStoreRepositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()), Times.Once);
            _mapperMock.Verify(m => m.Map<PromotionDTO>(It.IsAny<Promotion>()), Times.Never);
        }

        [Fact]
        public async Task GetPromotionAsync_UserAuthorizedForDifferentStore_ThrowsException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var differentStoreId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = differentStoreId };

            var promotions = new List<Promotion> { promotion };
            SetupPromotionQueryable(promotions);
            SetupValidUser(userId, user, userStore);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.GetPromotionAsync(promotionId, userId));

            Assert.Equal(Errors.AuthError.UserUnauthor + "@" + Errors.FieldName.UserId, exception.Message);

            // Verify calls
            _promotionRepositoryMock.Verify(r => r.AsQueryable(), Times.Once);
            _userManagerMock.Verify(um => um.FindByIdAsync(userId), Times.Once);
            _userStoreRepositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()), Times.Once);
            _mapperMock.Verify(m => m.Map<PromotionDTO>(It.IsAny<Promotion>()), Times.Never);
        }

        [Fact]
        public async Task GetPromotionAsync_PromotionWithBuyXGetYType_ReturnsPromotionDTO()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            promotion.Title = "Buy 2 Get 1 Free";
            promotion.PromotionType = PromotionType.BuyXGetY;
            promotion.PromotionItemConditions = new List<PromotionItemCondition>
            {
                new PromotionItemCondition
                {
                    Id = Guid.NewGuid(),
                    PromotionId = promotion.Id,
                    BuyItemId = Guid.NewGuid(),
                    GetItemId = Guid.NewGuid(),
                    BuyQuantity = 2,
                    GetQuantity = 1
                }
            };

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            var expectedPromotionDTO = new PromotionDTO
            {
                Id = promotionId,
                Title = "Buy 2 Get 1 Free",
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                PromotionType = PromotionType.BuyXGetY,
                IsActive = true
            };

            var promotions = new List<Promotion> { promotion };
            SetupPromotionQueryable(promotions);
            SetupValidUser(userId, user, userStore);

            _mapperMock.Setup(m => m.Map<PromotionDTO>(It.Is<Promotion>(p => p.Id == promotionId)))
                .Returns(expectedPromotionDTO);

            // Act
            var result = await _promotionService.GetPromotionAsync(promotionId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(promotionId, result.Id);
            Assert.Equal("Buy 2 Get 1 Free", result.Title);
            Assert.Equal(PromotionType.BuyXGetY, result.PromotionType);

            // Verify all mocks were called
            _promotionRepositoryMock.Verify(r => r.AsQueryable(), Times.Once);
            _userManagerMock.Verify(um => um.FindByIdAsync(userId), Times.Once);
            _userStoreRepositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()), Times.Once);
            _mapperMock.Verify(m => m.Map<PromotionDTO>(It.IsAny<Promotion>()), Times.Once);
        }
    }
}