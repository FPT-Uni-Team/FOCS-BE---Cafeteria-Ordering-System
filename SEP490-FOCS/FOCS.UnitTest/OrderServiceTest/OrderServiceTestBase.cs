using AutoMapper;
using FOCS.Application.Services;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Models.CartModels;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FOCS.UnitTest.OrderServiceTest
{
    public abstract class OrderServiceTestBase
    {
        // Mocked dependencies
        protected readonly Mock<IRepository<Store>> _mockStoreRepository;
        protected readonly Mock<IRepository<MenuItem>> _mockMenuItemRepository;
        protected readonly Mock<IRepository<MenuItemVariant>> _mockVariantRepository;
        protected readonly Mock<IRepository<Table>> _mockTableRepository;
        protected readonly Mock<IRepository<FOCS.Order.Infrastucture.Entities.Order>> _mockOrderRepository;
        protected readonly Mock<IRepository<Coupon>> _mockCouponRepository;
        protected readonly Mock<IRepository<OrderDetail>> _mockOrderDetailRepository;
        protected readonly Mock<IMobileTokenSevice> _mockMobileTokenService;
        protected readonly Mock<IPricingService> _mockPricingService;
        protected readonly Mock<IPromotionService> _mockPromotionService;
        protected readonly Mock<DiscountContext> _mockDiscountContext;
        protected readonly Mock<IRepository<SystemConfiguration>> _mockSystemConfig;
        protected readonly Mock<IStoreSettingService> _mockStoreSettingService;
        protected readonly Mock<ICouponUsageService> _mockCouponUsageService;
        protected readonly Mock<IRealtimeService> _mockRealtimeService;
        protected readonly Mock<ILogger<OrderService>> _mockLogger;
        protected readonly Mock<IMapper> _mockMapper;
        protected readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        protected readonly Mock<UserManager<User>> _mockUserManager;

        protected readonly OrderService _orderService;
        protected readonly string _validUserId = Guid.NewGuid().ToString();
        protected readonly Guid _validStoreId = Guid.NewGuid();
        protected readonly Guid _validTableId = Guid.NewGuid();

        protected OrderServiceTestBase()
        {
            // Initialize mocks
            _mockStoreRepository = new Mock<IRepository<Store>>();
            _mockMenuItemRepository = new Mock<IRepository<MenuItem>>();
            _mockVariantRepository = new Mock<IRepository<MenuItemVariant>>();
            _mockTableRepository = new Mock<IRepository<Table>>();
            _mockOrderRepository = new Mock<IRepository<FOCS.Order.Infrastucture.Entities.Order>>();
            _mockCouponRepository = new Mock<IRepository<Coupon>>();
            _mockOrderDetailRepository = new Mock<IRepository<OrderDetail>>();
            _mockMobileTokenService = new Mock<IMobileTokenSevice>();
            _mockPricingService = new Mock<IPricingService>();
            _mockPromotionService = new Mock<IPromotionService>();
            _mockDiscountContext = new Mock<DiscountContext>();
            _mockSystemConfig = new Mock<IRepository<SystemConfiguration>>();
            _mockStoreSettingService = new Mock<IStoreSettingService>();
            _mockCouponUsageService = new Mock<ICouponUsageService>();
            _mockRealtimeService = new Mock<IRealtimeService>();
            _mockLogger = new Mock<ILogger<OrderService>>();
            _mockMapper = new Mock<IMapper>();
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();

            // UserManager requires special construction
            var storeMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(storeMock.Object, null, null, null, null, null, null, null, null);

            // Instantiate service
            _orderService = new OrderService(
                _mockOrderRepository.Object,
                _mockLogger.Object,
                _mockOrderDetailRepository.Object,
                _mockPricingService.Object,
                _mockCouponRepository.Object,
                null,
                _mockStoreSettingService.Object,
                _mockTableRepository.Object,
                _mockStoreRepository.Object,
                _mockMenuItemRepository.Object,
                _mockVariantRepository.Object,
                _mockPromotionService.Object,
                _mockMapper.Object,
                _mockRealtimeService.Object,
                _mockUserManager.Object,
                _mockSystemConfig.Object,
                _mockPublishEndpoint.Object,
                _mockMobileTokenService.Object,
                _mockCouponUsageService.Object
            );
        }

        // Helper to create a valid CreateOrderRequest
        protected CreateOrderRequest CreateValidOrderRequest()
        {
            return new CreateOrderRequest
            {
                StoreId = _validStoreId,
                TableId = _validTableId,
                Items = new List<OrderItemDTO>
                {
                    new OrderItemDTO { MenuItemId = Guid.NewGuid(), VariantId = null, Quantity = 1 }
                }
            };
        }

        // Helper to setup default repository returns
        protected void SetupDefaultStoreAndTable()
        {
            _mockStoreRepository.Setup(r => r.GetByIdAsync(_validStoreId))
                                .ReturnsAsync(new Store { Id = _validStoreId });

            _mockTableRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Table, bool>>>()))
                                .ReturnsAsync(new List<Table> { new Table { Id = _validTableId, StoreId = _validStoreId } });
        }

        // Helper to setup mock pricing
        protected void SetupDefaultPricing(double totalPrice)
        {
            _mockPricingService.Setup(p => p.CalculatePriceOfProducts(It.IsAny<Dictionary<Guid, Guid?>>(), _validStoreId.ToString()))
                               .ReturnsAsync(totalPrice);
        }

        // Helper to setup default store setting
        protected void SetupDefaultStoreSetting()
        {
            _mockStoreSettingService.Setup(s => s.GetStoreSettingAsync(_validStoreId, _validUserId))
                                    .ReturnsAsync(new StoreSettingDTO { DiscountStrategy = DiscountStrategy.CouponOnly });
        }

        // Helper to setup valid menu item and variant
        protected void SetupDefaultMenuAndVariant()
        {
            var item = new MenuItem { Id = _validStoreId, Name = "Item" };
            _mockMenuItemRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MenuItem, bool>>>()))
                                   .ReturnsAsync(new List<MenuItem> { item });
            _mockVariantRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MenuItemVariant, bool>>>()))
                                   .ReturnsAsync(new List<MenuItemVariant>());
        }
    }
}