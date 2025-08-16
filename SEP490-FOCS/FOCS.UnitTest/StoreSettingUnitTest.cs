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
using MockQueryable.Moq;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace FOCS.UnitTest
{
    public class StoreSettingServiceTests
    {
        private readonly Mock<IRepository<StoreSetting>> _mockStoreSettingRepository;
        private readonly Mock<IRepository<UserStore>> _mockUserStoreRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly StoreSettingService _storeSettingService;

        public StoreSettingServiceTests()
        {
            _mockStoreSettingRepository = new Mock<IRepository<StoreSetting>>();
            _mockUserStoreRepository = new Mock<IRepository<UserStore>>();
            _mockUserManager = new Mock<UserManager<User>>(new Mock<IUserStore<User>>().Object, null, null, null, null, null, null, null, null);
            _mockMapper = new Mock<IMapper>();

            _storeSettingService = new StoreSettingService(
                _mockStoreSettingRepository.Object,
                _mockUserStoreRepository.Object,
                _mockUserManager.Object,
                _mockMapper.Object
            );
        }

        #region CreateStoreSettingAsync Tests (Setting Store Config)

        [Theory]
        [InlineData("", "17:00:00", "VNĐ", 0, 0, true, true, 1)]
        [InlineData(null, "17:00:00", "VNĐ", 0, 0, true, true, 1)]
        [InlineData("07:00:00", "", "VNĐ", 0, 0, true, true, 1)]
        [InlineData("07:00:00", null, "VNĐ", 0, 0, true, true, 1)]
        [InlineData("07:00:00", "17:00:00", "VNĐ", 0, 0, true, true, 0)]
        [InlineData("07:00:00", "17:00:00", "VNĐ", 0, 0, true, true, -1)]
        public async Task CreateStoreSettingAsync_WithInvalidInput_ShouldValidateFalse(
            string openTime, string closeTime, string currency, int paymentConfig, int discountStrategy,
            bool isSelfService, bool allowCombinePromotionAndCoupon, int? spendingRate)
        {
            // Arrange
            var dto = new StoreSettingDTO
            {
                OpenTime = TimeSpan.TryParse(openTime, out var openTimeSpan) ? openTimeSpan : TimeSpan.Zero,
                CloseTime = TimeSpan.TryParse(closeTime, out var closeTimeSpan) ? closeTimeSpan : TimeSpan.Zero,
                Currency = currency,
                PaymentConfig = (PaymentConfig)paymentConfig,
                DiscountStrategy = (DiscountStrategy)discountStrategy,
                IsSelfService = isSelfService,
                AllowCombinePromotionAndCoupon = allowCombinePromotionAndCoupon,
                SpendingRate = spendingRate
            };

            // Act
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (string.IsNullOrEmpty(openTime) || string.IsNullOrEmpty(closeTime))
            {
                validationResults.Add(new ValidationResult("OpenTime and CloseTime cannot be empty or null."));
            }
            else
            {
                Validator.TryValidateObject(dto, validationContext, validationResults, true);
            }


            //Assert
            Assert.False(validationResults.Count == 0);
        }

        [Theory]
        [InlineData("07:00:00", "17:00:00", "VNĐ", 0, 0, true, true, 1)]
        [InlineData("07:00:00", "17:00:00", "", 0, 0, true, true, 1)]
        [InlineData("07:00:00", "17:00:00", null, 0, 0, true, true, 1)]
        [InlineData("07:00:00", "17:00:00", "VNĐ", null, 0, true, true, 1)]
        [InlineData("07:00:00", "17:00:00", "VNĐ", 0, null, true, true, 1)]
        [InlineData("07:00:00", "17:00:00", "VNĐ", 0, 0, false, true, 1)]
        [InlineData("07:00:00", "17:00:00", "VNĐ", 0, 0, null, true, 1)]
        [InlineData("07:00:00", "17:00:00", "VNĐ", 0, 0, true, false, 1)]
        [InlineData("07:00:00", "17:00:00", "VNĐ", 0, 0, true, null, 1)]
        [InlineData("07:00:00", "17:00:00", "VNĐ", 0, 0, true, true, null)]
        public async Task CreateStoreSettingAsync_WithVariousInputs_ShouldReturnExpectedResult(
            string openTime, string closeTime, string currency, int paymentConfig, int discountStrategy,
            bool isSelfService, bool allowCombinePromotionAndCoupon, int? spendingRate)
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var dto = new StoreSettingDTO
            {
                OpenTime = TimeSpan.Parse(openTime),
                CloseTime = TimeSpan.Parse(closeTime),
                Currency = currency,
                PaymentConfig = (PaymentConfig)paymentConfig,
                DiscountStrategy = (DiscountStrategy)discountStrategy,
                IsSelfService = isSelfService,
                AllowCombinePromotionAndCoupon = allowCombinePromotionAndCoupon,
                SpendingRate = spendingRate
            };

            // Setup mock for existing store setting check (should return null for creation)
            var mockQueryable = new List<StoreSetting>().AsQueryable().BuildMockDbSet();
            _mockStoreSettingRepository.Setup(r => r.AsQueryable()).Returns(mockQueryable.Object);

            var newStoreSetting = new StoreSetting
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                OpenTime = dto.OpenTime,
                CloseTime = dto.CloseTime,
                Currency = dto.Currency,
                PaymentConfig = dto.PaymentConfig,
                discountStrategy = dto.DiscountStrategy,
                IsSelfService = dto.IsSelfService,
                AllowCombinePromotionAndCoupon = dto.AllowCombinePromotionAndCoupon,
                SpendingRate = dto.SpendingRate ?? 1
            };

            _mockMapper.Setup(m => m.Map<StoreSetting>(dto)).Returns(newStoreSetting);
            _mockMapper.Setup(m => m.Map<StoreSettingDTO>(It.IsAny<StoreSetting>())).Returns(dto);
            _mockStoreSettingRepository.Setup(r => r.AddAsync(It.IsAny<StoreSetting>())).Returns(Task.CompletedTask);
            _mockStoreSettingRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _storeSettingService.CreateStoreSettingAsync(storeId, dto, userId);

            // Assert
            Assert.NotNull(result);
            _mockStoreSettingRepository.Verify(r => r.AddAsync(It.IsAny<StoreSetting>()), Times.Once);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region UpdateStoreSettingAsync Tests

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "", "17:00:00", "VNĐ", 0, 0, true, true, 1, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", null, "17:00:00", "VNĐ", 0, 0, true, true, 1, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "", "VNĐ", 0, 0, true, true, 1, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", null, "VNĐ", 0, 0, true, true, 1, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", "VNĐ", 0, 0, true, true, 0, false)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", "VNĐ", 0, 0, true, true, -1, false)]
        public async Task UpdateStoreSettingAsync_WithInvalidInput_ShouldValidateFalse(
            string storeIdString, string openTime, string closeTime, string currency, int paymentConfig,
            int discountStrategy, bool isSelfService, bool allowCombinePromotionAndCoupon, int? spendingRate, bool shouldSucceed)
        {
            // Arrange
            var dto = new StoreSettingDTO
            {
                OpenTime = TimeSpan.TryParse(openTime, out var openTimeSpan) ? openTimeSpan : TimeSpan.Zero,
                CloseTime = TimeSpan.TryParse(closeTime, out var closeTimeSpan) ? closeTimeSpan : TimeSpan.Zero,
                Currency = currency,
                PaymentConfig = (PaymentConfig)paymentConfig,
                DiscountStrategy = (DiscountStrategy)discountStrategy,
                IsSelfService = isSelfService,
                AllowCombinePromotionAndCoupon = allowCombinePromotionAndCoupon,
                SpendingRate = spendingRate
            };

            // Act
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (string.IsNullOrEmpty(openTime) || string.IsNullOrEmpty(closeTime))
            {
                validationResults.Add(new ValidationResult("OpenTime and CloseTime cannot be empty or null."));
            }
            else
            {
                Validator.TryValidateObject(dto, validationContext, validationResults, true);
            }


            //Assert
            Assert.False(validationResults.Count == 0);
        }

        [Theory]
        [InlineData("4fa85f64-5717-4562-b3fc-2c963f66afa7", "07:00:00", "17:00:00", "VNĐ", 0, 0, true, true, 1)]
        [InlineData("3fa85f64-5717-4562-b3fc", "07:00:00", "17:00:00", "VNĐ", 0, 0, true, true, 1)]
        [InlineData(null, "07:00:00", "17:00:00", "VNĐ", 0, 0, true, true, 1)]
        public async Task UpdateStoreSettingAsync_WithVariousInputs_ShouldThrowException(
            string storeIdString, string openTime, string closeTime, string currency, int paymentConfig,
            int discountStrategy, bool isSelfService, bool allowCombinePromotionAndCoupon, int? spendingRate)
        {
            // Arrange
            Guid? storeId = null;
            if (!string.IsNullOrEmpty(storeIdString) && Guid.TryParse(storeIdString, out var parsedId))
            {
                storeId = parsedId;
            }
            var userId = Guid.NewGuid().ToString();

            var dto = new StoreSettingDTO
            {
                OpenTime = TimeSpan.Parse(openTime),
                CloseTime = TimeSpan.Parse(closeTime),
                Currency = currency,
                PaymentConfig = (PaymentConfig)paymentConfig,
                DiscountStrategy = (DiscountStrategy)discountStrategy,
                IsSelfService = isSelfService,
                AllowCombinePromotionAndCoupon = allowCombinePromotionAndCoupon,
                SpendingRate = spendingRate
            };

            var mockQueryable = new List<StoreSetting>().AsQueryable().BuildMockDbSet();
            _mockStoreSettingRepository.Setup(r => r.AsQueryable()).Returns(mockQueryable.Object);

            // Act
            var result = await _storeSettingService.UpdateStoreSettingAsync(storeId.HasValue ? storeId.Value : Guid.Empty, dto, userId);
            Assert.False(result);
        }

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", "VNĐ", 0, 0, true, true, 1)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", "", 0, 0, true, true, 1)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", null, 0, 0, true, true, 1)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", "VNĐ", null, 0, true, true, 1)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", "VNĐ", 0, null, true, true, 1)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", "VNĐ", 0, 0, false, true, 1)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", "VNĐ", 0, 0, null, true, 1)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", "VNĐ", 0, 0, true, false, 1)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", "VNĐ", 0, 0, true, null, 1)]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "07:00:00", "17:00:00", "VNĐ", 0, 0, true, true, null)]
        public async Task UpdateStoreSettingAsync_WithVariousInputs_ShouldReturnTrue(
            string storeIdString, string openTime, string closeTime, string currency, int paymentConfig,
            int discountStrategy, bool isSelfService, bool allowCombinePromotionAndCoupon, int? spendingRate)
        {
            // Arrange
            var storeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
            var userId = Guid.NewGuid().ToString();

            var dto = new StoreSettingDTO
            {
                OpenTime = TimeSpan.Parse(openTime),
                CloseTime = TimeSpan.Parse(closeTime),
                Currency = currency,
                PaymentConfig = (PaymentConfig)paymentConfig,
                DiscountStrategy = (DiscountStrategy)discountStrategy,
                IsSelfService = isSelfService,
                AllowCombinePromotionAndCoupon = allowCombinePromotionAndCoupon,
                SpendingRate = spendingRate
            };

            // Setup existing store setting based on whether it should exist
            var existingStoreSetting = storeIdString == "4fa85f64-5717-4562-b3fc-2c963f66afa7" ? null :
                new StoreSetting
                {
                    Id = Guid.NewGuid(),
                    StoreId = storeId,
                    IsDeleted = false,
                    OpenTime = TimeSpan.Parse("08:00:00"),
                    CloseTime = TimeSpan.Parse("18:00:00"),
                    Currency = "USD",
                    PaymentConfig = PaymentConfig.Momo,
                    discountStrategy = DiscountStrategy.CouponThenPromotion,
                    IsSelfService = true,
                    AllowCombinePromotionAndCoupon = true,
                    SpendingRate = 1
                };

            var storeSettings = existingStoreSetting != null ? new List<StoreSetting> { existingStoreSetting } : new List<StoreSetting>();
            var mockQueryable = storeSettings.AsQueryable().BuildMockDbSet();
            _mockStoreSettingRepository.Setup(r => r.AsQueryable()).Returns(mockQueryable.Object);

            _mockMapper.Setup(m => m.Map(dto, existingStoreSetting)).Callback(() =>
            {
                // Simulate mapping behavior
                existingStoreSetting.OpenTime = dto.OpenTime;
                existingStoreSetting.CloseTime = dto.CloseTime;
                if (!string.IsNullOrEmpty(dto.Currency)) existingStoreSetting.Currency = dto.Currency;
            });

            _mockStoreSettingRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _storeSettingService.UpdateStoreSettingAsync(storeId, dto, userId);

            // Assert
            Assert.True(result);
            _mockStoreSettingRepository.Verify(r => r.SaveChangesAsync(), Times.Once);

            #endregion
        }
    }
}