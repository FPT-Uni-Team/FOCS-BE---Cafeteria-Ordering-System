using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using System.Linq.Expressions;

namespace FOCS.Tests.Application.Services
{
    public class PromotionServiceActivationTests : PromotionServiceTestBase
    {
        [Fact]
        public async Task ActivatePromotionAsync_WhenPromotionNotFound_ReturnsFalse()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync((Promotion)null);

            // Act
            var result = await _promotionService.ActivatePromotionAsync(promotionId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ActivatePromotionAsync_WhenPromotionIsDeleted_ReturnsFalse()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = true;

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            // Act
            var result = await _promotionService.ActivatePromotionAsync(promotionId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ActivatePromotionAsync_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.ActivatePromotionAsync(promotionId, userId));
        }

        [Fact]
        public async Task ActivatePromotionAsync_WhenUserUnauthorized_ThrowsException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var unauthorizedStoreId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = unauthorizedStoreId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.ActivatePromotionAsync(promotionId, userId));
        }

        [Fact]
        public async Task ActivatePromotionAsync_WhenPromotionAlreadyActive_ThrowsException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;
            promotion.IsActive = true; // Already active

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.ActivatePromotionAsync(promotionId, userId));
        }

        [Fact]
        public async Task ActivatePromotionAsync_WhenValidRequest_ActivatesPromotionSuccessfully()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;
            promotion.IsActive = false; // Not active

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            _promotionRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _promotionService.ActivatePromotionAsync(promotionId, userId);

            // Assert
            Assert.True(result);
            Assert.True(promotion.IsActive);
            Assert.Equal(userId, promotion.UpdatedBy);
            Assert.True(promotion.UpdatedAt <= DateTime.UtcNow);
            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeactivatePromotionAsync_WhenPromotionNotFound_ReturnsFalse()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync((Promotion)null);

            // Act
            var result = await _promotionService.DeactivatePromotionAsync(promotionId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeactivatePromotionAsync_WhenPromotionIsDeleted_ReturnsFalse()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = true;

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            // Act
            var result = await _promotionService.DeactivatePromotionAsync(promotionId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeactivatePromotionAsync_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.DeactivatePromotionAsync(promotionId, userId));
        }

        [Fact]
        public async Task DeactivatePromotionAsync_WhenUserUnauthorized_ThrowsException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var unauthorizedStoreId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = unauthorizedStoreId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.DeactivatePromotionAsync(promotionId, userId));
        }

        [Fact]
        public async Task DeactivatePromotionAsync_WhenPromotionAlreadyInactive_ThrowsException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;
            promotion.IsActive = false; // Already inactive

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.DeactivatePromotionAsync(promotionId, userId));
        }

        [Fact]
        public async Task DeactivatePromotionAsync_WhenValidRequest_DeactivatesPromotionSuccessfully()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;
            promotion.IsActive = true; // Active

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            _promotionRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _promotionService.DeactivatePromotionAsync(promotionId, userId);

            // Assert
            Assert.True(result);
            Assert.False(promotion.IsActive);
            Assert.Equal(userId, promotion.UpdatedBy);
            Assert.True(promotion.UpdatedAt <= DateTime.UtcNow);
            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ActivatePromotionAsync_WhenRepositorySaveThrowsException_PropagatesException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;
            promotion.IsActive = false;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            _promotionRepositoryMock.Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.ActivatePromotionAsync(promotionId, userId));
        }

        [Fact]
        public async Task DeactivatePromotionAsync_WhenRepositorySaveThrowsException_PropagatesException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;
            promotion.IsActive = true;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            _promotionRepositoryMock.Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.DeactivatePromotionAsync(promotionId, userId));
        }

        [Fact]
        public async Task ActivatePromotionAsync_WhenUserHasMultipleStores_AndStoreMatches_ActivatesSuccessfully()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var otherStoreId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;
            promotion.IsActive = false;

            var user = new User { Id = userId };
            var userStores = new List<UserStore>
            {
                new UserStore { UserId = Guid.Parse(userId), StoreId = storeId },
                new UserStore { UserId = Guid.Parse(userId), StoreId = otherStoreId }
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userStoreRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);

            _promotionRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _promotionService.ActivatePromotionAsync(promotionId, userId);

            // Assert
            Assert.True(result);
            Assert.True(promotion.IsActive);
        }

        [Fact]
        public async Task DeactivatePromotionAsync_WhenUserHasMultipleStores_AndStoreMatches_DeactivatesSuccessfully()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var otherStoreId = Guid.NewGuid();

            var promotion = CreateValidPromotion(storeId);
            promotion.IsDeleted = false;
            promotion.IsActive = true;

            var user = new User { Id = userId };
            var userStores = new List<UserStore>
            {
                new UserStore { UserId = Guid.Parse(userId), StoreId = storeId },
                new UserStore { UserId = Guid.Parse(userId), StoreId = otherStoreId }
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userStoreRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);

            _promotionRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _promotionService.DeactivatePromotionAsync(promotionId, userId);

            // Assert
            Assert.True(result);
            Assert.False(promotion.IsActive);
        }
    }
}