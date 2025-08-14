using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FOCS.UnitTest
{
    public class PromotionUnitTest
    {
        private readonly Mock<IRepository<Promotion>> _mockPromotionRepository;
        private readonly Mock<IRepository<Coupon>> _mockCouponRepository;
        private readonly Mock<IRepository<UserStore>> _mockUserStoreRepository;
        private readonly Mock<IRepository<CouponUsage>> _mockCouponUsageRepository;
        private readonly Mock<IRepository<PromotionItemCondition>> _mockPromotionItemConditionRepository;
        private readonly Mock<IRepository<Store>> _mockStoreRepository;
        private readonly Mock<IRepository<StoreSetting>> _mockStoreSettingRepository;
        private readonly Mock<IRepository<MenuItem>> _mockMenuItemRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IPricingService> _mockPricingService;
        private readonly PromotionService _promotionService;

        public PromotionUnitTest()
        {
            _mockPromotionRepository = new Mock<IRepository<Promotion>>();
            _mockCouponRepository = new Mock<IRepository<Coupon>>();
            _mockUserStoreRepository = new Mock<IRepository<UserStore>>();
            _mockCouponUsageRepository = new Mock<IRepository<CouponUsage>>();
            _mockPromotionItemConditionRepository = new Mock<IRepository<PromotionItemCondition>>();
            _mockStoreRepository = new Mock<IRepository<Store>>();
            _mockStoreSettingRepository = new Mock<IRepository<StoreSetting>>();
            _mockMenuItemRepository = new Mock<IRepository<MenuItem>>();
            _mockMapper = new Mock<IMapper>();
            _mockUserManager = MockUserManager();
            _mockPricingService = new Mock<IPricingService>();

            _promotionService = new PromotionService(
                _mockPromotionRepository.Object,
                _mockPromotionItemConditionRepository.Object,
                _mockStoreRepository.Object,
                _mockMenuItemRepository.Object,
                _mockCouponRepository.Object,
                _mockCouponUsageRepository.Object,
                _mockUserManager.Object,
                _mockMapper.Object,
                _mockStoreSettingRepository.Object,
                _mockUserStoreRepository.Object,
                _mockPricingService.Object);
        }

        private static Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        #region CreatePromotion Tests (CM-06)

        [Theory]
        [InlineData(null, "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 10.0, false)]
        [InlineData("New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 0.0, false)]
        public async Task CreatePromotionAsync_WithInvalidInput_ShouldValidateFalse(
            string title,
            string description,
            string startDateStr,
            string endDateStr,
            PromotionType promotionType,
            double? discountValue,
            bool shouldSucceed)
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            DateTime? startDate = string.IsNullOrEmpty(startDateStr) ? null : DateTime.Parse(startDateStr);
            DateTime? endDate = string.IsNullOrEmpty(endDateStr) ? null : DateTime.Parse(endDateStr);

            var dto = new PromotionDTO
            {
                Title = title,
                Description = description,
                StartDate = startDate ?? DateTime.MinValue,
                EndDate = endDate ?? DateTime.MinValue,
                PromotionType = promotionType,
                DiscountValue = discountValue,
                CouponIds = new List<Guid>(),
                PromotionItemConditionDTO = promotionType == PromotionType.BuyXGetY && shouldSucceed ?
                    new PromotionItemConditionDTO
                    {
                        BuyItemId = Guid.NewGuid(),
                        BuyQuantity = 5,
                        GetItemId = Guid.NewGuid(),
                        GetQuantity = 1
                    } : null
            };

            // Act
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 10.0, true)]
        [InlineData("Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 10.0, false)]
        [InlineData("New Promotion", null, "2025-09-16", "2025-09-17", PromotionType.Percentage, 10.0, true)]
        [InlineData("New Promotion", "Create New Promotion", null, "2025-09-17", PromotionType.Percentage, 10.0, false)]
        [InlineData("New Promotion", "Create New Promotion", "2025-09-16", "2025-09-15", PromotionType.Percentage, 10.0, false)]
        [InlineData("New Promotion", "Create New Promotion", "2025-06-15", "2025-09-17", PromotionType.Percentage, 10.0, false)]
        [InlineData("New Promotion", "Create New Promotion", "2025-09-16", null, PromotionType.Percentage, 10.0, false)]
        [InlineData("New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.BuyXGetY, null, true)]
        [InlineData("New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.BuyXGetY, null, false)]
        [InlineData("New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 101.0, false)]
        [InlineData("New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.BuyXGetY, 10.0, true)]
        public async Task CreatePromotionAsync_VariousInputs_ReturnsExpectedResult(
            string title,
            string description,
            string startDateStr,
            string endDateStr,
            PromotionType promotionType,
            double? discountValue,
            bool shouldSucceed)
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            DateTime? startDate = string.IsNullOrEmpty(startDateStr) ? null : DateTime.Parse(startDateStr);
            DateTime? endDate = string.IsNullOrEmpty(endDateStr) ? null : DateTime.Parse(endDateStr);

            var dto = new PromotionDTO
            {
                Title = title,
                Description = description,
                StartDate = startDate ?? DateTime.MinValue,
                EndDate = endDate ?? DateTime.MinValue,
                PromotionType = promotionType,
                DiscountValue = discountValue,
                CouponIds = new List<Guid>(),
                PromotionItemConditionDTO = promotionType == PromotionType.BuyXGetY && shouldSucceed ?
                    new PromotionItemConditionDTO
                    {
                        BuyItemId = Guid.NewGuid(),
                        BuyQuantity = 5,
                        GetItemId = Guid.NewGuid(),
                        GetQuantity = 1
                    } : null
            };

            // Setup mocks
            SetupCommonMocks(storeId, userId, title == "Promotion");

            if (shouldSucceed)
            {
                var newPromotion = new Promotion { Id = Guid.NewGuid(), Title = title };
                _mockMapper.Setup(m => m.Map<Promotion>(dto)).Returns(newPromotion);
                _mockMapper.Setup(m => m.Map<PromotionDTO>(It.IsAny<Promotion>())).Returns(dto);
            }

            // Act & Assert
            if (shouldSucceed)
            {
                var result = await _promotionService.CreatePromotionAsync(dto, storeId, userId);
                Assert.NotNull(result);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(
                    () => _promotionService.CreatePromotionAsync(dto, storeId, userId));
            }
        }

        #endregion

        #region GetPromotionsByStore Tests (CM-07)

        [Theory]
        [InlineData(1, 10, null, null, null, null, null, true)]
        [InlineData(1, 10, null, null, null, null, null, false, "4fa85f64-5717-4562-b3fc-2c963f66afa7")]
        [InlineData(1, 10, null, null, null, null, null, false, "3fa85f64-5717-4562-b3fc")]
        [InlineData(1, 10, null, null, null, null, null, false, null)]
        [InlineData(0, 10, null, null, null, null, null, false)]
        [InlineData(1, 0, null, null, null, null, null, false)]
        [InlineData(1, 10, "title", "ti", null, null, null, true)]
        [InlineData(1, 10, "ttt", "ti", null, null, null, true)]
        [InlineData(1, 10, null, null, "title", "asc", null, true)]
        [InlineData(1, 10, null, null, "aa", "asc", null, true)]
        [InlineData(1, 10, null, null, null, null, "{\"promotion_type\": 0}", true)]
        public async Task GetPromotionsByStoreAsync_VariousInputs_ReturnsExpectedResult(
            int page,
            int pageSize,
            string searchBy,
            string searchValue,
            string sortBy,
            string sortOrder,
            string filtersJson,
            bool shouldSucceed,
            string storeIdStr = "3fa85f64-5717-4562-b3fc-2c963f66afa7")
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            Guid? storeId = null;

            if (!string.IsNullOrEmpty(storeIdStr))
            {
                if (Guid.TryParse(storeIdStr, out var parsedStoreId))
                {
                    storeId = parsedStoreId;
                }
            }

            var query = new UrlQueryParameters
            {
                Page = page,
                PageSize = pageSize,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Filters = string.IsNullOrEmpty(filtersJson) ? null :
                    new Dictionary<string, string> { { "promotion_type", "0" } }
            };

            // Setup mocks
            if (storeId.HasValue && shouldSucceed)
            {
                SetupCommonMocks(storeId.Value, userId);

                var promotions = new List<Promotion>
                {
                    new Promotion { Id = Guid.NewGuid(), Title = "Test Promotion", StoreId = storeId.Value }
                }.AsQueryable().BuildMockDbSet();

                _mockPromotionRepository.Setup(r => r.AsQueryable()).Returns(promotions.Object);
                _mockMapper.Setup(m => m.Map<List<PromotionDTO>>(It.IsAny<List<Promotion>>()))
                    .Returns(new List<PromotionDTO>());
            }

            // Act & Assert
            if (shouldSucceed && storeId.HasValue)
            {
                var result = await _promotionService.GetPromotionsByStoreAsync(query, storeId.Value, userId);
                Assert.NotNull(result);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(
                    () => _promotionService.GetPromotionsByStoreAsync(query, storeId.HasValue ? storeId.Value : Guid.Empty, userId));
            }
        }

        #endregion

        #region GetPromotionDetails Tests (CM-08)

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", true)]
        [InlineData("4fa85f64-5717-4562-b3fc-2c963f66afa7", false)]
        [InlineData("3fa85f64-5717-4562-b3fc", false)]
        [InlineData(null, false)]
        public async Task GetPromotionAsync_VariousInputs_ReturnsExpectedResult(
            string promotionIdStr,
            bool shouldSucceed)
        {
            // Arrange
            var storeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
            var userId = Guid.NewGuid().ToString();

            Guid? promotionId = null;
            if (!string.IsNullOrEmpty(promotionIdStr) && Guid.TryParse(promotionIdStr, out var parsedId))
            {
                promotionId = parsedId;
            }

            SetupCommonMocks(storeId, userId);

            var promotion = new Promotion
            {
                Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                StoreId = storeId,
                Title = "Test Promotion",
                IsDeleted = false
            };

            var promotionQueryable = new List<Promotion> { promotion }.AsQueryable().BuildMockDbSet();
            _mockPromotionRepository.Setup(r => r.AsQueryable())
                .Returns(promotionQueryable.Object);
            _mockMapper.Setup(m => m.Map<PromotionDTO>(It.IsAny<Promotion>())).Returns(new PromotionDTO());


            // Act & Assert
            if (shouldSucceed)
            {
                var result = await _promotionService.GetPromotionAsync(promotionId.Value, userId);
                Assert.NotNull(result);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(
                    () => _promotionService.GetPromotionAsync(promotionId.HasValue ? promotionId.Value : Guid.Empty, userId));
            }
        }

        #endregion

        #region UpdatePromotion Tests (CM-09)

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", null, "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 10.0, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 0.0, false)]
        public async Task UpdatePromotionAsync_WithInvalidInput_ShouldValidateFalse(
            string promotionIdStr,
            string title,
            string description,
            string startDateStr,
            string endDateStr,
            PromotionType promotionType,
            double? discountValue,
            bool shouldSucceed)
        {
            // Arrange
            var storeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
            var userId = Guid.NewGuid().ToString();

            Guid? promotionId = null;
            if (!string.IsNullOrEmpty(promotionIdStr) && Guid.TryParse(promotionIdStr, out var parsedId))
            {
                promotionId = parsedId;
            }

            DateTime? startDate = string.IsNullOrEmpty(startDateStr) ? null : DateTime.Parse(startDateStr);
            DateTime? endDate = string.IsNullOrEmpty(endDateStr) ? null : DateTime.Parse(endDateStr);

            var dto = new PromotionDTO
            {
                Id = promotionId ?? Guid.Empty,
                Title = title,
                Description = description,
                StartDate = startDate ?? DateTime.MinValue,
                EndDate = endDate ?? DateTime.MinValue,
                PromotionType = promotionType,
                DiscountValue = discountValue,
                CouponIds = new List<Guid>()
            };
        }

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 10.0, true)]
        [InlineData("4fa85f64-5717-4562-b3fc-2c963f66afa7", "New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 10.0, false)]
        [InlineData("3fa85f64-5717-4562-b3fc", "New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 10.0, false)]
        [InlineData(null, "New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 10.0, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 10.0, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "New Promotion", null, "2025-09-16", "2025-09-17", PromotionType.Percentage, 10.0, true)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "New Promotion", "Create New Promotion", "2025-06-15", "2025-09-17", PromotionType.Percentage, 10.0, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "New Promotion", "Create New Promotion", null, "2025-09-17", PromotionType.Percentage, 10.0, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "New Promotion", "Create New Promotion", "2025-09-16", "2025-06-15", PromotionType.Percentage, 10.0, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "New Promotion", "Create New Promotion", "2025-09-16", null, PromotionType.Percentage, 10.0, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.BuyXGetY, null, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", null, 10.0, true)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, 101.0, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "New Promotion", "Create New Promotion", "2025-09-16", "2025-09-17", PromotionType.Percentage, null, false)]
        public async Task UpdatePromotionAsync_VariousInputs_ReturnsExpectedResult(
            string promotionIdStr,
            string title,
            string description,
            string startDateStr,
            string endDateStr,
            PromotionType promotionType,
            double? discountValue,
            bool shouldSucceed)
        {
            // Arrange
            var storeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
            var userId = Guid.NewGuid().ToString();

            Guid? promotionId = null;
            if (!string.IsNullOrEmpty(promotionIdStr) && Guid.TryParse(promotionIdStr, out var parsedId))
            {
                promotionId = parsedId;
            }

            DateTime? startDate = string.IsNullOrEmpty(startDateStr) ? null : DateTime.Parse(startDateStr);
            DateTime? endDate = string.IsNullOrEmpty(endDateStr) ? null : DateTime.Parse(endDateStr);

            var dto = new PromotionDTO
            {
                Id = promotionId ?? Guid.Empty,
                Title = title,
                Description = description,
                StartDate = startDate ?? DateTime.MinValue,
                EndDate = endDate ?? DateTime.MinValue,
                PromotionType = promotionType,
                DiscountValue = discountValue,
                CouponIds = new List<Guid>()
            };

            if (promotionId.HasValue)
            {
                var existingPromotion = new Promotion
                {
                    Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    StoreId = storeId,
                    Title = "Old Title",
                    IsDeleted = false,
                    IsActive = false,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2)
                };

                _mockPromotionRepository.Setup(r => r.GetByIdAsync(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")))
                    .ReturnsAsync(existingPromotion);

                SetupCommonMocks(storeId, userId, title == "Promotion");
            }

            // Act & Assert
            if (shouldSucceed && promotionId.HasValue)
            {
                var result = await _promotionService.UpdatePromotionAsync(promotionId.Value, dto, storeId, userId);
                Assert.True(result);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(
                    () => _promotionService.UpdatePromotionAsync(promotionId.HasValue ? promotionId.Value : Guid.Empty, dto, storeId, userId));
            }
        }

        #endregion

        #region DeletePromotion Tests (CM-10)

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", true)]
        [InlineData("4fa85f64-5717-4562-b3fc-2c963f66afa7", false)]
        [InlineData("3fa85f64-5717-4562-b3fc", false)]
        [InlineData(null, false)] 
        public async Task DeletePromotionAsync_VariousInputs_ReturnsExpectedResult(
            string promotionIdStr,
            bool shouldSucceed)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

            Guid? promotionId = null;
            if (!string.IsNullOrEmpty(promotionIdStr) && Guid.TryParse(promotionIdStr, out var parsedId))
            {
                promotionId = parsedId;
            }

            if (shouldSucceed && promotionId.HasValue)
            {
                var existingPromotion = new Promotion
                {
                    Id = promotionId.Value,
                    StoreId = storeId,
                    Title = "Test Promotion",
                    IsDeleted = false
                };

                _mockPromotionRepository.Setup(r => r.GetByIdAsync(promotionId.Value))
                    .ReturnsAsync(existingPromotion);

                _mockCouponRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>()))
                    .ReturnsAsync(new List<Coupon>());

                SetupCommonMocks(storeId, userId);
            }

            // Act & Assert
            if (shouldSucceed)
            {
                var result = await _promotionService.DeletePromotionAsync(promotionId.Value, userId);
                Assert.True(result);
            }
            else
            {
                var result = await _promotionService.DeletePromotionAsync(promotionId.HasValue ? promotionId.Value : Guid.Empty, userId);
                Assert.False(result);
            }
        }

        #endregion

        #region Helper Methods

        private void SetupCommonMocks(Guid storeId, string userId, bool titleExists = false)
        {
            // Setup store validation
            _mockStoreRepository.Setup(r => r.GetByIdAsync(storeId))
                .ReturnsAsync(new Store { Id = storeId });

            // Setup user validation
            var user = new User { Id = userId };
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            var userStores = new List<UserStore>
            {
                new UserStore { UserId = Guid.Parse(userId), StoreId = storeId }
            };
            _mockUserStoreRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);

            // Setup promotion title uniqueness
            if (titleExists)
            {
                var existingPromotion = new Promotion { Title = "Promotion" };

                var promotionQueryable = new List<Promotion>{ existingPromotion }.AsQueryable().BuildMockDbSet();
                _mockPromotionRepository.Setup(r => r.AsQueryable())
                    .Returns(promotionQueryable.Object);
            }
            else
            {
                var promotionQueryable = new List<Promotion>().AsQueryable().BuildMockDbSet();
                _mockPromotionRepository.Setup(r => r.AsQueryable())
                    .Returns(promotionQueryable.Object);
            }

            // Setup coupon validation
            var couponQueryable = new List<Coupon>().AsQueryable().BuildMockDbSet();
            _mockCouponRepository.Setup(x => x.AsQueryable())
                .Returns(couponQueryable.Object);
        }

        #endregion
    }
}
