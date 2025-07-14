using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;

namespace FOCS.Tests.Application.Services
{
    public class CreatePromotionAsyncTests : PromotionServiceTestBase
    {
        [Fact]
        public async Task CreatePromotionAsync_ValidInput_ReturnsPromotionDTO()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var couponId = Guid.NewGuid();
            var promotionDto = CreateValidPromotionDTO(new List<Guid> { couponId });
            var user = new User { Id = userId };
            var store = new Store { Id = storeId };
            var coupon = new Coupon
            {
                Id = couponId, 
                StoreId = storeId,
                StartDate = promotionDto.StartDate.AddDays(1),
                EndDate = promotionDto.EndDate.AddDays(-1),
                PromotionId = null
            };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var promotionEntity = new Promotion { Id = Guid.NewGuid(), Title = promotionDto.Title };
            var resultDto = new PromotionDTO { Id = promotionEntity.Id, Title = promotionDto.Title };

            SetupValidUser(userId, user, userStore);
            SetupValidStore(storeId, store);
            SetupValidCoupon(coupon);

            SetupValidPromotionUniqueness();
            SetupMapperForCreation(promotionDto, promotionEntity, resultDto);

            // Act
            var result = await _promotionService.CreatePromotionAsync(promotionDto, storeId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(resultDto.Id, result.Id);
            Assert.Equal(resultDto.Title, result.Title);

            _promotionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Promotion>()), Times.Once);
            _promotionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePromotionAsync_BuyXGetYPromotion_CreatesPromotionItemCondition()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var couponId = Guid.NewGuid();
            var buyItemId = Guid.NewGuid();
            var getItemId = Guid.NewGuid();

            var promotionDto = CreateValidPromotionDTO(new List<Guid> { couponId });
            promotionDto.PromotionType = PromotionType.BuyXGetY;
            promotionDto.PromotionItemConditionDTO = new PromotionItemConditionDTO
            {
                BuyItemId = buyItemId,
                GetItemId = getItemId,
                BuyQuantity = 2,
                GetQuantity = 1
            };

            var user = new User { Id = userId };
            var store = new Store { Id = storeId };
            var coupon = new Coupon
            {
                Id = couponId,
                StoreId = storeId,
                StartDate = promotionDto.StartDate.AddDays(1),
                EndDate = promotionDto.EndDate.AddDays(-1),
                PromotionId = null
            };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var promotionEntity = new Promotion
            {
                Id = Guid.NewGuid(),
                Title = promotionDto.Title,
                PromotionType = PromotionType.BuyXGetY
            };
            var resultDto = new PromotionDTO { Id = promotionEntity.Id, Title = promotionDto.Title };
            var buyMenuItem = new MenuItem { Id = buyItemId };
            var getMenuItem = new MenuItem { Id = getItemId };
            var promotionItemCondition = new PromotionItemCondition
            {
                Id = Guid.NewGuid(),
                PromotionId = promotionEntity.Id,
                BuyItemId = buyItemId,
                GetItemId = getItemId
            };

            SetupValidUser(userId, user, userStore);
            SetupValidStore(storeId, store);
            SetupValidCoupon(coupon);
            SetupValidPromotionUniqueness();
            SetupMapperForCreation(promotionDto, promotionEntity, resultDto);

            // Setup menu items validation
            _menuItemRepositoryMock.Setup(x => x.GetByIdAsync(buyItemId))
                .ReturnsAsync(buyMenuItem);
            _menuItemRepositoryMock.Setup(x => x.GetByIdAsync(getItemId))
                .ReturnsAsync(getMenuItem);

            // Setup promotion item condition repository
            var promotionItemConditionQueryable = new List<PromotionItemCondition>().AsQueryable().BuildMockDbSet();
            _promotionItemConditionRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(promotionItemConditionQueryable.Object);

            _mapperMock.Setup(x => x.Map<PromotionItemCondition>(promotionDto.PromotionItemConditionDTO))
                .Returns(promotionItemCondition);

            // Act
            var result = await _promotionService.CreatePromotionAsync(promotionDto, storeId, userId);

            // Assert
            Assert.NotNull(result);
            _promotionItemConditionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PromotionItemCondition>()), Times.Once);
            _promotionItemConditionRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePromotionAsync_InvalidUser_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var promotionDto = CreateValidPromotionDTO();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.CreatePromotionAsync(promotionDto, storeId, userId));
        }

        [Fact]
        public async Task CreatePromotionAsync_UserNotAuthorizedForStore_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var promotionDto = CreateValidPromotionDTO();
            var user = new User { Id = userId };
            var differentStoreId = Guid.NewGuid();
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = differentStoreId };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userStoreRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(new List<UserStore> { userStore });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.CreatePromotionAsync(promotionDto, storeId, userId));
        }

        [Fact]
        public async Task CreatePromotionAsync_InvalidPromotionDto_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var promotionDto = CreateValidPromotionDTO();
            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            SetupValidUser(userId, user, userStore);

            var invalidDto = new PromotionDTO
            {
                Title = "Invalid Promotion",
                PromotionType = PromotionType.FixedAmount,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountValue = 100,
                CouponIds = new List<Guid>(),
                AcceptForItems = new List<Guid>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.CreatePromotionAsync(invalidDto, storeId, userId));
        }

        [Fact]
        public async Task CreatePromotionAsync_DuplicatePromotionTitle_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var promotionDto = CreateValidPromotionDTO();
            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var existingPromotion = new Promotion
            {
                Id = Guid.NewGuid(),
                Title = promotionDto.Title,
                IsDeleted = false
            };

            SetupValidUser(userId, user, userStore);


            var promotionQueryable = new List<Promotion> { existingPromotion }.AsQueryable().BuildMockDbSet();
            _promotionRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(promotionQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.CreatePromotionAsync(promotionDto, storeId, userId));
        }

        [Fact]
        public async Task CreatePromotionAsync_OverlappingPromotion_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var promotionDto = CreateValidPromotionDTO();
            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var overlappingPromotion = new Promotion
            {
                Id = Guid.NewGuid(),
                Title = "Different Title",
                PromotionType = promotionDto.PromotionType,
                StoreId = storeId,
                StartDate = promotionDto.StartDate,
                EndDate = promotionDto.EndDate,
                IsDeleted = false
            };

            SetupValidUser(userId, user, userStore);

            // Setup two different calls to AsQueryable for title check and overlap check
            var titleCheckQueryable = new List<Promotion>().AsQueryable().BuildMockDbSet();
            var overlapCheckQueryable = new List<Promotion> { overlappingPromotion }.AsQueryable().BuildMockDbSet();

            _promotionRepositoryMock.SetupSequence(x => x.AsQueryable())
                .Returns(titleCheckQueryable.Object)
                .Returns(overlapCheckQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.CreatePromotionAsync(promotionDto, storeId, userId));
        }

        [Fact]
        public async Task CreatePromotionAsync_StoreNotFound_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var promotionDto = CreateValidPromotionDTO();
            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            SetupValidUser(userId, user, userStore);
            SetupValidPromotionUniqueness();

            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync((Store)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.CreatePromotionAsync(promotionDto, storeId, userId));
        }

        [Fact]
        public async Task CreatePromotionAsync_InvalidCouponDates_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var couponId = Guid.NewGuid();
            var promotionDto = CreateValidPromotionDTO(new List<Guid> { couponId });
            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var store = new Store { Id = storeId };
            var coupon = new Coupon
            {
                Id = couponId,
                StoreId = storeId,
                StartDate = promotionDto.StartDate.AddDays(-1),
                EndDate = promotionDto.EndDate.AddDays(-1),
                PromotionId = null
            };

            SetupValidUser(userId, user, userStore);
            SetupValidStore(storeId, store);
            SetupValidPromotionUniqueness();

            var couponQueryable = new List<Coupon> { coupon }.AsQueryable().BuildMockDbSet();
            _couponRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(couponQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.CreatePromotionAsync(promotionDto, storeId, userId));
        }

        [Fact]
        public async Task CreatePromotionAsync_CouponAlreadyAssigned_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var couponId = Guid.NewGuid();
            var promotionDto = CreateValidPromotionDTO(new List<Guid> { couponId });
            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var store = new Store { Id = storeId };
            var coupon = new Coupon
            {
                Id = couponId,
                StoreId = storeId,
                StartDate = promotionDto.StartDate.AddDays(-1),
                EndDate = promotionDto.EndDate.AddDays(1),
                PromotionId = Guid.NewGuid() // Already assigned to another promotion
            };

            SetupValidUser(userId, user, userStore);
            SetupValidStore(storeId, store);
            SetupValidPromotionUniqueness();

            var couponQueryable = new List<Coupon> { coupon }.AsQueryable().BuildMockDbSet();
            _couponRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(couponQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.CreatePromotionAsync(promotionDto, storeId, userId));
        }

        [Fact]
        public async Task CreatePromotionAsync_AcceptForItemsWithInvalidMenuItem_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var invalidMenuItemId = Guid.NewGuid();
            var promotionDto = CreateValidPromotionDTO();
            promotionDto.AcceptForItems = new List<Guid> { invalidMenuItemId };
            var user = new User { Id = userId };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };

            SetupValidUser(userId, user, userStore);

            // Setup title uniqueness check - return empty list
            var titleCheckQueryable = new List<Promotion>().AsQueryable().BuildMockDbSet();
            _promotionRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(titleCheckQueryable.Object);

            // Setup menu item not found
            _menuItemRepositoryMock.Setup(x => x.GetByIdAsync(invalidMenuItemId))
                .ReturnsAsync((MenuItem)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.CreatePromotionAsync(promotionDto, storeId, userId));
        }

        [Fact]
        public async Task CreatePromotionAsync_BuyXGetYWithInvalidBuyMenuItem_ThrowsException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var couponId = Guid.NewGuid();
            var buyItemId = Guid.NewGuid();
            var getItemId = Guid.NewGuid();

            var promotionDto = CreateValidPromotionDTO(new List<Guid> { couponId });
            promotionDto.PromotionType = PromotionType.BuyXGetY;
            promotionDto.PromotionItemConditionDTO = new PromotionItemConditionDTO
            {
                BuyItemId = buyItemId,
                GetItemId = getItemId,
                BuyQuantity = 2,
                GetQuantity = 1
            };

            var user = new User { Id = userId };
            var store = new Store { Id = storeId };
            var coupon = new Coupon
            {
                Id = couponId,
                StoreId = storeId,
                StartDate = promotionDto.StartDate.AddDays(-1),
                EndDate = promotionDto.EndDate.AddDays(1),
                PromotionId = null
            };
            var userStore = new UserStore { UserId = Guid.Parse(userId), StoreId = storeId };
            var promotionEntity = new Promotion
            {
                Id = Guid.NewGuid(),
                Title = promotionDto.Title,
                PromotionType = PromotionType.BuyXGetY
            };
            var resultDto = new PromotionDTO { Id = promotionEntity.Id, Title = promotionDto.Title };

            SetupValidUser(userId, user, userStore);
            SetupValidStore(storeId, store);
            SetupValidCoupon(coupon);
            SetupValidPromotionUniqueness();
            SetupMapperForCreation(promotionDto, promotionEntity, resultDto);

            // Setup invalid buy menu item
            _menuItemRepositoryMock.Setup(x => x.GetByIdAsync(buyItemId))
                .ReturnsAsync((MenuItem)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.CreatePromotionAsync(promotionDto, storeId, userId));
        }
    }
}