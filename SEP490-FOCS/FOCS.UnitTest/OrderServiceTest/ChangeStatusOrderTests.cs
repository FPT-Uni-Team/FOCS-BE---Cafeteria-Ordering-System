using FOCS.Common.Interfaces;
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
    public class ChangeStatusOrderTests : OrderServiceTestBase
    {
        [Fact]
        public async Task ChangeStatusOrder_ShouldReturnFalse_WhenOrderNotFound()
        {
            // Arrange: AsQueryable trả về empty
            var emptyMock = new List<OrderEntity>()
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository
                .Setup(r => r.AsQueryable())
                .Returns(emptyMock.Object);

            // Act
            var result = await _orderService.ChangeStatusOrder("1234", new ChangeOrderStatusRequest
            {
                OrderStatus = Common.Enums.OrderStatus.Confirmed
            }, _validStoreId.ToString());

            // Assert
            Assert.False(result);
            // Không gọi Update hay SaveChanges
            _mockOrderRepository.Verify(r => r.Update(It.IsAny<OrderEntity>()), Times.Never);
            _mockOrderRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ChangeStatusOrder_ShouldUpdateStatusOnly_WhenNotConfirmed()
        {
            // Arrange
            long code = 2222L;
            var existing = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderCode = code,
                StoreId = _validStoreId,
                OrderStatus = Common.Enums.OrderStatus.Pending,
                PaymentStatus = Common.Enums.PaymentStatus.Unpaid,
                IsDeleted = false
            };
            var orderMock = new List<OrderEntity> { existing }
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(orderMock.Object);

            // Capture the updated entity
            OrderEntity? updated = null;
            _mockOrderRepository
                .Setup(r => r.Update(It.IsAny<OrderEntity>()))
                .Callback<OrderEntity>(o => updated = o);

            _mockOrderRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            var req = new ChangeOrderStatusRequest { OrderStatus = Common.Enums.OrderStatus.Canceled };

            // Act
            var result = await _orderService.ChangeStatusOrder(code.ToString(), req, _validStoreId.ToString());

            // Assert
            Assert.True(result);
            Assert.NotNull(updated);
            Assert.Equal(Common.Enums.OrderStatus.Canceled, updated.OrderStatus);
            // PaymentStatus giữ nguyên
            Assert.Equal(Common.Enums.PaymentStatus.Unpaid, updated.PaymentStatus);
            _mockOrderRepository.Verify(r => r.Update(It.IsAny<OrderEntity>()), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ChangeStatusOrder_ShouldSetPaymentPaid_WhenConfirmed()
        {
            // Arrange
            long code = 3333L;
            var existing = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderCode = code,
                StoreId = _validStoreId,
                OrderStatus = Common.Enums.OrderStatus.Pending,
                PaymentStatus = Common.Enums.PaymentStatus.Unpaid,
                IsDeleted = false
            };
            var orderMock = new List<OrderEntity> { existing }
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(orderMock.Object);

            OrderEntity? updated = null;
            _mockOrderRepository
                .Setup(r => r.Update(It.IsAny<OrderEntity>()))
                .Callback<OrderEntity>(o => updated = o);

            _mockOrderRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            var req = new ChangeOrderStatusRequest { OrderStatus = Common.Enums.OrderStatus.Confirmed };

            // Act
            var result = await _orderService.ChangeStatusOrder(code.ToString(), req, _validStoreId.ToString());

            // Assert
            Assert.True(result);
            Assert.NotNull(updated);
            Assert.Equal(Common.Enums.OrderStatus.Confirmed, updated.OrderStatus);
            Assert.Equal(Common.Enums.PaymentStatus.Paid, updated.PaymentStatus);
            _mockOrderRepository.Verify(r => r.Update(It.IsAny<OrderEntity>()), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ChangeStatusOrder_ShouldReturnFalse_WhenSaveChangesThrows()
        {
            // Arrange: có 1 order pending
            long code = 5555L;
            var existing = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderCode = code,
                StoreId = _validStoreId,
                OrderStatus = Common.Enums.OrderStatus.Pending,
                PaymentStatus = Common.Enums.PaymentStatus.Unpaid,
                IsDeleted = false
            };

            var orderMock = new List<OrderEntity> { existing }
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository
                .Setup(r => r.AsQueryable())
                .Returns(orderMock.Object);

            // Khi SaveChangesAsync được gọi, ném lỗi
            _mockOrderRepository
                .Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(new Exception("DB failure"));

            var req = new ChangeOrderStatusRequest
            {
                OrderStatus = Common.Enums.OrderStatus.Canceled
            };

            // Act
            var result = await _orderService.ChangeStatusOrder(code.ToString(), req, _validStoreId.ToString());

            // Assert
            Assert.False(result);
            // Đã gọi Update nhưng SaveChangesAsync thất bại
            _mockOrderRepository.Verify(r => r.Update(It.IsAny<OrderEntity>()), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ChangeStatusOrder_ShouldReturnFalse_WhenStoreIdInvalid()
        {
            // Arrange: có 1 order pending, nhưng storeId truyền vào không phải GUID
            long code = 6666L;
            var existing = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderCode = code,
                StoreId = _validStoreId,
                OrderStatus = Common.Enums.OrderStatus.Pending,
                PaymentStatus = Common.Enums.PaymentStatus.Unpaid,
                IsDeleted = false
            };

            var orderMock = new List<OrderEntity> { existing }
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository
                .Setup(r => r.AsQueryable())
                .Returns(orderMock.Object);

            var req = new ChangeOrderStatusRequest
            {
                OrderStatus = Common.Enums.OrderStatus.Confirmed
            };

            // Act: storeId không phải GUID → FormatException trong Guid.Parse
            var result = await _orderService.ChangeStatusOrder(code.ToString(), req, "not-a-guid");

            // Assert
            Assert.False(result);
            // Không gọi Update vì exception xảy ra trước
            _mockOrderRepository.Verify(r => r.Update(It.IsAny<OrderEntity>()), Times.Never);
            _mockOrderRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
