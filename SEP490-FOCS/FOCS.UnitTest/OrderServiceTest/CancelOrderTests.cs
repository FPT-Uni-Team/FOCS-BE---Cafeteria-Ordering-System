using FOCS.Common.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.UnitTest.OrderServiceTest
{
    public class CancelOrderTests : OrderServiceTestBase
    {
        [Fact]
        public async Task CancelOrderAsync_ShouldUpdateStatusToCanceled_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid().ToString();
            var order = new OrderEntity { Id = orderId, OrderStatus = OrderStatus.Pending };

            _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync(order);
            _mockOrderRepository.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _orderService.CancelOrderAsync(orderId, userId, storeId);

            // Assert
            Assert.True(result);
            Assert.Equal(OrderStatus.Canceled, order.OrderStatus);
            Assert.True(order.UpdatedAt <= DateTime.UtcNow);

            _mockOrderRepository.Verify(r => r.Update(order), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldReturnFalse_WhenOrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid().ToString();

            _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync((OrderEntity)null);

            // Act
            var result = await _orderService.CancelOrderAsync(orderId, userId, storeId);

            // Assert
            Assert.False(result);

            _mockLogger.Verify(
                        x => x.Log(
                            LogLevel.Error,
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Not found")),
                            It.IsAny<Exception>(),
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
            _mockOrderRepository.Verify(r => r.Update(It.IsAny<OrderEntity>()), Times.Never);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldReturnFalse_WhenExceptionThrown()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid().ToString();

            _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _orderService.CancelOrderAsync(orderId, userId, storeId);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database error")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldUpdateOrderStatusAndReturnTrue_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid().ToString();
            var order = new OrderEntity { Id = orderId, OrderStatus = OrderStatus.Pending };

            _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            // Act
            var result = await _orderService.CancelOrderAsync(orderId, userId, storeId);

            // Assert
            Assert.True(result);
            Assert.Equal(OrderStatus.Canceled, order.OrderStatus);
            _mockOrderRepository.Verify(r => r.Update(order), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

    }

}
