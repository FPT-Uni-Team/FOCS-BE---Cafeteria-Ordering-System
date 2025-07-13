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
    public class PromotionServiceTests
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

        public PromotionServiceTests()
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

            SetupValidUserAndStore(userId, storeId, user, userStore);
            SetupValidStore(storeId, store);
            SetupValidCoupon(couponId, storeId, coupon);
            SetupValidPromotionDto(promotionDto);
            SetupValidPromotionUniqueness(promotionDto, storeId);
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

            SetupValidUserAndStore(userId, storeId, user, userStore);
            SetupValidStore(storeId, store);
            SetupValidCoupon(couponId, storeId, coupon);
            SetupValidPromotionDto(promotionDto);
            SetupValidPromotionUniqueness(promotionDto, storeId);
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

            SetupValidUserAndStore(userId, storeId, user, userStore);

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

            SetupValidUserAndStore(userId, storeId, user, userStore);
            SetupValidPromotionDto(promotionDto);

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

            SetupValidUserAndStore(userId, storeId, user, userStore);
            SetupValidPromotionDto(promotionDto);

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

            SetupValidUserAndStore(userId, storeId, user, userStore);
            SetupValidPromotionDto(promotionDto);
            SetupValidPromotionUniqueness(promotionDto, storeId);

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

            SetupValidUserAndStore(userId, storeId, user, userStore);
            SetupValidStore(storeId, store);
            SetupValidPromotionDto(promotionDto);
            SetupValidPromotionUniqueness(promotionDto, storeId);

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

            SetupValidUserAndStore(userId, storeId, user, userStore);
            SetupValidStore(storeId, store);
            SetupValidPromotionDto(promotionDto);
            SetupValidPromotionUniqueness(promotionDto, storeId);

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

            SetupValidUserAndStore(userId, storeId, user, userStore);
            SetupValidPromotionDto(promotionDto);

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

            SetupValidUserAndStore(userId, storeId, user, userStore);
            SetupValidStore(storeId, store);
            SetupValidCoupon(couponId, storeId, coupon);
            SetupValidPromotionDto(promotionDto);
            SetupValidPromotionUniqueness(promotionDto, storeId);
            SetupMapperForCreation(promotionDto, promotionEntity, resultDto);

            // Setup invalid buy menu item
            _menuItemRepositoryMock.Setup(x => x.GetByIdAsync(buyItemId))
                .ReturnsAsync((MenuItem)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _promotionService.CreatePromotionAsync(promotionDto, storeId, userId));
        }

        // Helper methods
        private PromotionDTO CreateValidPromotionDTO(List<Guid> couponIds = null)
        {
            return new PromotionDTO
            {
                Id = Guid.NewGuid(),
                Title = "Test Promotion",
                PromotionType = PromotionType.FixedAmount,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountValue = 100,
                CouponIds = couponIds ?? new List<Guid>(),
                AcceptForItems = new List<Guid>()
            };
        }

        private void SetupValidUserAndStore(string userId, Guid storeId, User user, UserStore userStore)
        {
            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userStoreRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(new List<UserStore> { userStore });
        }

        private void SetupValidStore(Guid storeId, Store store)
        {
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync(store);
        }

        private void SetupValidCoupon(Guid couponId, Guid storeId, Coupon coupon)
        {
            var couponQueryable = new List<Coupon> { coupon }.AsQueryable().BuildMockDbSet();
            _couponRepositoryMock.Setup(x => x.AsQueryable())
                .Returns(couponQueryable.Object);
        }

        private void SetupValidPromotionDto(PromotionDTO dto)
        {
            // Already handled in CreateValidPromotionDTO
        }

        private void SetupValidPromotionUniqueness(PromotionDTO dto, Guid storeId)
        {
            var titleCheckQueryable = new List<Promotion>().AsQueryable().BuildMockDbSet();
            var overlapCheckQueryable = new List<Promotion>().AsQueryable().BuildMockDbSet();

            _promotionRepositoryMock.SetupSequence(x => x.AsQueryable())
                .Returns(titleCheckQueryable.Object)
                .Returns(overlapCheckQueryable.Object);
        }

        private void SetupMapperForCreation(PromotionDTO dto, Promotion entity, PromotionDTO resultDto)
        {
            _mapperMock.Setup(x => x.Map<Promotion>(dto))
                .Returns(entity);
            _mapperMock.Setup(x => x.Map<PromotionDTO>(It.IsAny<Promotion>()))
                .Returns(resultDto);
        }
    }
}