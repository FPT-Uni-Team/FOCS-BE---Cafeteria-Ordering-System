using FOCS.Order.Infrastucture.Entities;
using MockQueryable;
using Moq;
using System;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.UnitTest.OrderServiceTest
{
    public class DeleteOrderTests : OrderServiceTestBase
    {
        [Fact]
        public async Task DeleteOrderAsync_ShouldDeleteOrderAndDetails_WhenExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid().ToString();

            var order = new OrderEntity { Id = orderId };
            var orderDetails = new List<OrderDetail>
        {
            new() { Id = Guid.NewGuid(), OrderId = orderId },
            new() { Id = Guid.NewGuid(), OrderId = orderId }
        };

            _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            _mockOrderDetailRepository.Setup(r => r.AsQueryable())
                .Returns(orderDetails.AsQueryable().BuildMock());

            _mockOrderRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            Random random = new Random();
            var result = await _orderService.DeleteOrderAsync(random.NextDouble().ToString(), userId, storeId);

            // Assert
            Assert.True(result);
            _mockOrderDetailRepository.Verify(r => r.RemoveRange(It.IsAny<List<OrderDetail>>()), Times.Once);
            _mockOrderRepository.Verify(r => r.Remove(order), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteOrderAsync_ShouldReturnFalse_WhenExceptionThrown()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid().ToString();

            _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId))
                .ThrowsAsync(new Exception("Database error"));
            Random random = new Random();

            // Act
            var result = await _orderService.DeleteOrderAsync(random.NextDouble().ToString(), userId, storeId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteOrderAsync_ShouldRemoveOrderOnly_WhenNoDetails()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid().ToString();

            var order = new OrderEntity { Id = orderId };

            _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            _mockOrderDetailRepository.Setup(r => r.AsQueryable())
                .Returns(new List<OrderDetail>().AsQueryable().BuildMock());

            _mockOrderRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            Random random = new Random();

            var result = await _orderService.DeleteOrderAsync(random.NextDouble().ToString(), userId, storeId);

            // Assert
            Assert.True(result);
            _mockOrderDetailRepository.Verify(r => r.RemoveRange(It.IsAny<List<OrderDetail>>()), Times.Never);
            _mockOrderRepository.Verify(r => r.Remove(order), Times.Once);
        }

        [Fact]
        public async Task DeleteOrderAsync_ShouldThrowNotFoundException_WhenOrderIsNull()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid().ToString();

            _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync((OrderEntity)null);

            // Act
            Random random = new Random();

            var result = await _orderService.DeleteOrderAsync(random.NextDouble().ToString(), userId, storeId);

            // Assert
            Assert.False(result);
        }
    }

}
