using FOCS.Common.Enums;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.UnitTest.OrderServiceTest
{
    public class GetPendingOrdersInDayTests : OrderServiceTestBase
    {
        [Fact]
        public async Task GetPendingOrdersInDayAsync_ShouldReturnEmptyList_WhenNoPendingOrders()
        {
            // Arrange: AsQueryable trả về empty
            var emptyOrders = new List<OrderEntity>()
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository
                .Setup(r => r.AsQueryable())
                .Returns(emptyOrders.Object);

            // Cần mock mapper để không trả null
            _mockMapper
                .Setup(m => m.Map<List<OrderDTO>>(It.IsAny<List<OrderEntity>>()))
                .Returns(new List<OrderDTO>());

            // Act
            var result = await _orderService.GetPendingOrdersInDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Verify không gọi UpdateRange/SaveChanges/GetByIdAsync
            _mockMenuItemRepository.Verify(m => m.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetPendingOrdersInDayAsync_ShouldProcessAndReturnDtos_WhenThereArePendingOrders()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var menuItemId = Guid.NewGuid();
            var fakeMenuItem = new MenuItem { Id = menuItemId, Name = "Pizza" };

            // Tạo một Order thỏa điều kiện
            var detail = new OrderDetail
            {
                Id = Guid.NewGuid(),
                MenuItemId = menuItemId,
                Quantity = 2
            };
            var fakeOrder = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderCode = 1111,
                StoreId = _validStoreId,
                OrderStatus = Common.Enums.OrderStatus.Pending,
                PaymentStatus = Common.Enums.PaymentStatus.Paid,
                IsDeleted = false,
                CreatedAt = now,
                OrderDetails = new List<OrderDetail> { detail }
            };

            // IQueryable<Order> mock async + Include
            var ordersMock = new List<OrderEntity> { fakeOrder }
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository
                .Setup(r => r.AsQueryable())
                .Returns(ordersMock.Object);

            // Mapper: map entity → DTO (without MenuItemName)
            var expectedDto = new OrderDTO
            {
                Id = fakeOrder.Id,
                OrderCode = fakeOrder.OrderCode.ToString(),
                StoreId = fakeOrder.StoreId,
                OrderDetails = new List<OrderDetailDTO>
                {
                    new OrderDetailDTO { MenuItemId = menuItemId, Quantity = 2 }
                }
            };
            _mockMapper
                .Setup(m => m.Map<List<OrderDTO>>(It.IsAny<List<OrderEntity>>()))
                .Returns(new List<OrderDTO> { expectedDto });

            // Khi UpdateRange => no-op
            _mockOrderRepository
                .Setup(r => r.UpdateRange(It.IsAny<IEnumerable<OrderEntity>>()));

            _mockOrderRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Mock menuItem repo để trả tên
            _mockMenuItemRepository
                .Setup(m => m.GetByIdAsync(menuItemId))
                .ReturnsAsync(fakeMenuItem);

            // Act
            var result = await _orderService.GetPendingOrdersInDayAsync();

            // Assert
            Assert.Single(result);
            var dto = result[0];
            Assert.Equal(expectedDto.Id, dto.Id);
            Assert.Equal(expectedDto.OrderCode, dto.OrderCode);

            // Sau mapping, service gán MenuItemName lên tất cả details
            Assert.All(dto.OrderDetails, d => Assert.Equal("Pizza", d.MenuItemName));

            // Verify update & save
            _mockOrderRepository.Verify(r => r.UpdateRange(It.Is<IEnumerable<OrderEntity>>(l => l.Contains(fakeOrder))), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveChangesAsync(), Times.Once);

            // Verify menuItem lookup
            _mockMenuItemRepository.Verify(m => m.GetByIdAsync(menuItemId), Times.Once);
        }

        [Fact]
        public async Task ShouldExcludeOrdersOlderThan24Hours()
        {
            // Arrange: một order cũ (2 ngày trước) và một order đúng (giờ này)
            var oldOrder = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderCode = 1,
                StoreId = _validStoreId,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Paid,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                OrderDetails = new List<OrderDetail>()
            };
            var freshOrder = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderCode = 2,
                StoreId = _validStoreId,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Paid,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>()
            };
            var mockOrders = new[] { oldOrder, freshOrder }
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(mockOrders.Object);

            List<OrderEntity>? passedToMapper = null;
            _mockMapper
                .Setup(m => m.Map<List<OrderDTO>>(It.IsAny<List<OrderEntity>>()))
                .Callback((object src) => passedToMapper = src as List<OrderEntity>)
                .Returns(new List<OrderDTO>());

            // Act
            await _orderService.GetPendingOrdersInDayAsync();

            // Assert: chỉ freshOrder được lọc
            Assert.NotNull(passedToMapper);
            Assert.Single(passedToMapper);
            Assert.Equal(2, passedToMapper![0].OrderCode);
        }

        [Fact]
        public async Task ShouldExcludeNonPaidOrNonPendingOrDeletedOrders()
        {
            // Arrange: 4 orders, chỉ 1 hợp lệ
            var baseTime = DateTime.UtcNow;
            List<OrderEntity> list = new()
            {
                // wrong payment
                new OrderEntity { Id=Guid.NewGuid(), OrderCode=10, OrderStatus=OrderStatus.Pending,  PaymentStatus=PaymentStatus.Unpaid,      IsDeleted=false, CreatedAt=baseTime, OrderDetails=new List<OrderDetail>() },
                // wrong status
                new OrderEntity { Id=Guid.NewGuid(), OrderCode=11, OrderStatus=OrderStatus.Confirmed,PaymentStatus=PaymentStatus.Paid,        IsDeleted=false, CreatedAt=baseTime, OrderDetails=new List<OrderDetail>() },
                // deleted
                new OrderEntity { Id=Guid.NewGuid(), OrderCode=12, OrderStatus=OrderStatus.Pending,  PaymentStatus=PaymentStatus.Paid,        IsDeleted=true,  CreatedAt=baseTime, OrderDetails=new List<OrderDetail>() },
                // correct
                new OrderEntity { Id=Guid.NewGuid(), OrderCode=13, OrderStatus=OrderStatus.Pending,  PaymentStatus=PaymentStatus.Paid,        IsDeleted=false, CreatedAt=baseTime, OrderDetails=new List<OrderDetail>() }
            };
            var mockOrders = list.AsQueryable().BuildMockDbSet();
            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(mockOrders.Object);

            List<OrderEntity>? passed = null;
            _mockMapper
              .Setup(m => m.Map<List<OrderDTO>>(It.IsAny<List<OrderEntity>>()))
              .Callback((object src) => passed = src as List<OrderEntity>)
              .Returns(new List<OrderDTO>());

            // Act
            await _orderService.GetPendingOrdersInDayAsync();

            // Assert
            Assert.NotNull(passed);
            Assert.Single(passed);
            Assert.Equal(13, passed![0].OrderCode);
        }

        [Fact]
        public async Task ShouldSkipMappingWhenNoOrders()
        {
            // Arrange: no orders
            var emptyMock = new List<OrderEntity>().AsQueryable().BuildMockDbSet();
            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(emptyMock.Object);

            bool mapperCalled = false;
            _mockMapper
              .Setup(m => m.Map<List<OrderDTO>>(It.IsAny<List<OrderEntity>>()))
              .Callback((object src) => mapperCalled = true)
              .Returns(new List<OrderDTO>());

            // Act
            var result = await _orderService.GetPendingOrdersInDayAsync();

            // Assert
            Assert.Empty(result);
            Assert.True(mapperCalled);
        }

    }
}
