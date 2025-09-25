using AutoMapper;
using FOCS.Application.Services;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.UnitTest
{
    public class OrderUnitTest
    {
        private readonly Mock<IRepository<OrderEntity>> _orderRepoMock = new();
        private readonly Mock<IRepository<OrderDetail>> _orderDetailRepoMock = new();
        private readonly Mock<IRepository<Store>> _storeRepoMock = new();
        private readonly Mock<IRepository<Table>> _tableRepoMock = new();
        private readonly Mock<IRepository<MenuItem>> _menuRepoMock = new();
        private readonly Mock<IRepository<MenuItemVariant>> _variantRepoMock = new();
        private readonly Mock<IRepository<Coupon>> _couponRepoMock = new();
        private readonly Mock<IRepository<SystemConfiguration>> _sysConfigMock = new();

        private readonly Mock<IPricingService> _pricingMock = new();
        private readonly Mock<IPromotionService> _promotionMock = new();
        private readonly Mock<IStoreSettingService> _storeSettingMock = new();
        private readonly Mock<IRealtimeService> _realtimeMock = new();
        private readonly Mock<ICouponUsageService> _couponUsageMock = new();
        private readonly Mock<IMobileTokenSevice> _mobileTokenMock = new();

        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger<OrderService>> _loggerMock = new();
        private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
        private readonly Mock<UserManager<User>> _userManagerMock;

        private readonly OrderService _orderService;

        public OrderUnitTest()
        {
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            _orderService = new OrderService(
                _orderRepoMock.Object,
                _loggerMock.Object,
                _orderDetailRepoMock.Object,
                _pricingMock.Object,
                _couponRepoMock.Object,
                null,
                _storeSettingMock.Object,
                _tableRepoMock.Object,
                _storeRepoMock.Object,
                _menuRepoMock.Object,
                _variantRepoMock.Object,
                _promotionMock.Object,
                _mapperMock.Object,
                _realtimeMock.Object,
                _userManagerMock.Object,
                _sysConfigMock.Object,
                _publishEndpointMock.Object,
                _mobileTokenMock.Object,
                _couponUsageMock.Object,
                null,
                null
            );
        }

        #region CreateOrder CM-42
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", PaymentType.CASH, OrderType.DineIn, true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", PaymentType.CASH, OrderType.DineIn, false)]
        [InlineData(null, "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", PaymentType.CASH, OrderType.DineIn, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "9dbfa424-24fc-43ef-af10-8c67f277115a", "String note", "Coupon code", PaymentType.CASH, OrderType.DineIn, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, "String note", "Coupon code", PaymentType.CASH, OrderType.DineIn, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", null, "Coupon code", PaymentType.CASH, OrderType.DineIn, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", null, PaymentType.CASH, OrderType.DineIn, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", PaymentType.ONLINE_PAYMENT, OrderType.DineIn, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", null, OrderType.DineIn, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", PaymentType.CASH, OrderType.PreOrder, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", PaymentType.CASH, null, false)]
        [InlineData(null, null, null, null, null, null, false)]
        public async Task CreateOrderAsync_SimpleRun_ChecksIfServiceRuns(
            string storeIdStr,
            string tableIdStr,
            string note,
            string couponCode,
            PaymentType paymentType,
            OrderType orderType,
            bool shouldSucceed)
        {
            // Arrange
            Guid? storeId = string.IsNullOrEmpty(storeIdStr) ? (Guid?)null : Guid.Parse(storeIdStr);
            Guid? tableId = string.IsNullOrEmpty(tableIdStr) ? (Guid?)null : Guid.Parse(tableIdStr);
            var userId = Guid.NewGuid().ToString();

            var request = new CreateOrderRequest
            {
                StoreId = storeId ?? Guid.Empty,
                TableId = tableId ?? Guid.Empty,
                Note = note,
                CouponCode = couponCode,
                PaymentType = paymentType,
                OrderType = orderType,
                Items = new List<OrderItemDTO>(),
                DiscountResult = new DiscountResultDTO()
            };

            // Setup mocks based on test case
            if (shouldSucceed)
            {
                _storeRepoMock.Setup(r => r.GetByIdAsync(storeId.Value))
                    .ReturnsAsync(new Store { Id = storeId.Value });

                _tableRepoMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Table>
                    {
                        new Table { Id = tableId.Value, StoreId = storeId.Value }
                    }.BuildMock());

                _storeSettingMock.Setup(s => s.GetStoreSettingAsync(storeId.Value, userId))
    .ReturnsAsync(new StoreSettingDTO { DiscountStrategy = 0 });
            }
            else
            {
                _storeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((Store)null);
            }

            // Act & Assert
            if (shouldSucceed)
            {
                var exception = await Record.ExceptionAsync(() =>
                    _orderService.CreateOrderAsync(request, userId));
                Assert.Null(exception);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(() =>
                    _orderService.CreateOrderAsync(request, userId));
            }
        }
        #endregion

        #region GetAllOrders CM-43
        [Theory]
        [InlineData(1, 10, "comment", "search comment", "created_date", "desc", true)]
        [InlineData(5, 10, "comment", "search comment", "created_date", "desc", true)]
        [InlineData(null, 10, "comment", "search comment", "created_date", "desc", false)]
        [InlineData(1, 20, "comment", "search comment", "created_date", "desc", true)]
        [InlineData(1, null, "comment", "search comment", "created_date", "desc", false)]
        [InlineData(1, 10, null, "search comment", "created_date", "desc", true)]
        [InlineData(1, 10, "comment", null, "created_date", "desc", true)]
        [InlineData(1, 10, "comment", "search comment", "rating", "desc", true)]
        [InlineData(1, 10, "comment", "search comment", null, "desc", true)]
        [InlineData(1, 10, "comment", "search comment", "created_date", "desc", true)]
        [InlineData(1, 10, "comment", "search comment", "created_date", "asc", true)]
        [InlineData(1, 10, "comment", "search comment", "created_date", null, true)]
        public async Task GetAllOrders_SimpleRun_ChecksIfServiceRuns(
            int? page, int? pageSize, string searchBy, string searchValue, string sortBy, string sortOrder, bool shouldSucceed)
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 10,
                SearchBy = searchBy,
                SearchValue = null,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var storeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();

            // Setup
            var orders = new List<OrderEntity> { new OrderEntity { Id = Guid.NewGuid(), StoreId = Guid.Parse(storeId) } };
            _orderRepoMock.Setup(r => r.AsQueryable())
                .Returns(orders.BuildMock());

            _mapperMock.Setup(m => m.Map<List<OrderDTO>>(It.IsAny<List<OrderEntity>>()))
                .Returns(new List<OrderDTO> { new OrderDTO() });

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _orderService.GetListOrders(query, storeId, userId);
            });

            Assert.Null(exception);
        }
        #endregion

        #region GetOrderDetail CM-44
        [Theory]
        [InlineData("550e8400-e29b-41d4-a716-446655440999", true)]
        [InlineData("550e8400-e29b-41d4-a716-446655440999", false)]
        //[InlineData(null, false)]
        public async Task GetOrderDetail_SimpleRun_ChecksIfServiceRuns(string orderIdStr, bool shouldSucceed)
        {
            var orderId = Guid.Parse(orderIdStr);
            var userId = Guid.NewGuid();

            Exception ex = await Record.ExceptionAsync(async () =>
            {
                var res = await _orderService.GetUserOrderDetailAsync(userId, orderId);
            });

            Assert.NotNull(ex);
        }
        #endregion

        #region ChangeStatusOrder CM-45
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", OrderStatus.CartSaved, true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", OrderStatus.CartSaved, false)]
        [InlineData(null, OrderStatus.CartSaved, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", OrderStatus.Rejected, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, false)]
        [InlineData(null, null, false)]
        public async Task ChangeStatusOrder_SimpleRun_ChecksIfServiceRuns(string id, OrderStatus status, bool shouldSucceed)
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var code = "1001";
            var req = new ChangeOrderStatusRequest { OrderStatus = status };

            if (shouldSucceed && !string.IsNullOrEmpty(id))
            {
                var order = new OrderEntity
                {
                    Id = Guid.Parse(id),
                    OrderCode = long.Parse(code),
                    StoreId = Guid.Parse(storeId),
                    OrderStatus = OrderStatus.Pending
                };

                _orderRepoMock.Setup(r => r.AsQueryable())
                    .Returns(new List<OrderEntity> { order }.BuildMock());

                _orderRepoMock.Setup(r => r.Update(It.IsAny<OrderEntity>()));
                _orderRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            }
            else
            {
                _orderRepoMock.Setup(r => r.AsQueryable())
                    .Returns(new List<OrderEntity>().BuildMock());
            }

            // Act
            var result = await _orderService.ChangeStatusOrder(code, req, storeId);

            // Assert
            if (shouldSucceed)
            {
                Assert.True(result);
            }
            else
            {
                Assert.False(result);
            }
        }
        #endregion

        #region CancelOrder CM-46
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", true)]
        [InlineData(null, true)]
        public async Task CancelOrder_SimpleRun_ChecksIfServiceRuns(string orderIdStr, bool shouldSucceed)
        {
            // Arrange
            Guid? orderId = string.IsNullOrEmpty(orderIdStr) ? (Guid?)null : Guid.Parse(orderIdStr);
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid().ToString();

            // Setup mocks
            if (shouldSucceed && orderId.HasValue)
            {
                _orderRepoMock.Setup(r => r.GetByIdAsync(orderId.Value))
                    .ReturnsAsync(new OrderEntity { Id = orderId.Value });
            }
            else
            {
                _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((OrderEntity)null);
            }

            // Act & Assert
            if (shouldSucceed)
            {
                var exception = await Record.ExceptionAsync(async () =>
                {
                    var res = await _orderService.CancelOrderAsync(orderId ?? Guid.Empty, userId, storeId);
                });
                Assert.Null(exception);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(async () =>
                {
                    var res = await _orderService.CancelOrderAsync(orderId ?? Guid.Empty, userId, storeId);
                });
            }
        }
        #endregion

        #region DeleteOrder CM-47
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        public async Task DeleteOrder_SimpleRun_ChecksIfServiceRuns(string orderIdStr, bool shouldSucceed)
        {
            // Arrange
            Guid? orderId = string.IsNullOrEmpty(orderIdStr) ? (Guid?)null : Guid.Parse(orderIdStr);
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid().ToString();

            // Setup mocks
            if (shouldSucceed && orderId.HasValue)
            {
                _orderRepoMock.Setup(r => r.GetByIdAsync(orderId.Value))
                    .ReturnsAsync(new OrderEntity { Id = orderId.Value });

                _orderDetailRepoMock.Setup(r => r.AsQueryable())
                    .Returns(new List<OrderDetail>().BuildMock());
            }
            else
            {
                _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((OrderEntity)null);
            }
            Random random = new Random();


            // Act & Assert
            if (shouldSucceed)
            {
                var exception = await Record.ExceptionAsync(async () =>
                {
                    var res = await _orderService.DeleteOrderAsync(random.Next().ToString(), userId, storeId);
                });
                Assert.Null(exception);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(async () =>
                {
                    var res = await _orderService.DeleteOrderAsync(random.Next().ToString(), userId, storeId);
                });
            }
        }
        #endregion

        #region GetOrderDetailByCode CM-48
        [Theory]
        [InlineData(1000, true)]
        [InlineData(9999, true)]
        //[InlineData(null, false)]
        public async Task GetOrderDetailByCode_SimpleRun_ChecksIfServiceRuns(long code, bool shouldSucceed)
        {
            // Setup mocks
            if (shouldSucceed)
            {
                _orderRepoMock.Setup(r => r.AsQueryable())
                    .Returns(new List<OrderEntity>
                    {
                new OrderEntity { OrderCode = code }
                    }.BuildMock());

                _variantRepoMock.Setup(r => r.AsQueryable())
                    .Returns(new List<MenuItemVariant>().BuildMock());
            }
            else
            {
                _orderRepoMock.Setup(r => r.AsQueryable())
                    .Returns(new List<OrderEntity>().BuildMock());
            }

            // Act & Assert
            if (shouldSucceed)
            {
                var exception = await Record.ExceptionAsync(async () =>
                {
                    var res = await _orderService.GetOrderByCodeAsync(code);
                });
                Assert.Null(exception);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(async () =>
                {
                    var res = await _orderService.GetOrderByCodeAsync(code);
                });
            }
        }
        #endregion

        #region ApplyDiscountForOrder CM-49
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", 0, true, true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", 0, true, false)]
        [InlineData(null, "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", 0, true, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "9dbfa424-24fc-43ef-af10-8c67f277115a", "String note", "Coupon code", 0, true, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, "String note", "Coupon code", 0, true, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", null, "Coupon code", 0, true, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", null, 0, true, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", 4, true, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", null, true, false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", 0, false, true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "2e959705-90be-46f6-8d77-64d6ed70e637", "String note", "Coupon code", 0, null, false)]
        [InlineData(null, null, null, null, null, null, false)]
        public async Task ApplyDiscountForOrder_SimpleRun_ChecksIfServiceRuns(
            string storeIdStr, string tableIdStr, string note, string couponCode, int point, bool isUsePoint, bool shouldSucceed)
        {
            Guid? storeId = string.IsNullOrEmpty(storeIdStr) ? (Guid?)null : Guid.Parse(storeIdStr);
            Guid? tableId = string.IsNullOrEmpty(tableIdStr) ? (Guid?)null : Guid.Parse(tableIdStr);

            var request = new ApplyDiscountOrderRequest
            {
                StoreId = storeId ?? Guid.Empty,
                TableId = tableId ?? Guid.Empty,
                Note = note,
                CouponCode = couponCode ?? "",
                Point = point,
                IsUseLoyatyPoint = isUsePoint,
                Items = new List<OrderItemDTO>()
            };

            var userId = Guid.NewGuid().ToString();

            Exception ex = await Record.ExceptionAsync(async () =>
            {
                var res = await _orderService.ApplyDiscountForOrder(request, userId, storeId.ToString());
            });

            Assert.NotNull(ex);
        }
        #endregion
    }
}
