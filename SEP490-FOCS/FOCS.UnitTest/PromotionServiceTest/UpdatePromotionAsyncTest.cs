using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Enums;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.PromotionServiceTest
{
    public class UpdatePromotionAsyncTest : PromotionServiceTestBase
    {
        [Fact]
        public async Task UpdatePromotionAsync_WhenPromotionIdDoesNotMatchDtoId_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenPromotionNotFound_ShouldReturnFalse()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync((Promotion)null);

            // Act
            var result = await _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenPromotionIsDeleted_ShouldReturnFalse()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            promotion.IsDeleted = true;

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            // Act
            var result = await _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenUserNotFound_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenUserUnauthorized_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;

            var user = new User { Id = userId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userStoreRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(new List<UserStore>()); // Empty list - user has no stores

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenDtoValidationFails_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            dto.StartDate = DateTime.UtcNow.AddDays(-1);
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenPromotionTitleExists_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            var existingPromotionWithSameTitle = new Promotion
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                IsDeleted = false
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            var titleCheckQueryable = new List<Promotion> { existingPromotionWithSameTitle }.AsQueryable().BuildMockDbSet();
            _promotionRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(titleCheckQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenPromotionOverlaps_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            var overlappingPromotion = new Promotion
            {
                Id = Guid.NewGuid(),
                PromotionType = dto.PromotionType,
                StoreId = storeId,
                StartDate = dto.StartDate.AddDays(-1),
                EndDate = dto.EndDate.AddDays(1),
                IsDeleted = false
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            var titleCheckQueryable = new List<Promotion>().AsQueryable().BuildMockDbSet();
            var overlapCheckQueryable = new List<Promotion> { overlappingPromotion }.AsQueryable().BuildMockDbSet();

            _promotionRepositoryMock.SetupSequence(x => x.AsQueryable())
                .Returns(titleCheckQueryable.Object)
                .Returns(overlapCheckQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenStoreNotFound_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);
            SetupValidPromotionUniqueness();

            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync((Store)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenPromotionIsActiveAndOngoing_ShouldUpdateEndDateOnly()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            dto.EndDate = DateTime.UtcNow.AddDays(10);
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            promotion.IsActive = true;
            promotion.StartDate = DateTime.UtcNow.AddDays(-1); // Started yesterday
            promotion.EndDate = DateTime.UtcNow.AddDays(5); // Ends in 5 days

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var store = new Store { Id = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);
            SetupValidPromotionUniqueness();
            SetupValidStore(storeId, store);

            _promotionRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId);

            // Assert
            Assert.True(result);
            Assert.Equal(dto.EndDate, promotion.EndDate);
            Assert.Equal(DateTime.UtcNow.Date, promotion.UpdatedAt.Value.Date);
            Assert.Equal(userId, promotion.UpdatedBy);
            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenPromotionIsNotActiveOrNotOngoing_ShouldUpdateAllFields()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            dto.CouponIds = new List<Guid> { Guid.NewGuid() };
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            promotion.IsActive = false; // Not active

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var store = new Store { Id = storeId };

            var coupon = new Coupon
            {
                Id = dto.CouponIds[0],
                StoreId = storeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                PromotionId = null
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);
            SetupValidPromotionUniqueness();
            SetupValidStore(storeId, store);

            var couponQueryable = new List<Coupon> { coupon }.AsQueryable().BuildMockDbSet();
            _couponRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(couponQueryable.Object);

            _mapperMock.Setup(x => x.Map(dto, promotion))
                .Callback<PromotionDTO, Promotion>((src, dest) =>
                {
                    dest.Title = src.Title;
                    dest.DiscountValue = src.DiscountValue;
                });

            _promotionRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId);

            // Assert
            Assert.True(result);
            Assert.Contains(coupon, promotion.Coupons);
            Assert.Equal(DateTime.UtcNow.Date, promotion.UpdatedAt.Value.Date);
            Assert.Equal(userId, promotion.UpdatedBy);
            _mapperMock.Verify(x => x.Map(dto, promotion), Times.Once);
            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenCouponValidationFails_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            dto.CouponIds = new List<Guid> { Guid.NewGuid() };
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            promotion.IsActive = false;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var store = new Store { Id = storeId };

            var coupon = new Coupon
            {
                Id = dto.CouponIds[0],
                StoreId = storeId,
                StartDate = dto.StartDate.AddDays(1), // Invalid: coupon starts after promotion
                EndDate = dto.EndDate.AddDays(1),
                PromotionId = null
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);
            SetupValidPromotionUniqueness();
            SetupValidStore(storeId, store);

            var couponQueryable = new List<Coupon> { coupon }.AsQueryable().BuildMockDbSet();
            _couponRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(couponQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenCouponAlreadyAssigned_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            dto.CouponIds = new List<Guid> { Guid.NewGuid() };
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            promotion.IsActive = false;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var store = new Store { Id = storeId };

            var coupon = new Coupon
            {
                Id = dto.CouponIds[0],
                StoreId = storeId,
                StartDate = dto.StartDate.AddDays(-1),
                EndDate = dto.EndDate.AddDays(1),
                PromotionId = Guid.NewGuid() // Already assigned to another promotion
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);
            SetupValidPromotionUniqueness();
            SetupValidStore(storeId, store);

            var couponQueryable = new List<Coupon> { coupon }.AsQueryable().BuildMockDbSet();
            _couponRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(couponQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenPromotionTypeIsBuyXGetY_ShouldUpdatePromotionItemCondition()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            dto.PromotionType = PromotionType.BuyXGetY;
            dto.PromotionItemConditionDTO = new PromotionItemConditionDTO
            {
                BuyItemId = Guid.NewGuid(),
                GetItemId = Guid.NewGuid(),
                BuyQuantity = 2,
                GetQuantity = 1
            };
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            promotion.IsActive = false;
            promotion.PromotionType = PromotionType.BuyXGetY;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var store = new Store { Id = storeId };

            var buyMenuItem = new MenuItem { Id = dto.PromotionItemConditionDTO.BuyItemId };
            var getMenuItem = new MenuItem { Id = dto.PromotionItemConditionDTO.GetItemId };

            var existingCondition = new PromotionItemCondition
            {
                Id = Guid.NewGuid(),
                PromotionId = promotionId,
                BuyItemId = dto.PromotionItemConditionDTO.BuyItemId,
                GetItemId = dto.PromotionItemConditionDTO.GetItemId,
                BuyQuantity = 1,
                GetQuantity = 1
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);
            SetupValidPromotionUniqueness();
            SetupValidStore(storeId, store);

            _couponRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(new List<Coupon>().AsQueryable().BuildMockDbSet().Object);

            _menuItemRepositoryMock.Setup(x => x.GetByIdAsync(dto.PromotionItemConditionDTO.BuyItemId))
                .ReturnsAsync(buyMenuItem);
            _menuItemRepositoryMock.Setup(x => x.GetByIdAsync(dto.PromotionItemConditionDTO.GetItemId))
                .ReturnsAsync(getMenuItem);

            var conditionQueryable = new List<PromotionItemCondition> { existingCondition }.AsQueryable().BuildMockDbSet();
            _promotionItemConditionRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(conditionQueryable.Object);

            _mapperMock.Setup(x => x.Map(dto, promotion));
            _mapperMock.Setup(x => x.Map(dto.PromotionItemConditionDTO, existingCondition))
                .Callback<PromotionItemConditionDTO, PromotionItemCondition>((src, dest) =>
                {
                    dest.BuyQuantity = src.BuyQuantity;
                    dest.GetQuantity = src.GetQuantity;
                });

            _promotionRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _promotionItemConditionRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId);

            // Assert
            Assert.True(result);
            _mapperMock.Verify(x => x.Map(dto.PromotionItemConditionDTO, existingCondition), Times.Once);
            _promotionItemConditionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenPromotionTypeIsBuyXGetYAndNoExistingCondition_ShouldCreateNewCondition()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            dto.PromotionType = PromotionType.BuyXGetY;
            dto.PromotionItemConditionDTO = new PromotionItemConditionDTO
            {
                BuyItemId = Guid.NewGuid(),
                GetItemId = Guid.NewGuid(),
                BuyQuantity = 2,
                GetQuantity = 1
            };
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId; 
            promotion.IsActive = false;
            promotion.PromotionType = PromotionType.BuyXGetY;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var store = new Store { Id = storeId };

            var buyMenuItem = new MenuItem { Id = dto.PromotionItemConditionDTO.BuyItemId };
            var getMenuItem = new MenuItem { Id = dto.PromotionItemConditionDTO.GetItemId };

            var newCondition = new PromotionItemCondition
            {
                Id = Guid.NewGuid(),
                PromotionId = promotionId,
                BuyItemId = dto.PromotionItemConditionDTO.BuyItemId,
                GetItemId = dto.PromotionItemConditionDTO.GetItemId,
                BuyQuantity = dto.PromotionItemConditionDTO.BuyQuantity,
                GetQuantity = dto.PromotionItemConditionDTO.GetQuantity
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);
            SetupValidPromotionUniqueness();
            SetupValidStore(storeId, store);

            _couponRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(new List<Coupon>().AsQueryable().BuildMockDbSet().Object);

            _menuItemRepositoryMock.Setup(x => x.GetByIdAsync(dto.PromotionItemConditionDTO.BuyItemId))
                .ReturnsAsync(buyMenuItem);
            _menuItemRepositoryMock.Setup(x => x.GetByIdAsync(dto.PromotionItemConditionDTO.GetItemId))
                .ReturnsAsync(getMenuItem);

            var conditionQueryable = new List<PromotionItemCondition>().AsQueryable().BuildMockDbSet();
            _promotionItemConditionRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(conditionQueryable.Object);

            _mapperMock.Setup(x => x.Map(dto, promotion));
            _mapperMock.Setup(x => x.Map<PromotionItemCondition>(dto.PromotionItemConditionDTO))
                .Returns(newCondition);

            _promotionRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _promotionItemConditionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<PromotionItemCondition>()))
                .Returns(Task.CompletedTask);
            _promotionItemConditionRepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId);

            // Assert
            Assert.True(result);
            _mapperMock.Verify(x => x.Map<PromotionItemCondition>(dto.PromotionItemConditionDTO), Times.Once);
            _promotionItemConditionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PromotionItemCondition>()), Times.Once);
            _promotionItemConditionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenMenuItemNotFound_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            dto.AcceptForItems = new List<Guid> { Guid.NewGuid() };
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            promotion.IsActive = false;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);

            var titleCheckQueryable = new List<Promotion>().AsQueryable().BuildMockDbSet();
            _promotionRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(titleCheckQueryable.Object);

            _menuItemRepositoryMock.Setup(x => x.GetByIdAsync(dto.AcceptForItems[0]))
                .ReturnsAsync((MenuItem)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenCouponNotValidForPeriod_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            dto.CouponIds = new List<Guid> { Guid.NewGuid() };
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            promotion.IsActive = false;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var store = new Store { Id = storeId };

            var coupon = new Coupon
            {
                Id = dto.CouponIds[0],
                StoreId = storeId,
                StartDate = dto.StartDate.AddDays(1), // Invalid: starts after promotion
                EndDate = dto.EndDate.AddDays(1),
                PromotionId = null
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);
            SetupValidPromotionUniqueness();
            SetupValidStore(storeId, store);
            SetupValidCoupon(coupon);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenCouponAlreadyAssignedToOtherPromotion_ShouldThrowException()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            dto.CouponIds = new List<Guid> { Guid.NewGuid() };
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            promotion.IsActive = false;

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var store = new Store { Id = storeId };

            var coupon = new Coupon
            {
                Id = dto.CouponIds[0],
                StoreId = storeId,
                StartDate = dto.StartDate.AddDays(-1),
                EndDate = dto.EndDate.AddDays(1),
                PromotionId = Guid.NewGuid() // Assigned to another promotion
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);
            SetupValidPromotionUniqueness();
            SetupValidStore(storeId, store);
            SetupValidCoupon(coupon);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId));
        }

        [Fact]
        public async Task UpdatePromotionAsync_WhenActivePromotionNotWithinPeriod_ShouldUpdateAllFields()
        {
            // Arrange
            var promotionId = Guid.NewGuid();
            var dto = CreateValidPromotionDTO();
            dto.Id = promotionId;
            dto.CouponIds = new List<Guid> { Guid.NewGuid() };
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var promotion = CreateValidPromotion(storeId);
            promotion.Id = promotionId;
            promotion.IsActive = true;
            promotion.StartDate = DateTime.UtcNow.AddDays(1); // Future promotion
            promotion.EndDate = DateTime.UtcNow.AddDays(5);

            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var store = new Store { Id = storeId };

            var coupon = new Coupon
            {
                Id = dto.CouponIds[0],
                StoreId = storeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                PromotionId = null
            };

            _promotionRepositoryMock.Setup(x => x.GetByIdAsync(promotionId))
                .ReturnsAsync(promotion);

            SetupValidUser(userId, user, userStore);
            SetupValidPromotionUniqueness();
            SetupValidStore(storeId, store);
            SetupValidCoupon(coupon);

            _mapperMock.Setup(x => x.Map(dto, promotion))
                .Verifiable();

            // Act
            var result = await _promotionService.UpdatePromotionAsync(promotionId, dto, storeId, userId);

            // Assert
            Assert.True(result);
            Assert.Equal(DateTime.UtcNow.Date, promotion.UpdatedAt.Value.Date);
            Assert.Equal(userId, promotion.UpdatedBy);
            _mapperMock.Verify(x => x.Map(dto, promotion), Times.Once);
            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}