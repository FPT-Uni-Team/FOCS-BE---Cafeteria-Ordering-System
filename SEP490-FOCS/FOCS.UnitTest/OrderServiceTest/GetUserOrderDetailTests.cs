using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.UnitTest.OrderServiceTest
{
    public class GetUserOrderDetailTests : OrderServiceTestBase
    {
        [Fact]
        public async Task GetUserOrderDetailAsync_ShouldReturnOrderDTO_WhenOrderExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            var fakeOrder = new OrderEntity
            {
                Id = orderId,
                UserId = userId,
                StoreId = _validStoreId,
                TableId = _validTableId,
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail { Id = Guid.NewGuid(), MenuItemId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            // Mock repository.AsQueryable() with async + Include support
            var ordersMock = new List<OrderEntity> { fakeOrder }
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository
                .Setup(r => r.AsQueryable())
                .Returns(ordersMock.Object);

            // Mock mapper
            var expectedDto = new OrderDTO
            {
                Id = fakeOrder.Id,
                OrderCode = fakeOrder.OrderCode.ToString(),
                StoreId = fakeOrder.StoreId,
                OrderDetails = new List<OrderDetailDTO>
                {
                    new OrderDetailDTO { Quantity = 1 }
                }
            };
            _mockMapper
                .Setup(m => m.Map<OrderDTO>(It.Is<OrderEntity>(o => o == fakeOrder)))
                .Returns(expectedDto);

            // Act
            var result = await _orderService.GetUserOrderDetailAsync(userId, orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.OrderCode, result.OrderCode);
            Assert.Equal(expectedDto.StoreId, result.StoreId);
            Assert.Single(result.OrderDetails);
            Assert.Equal(1, result.OrderDetails[0].Quantity);
        }

        [Fact]
        public async Task GetUserOrderDetailAsync_ShouldThrowNotFound_WhenOrderDoesNotExist()
        {
            // Arrange: repository returns empty
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var emptyMock = new List<OrderEntity>()
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository
                .Setup(r => r.AsQueryable())
                .Returns(emptyMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _orderService.GetUserOrderDetailAsync(userId, orderId));
        }

        [Fact]
        public async Task GetUserOrderDetailAsync_ShouldReturnEmptyDetails_WhenNoOrderDetails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            var fakeOrder = new OrderEntity
            {
                Id = orderId,
                UserId = userId,
                StoreId = _validStoreId,
                TableId = _validTableId,
                OrderDetails = new List<OrderDetail>() // empty
            };

            var ordersMock = new List<OrderEntity> { fakeOrder }
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(ordersMock.Object);

            var expectedDto = new OrderDTO
            {
                Id = fakeOrder.Id,
                OrderCode = fakeOrder.OrderCode.ToString(),
                StoreId = fakeOrder.StoreId,
                OrderDetails = new List<OrderDetailDTO>() // expect empty
            };
            _mockMapper
                .Setup(m => m.Map<OrderDTO>(It.IsAny<OrderEntity>()))
                .Returns(expectedDto);

            // Act
            var result = await _orderService.GetUserOrderDetailAsync(userId, orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.OrderDetails);
        }

        [Fact]
        public async Task GetUserOrderDetailAsync_ShouldInvokeMapperWithCorrectEntity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            var detail1 = new OrderDetail { Id = Guid.NewGuid(), Quantity = 1 };
            var detail2 = new OrderDetail { Id = Guid.NewGuid(), Quantity = 2 };
            var fakeOrder = new OrderEntity
            {
                Id = orderId,
                UserId = userId,
                StoreId = _validStoreId,
                TableId = _validTableId,
                OrderDetails = new List<OrderDetail> { detail1, detail2 }
            };

            var ordersMock = new List<OrderEntity> { fakeOrder }
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(ordersMock.Object);

            OrderEntity? captured = null;
            _mockMapper
                .Setup(m => m.Map<OrderDTO>(It.IsAny<OrderEntity>()))
                .Callback((object src) => captured = src as OrderEntity)
                .Returns(new OrderDTO());

            // Act
            await _orderService.GetUserOrderDetailAsync(userId, orderId);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(orderId, captured!.Id);
            Assert.Equal(userId, captured.UserId);
            Assert.Equal(2, captured.OrderDetails.Count);
            Assert.Contains(captured.OrderDetails, d => d.Id == detail1.Id);
            Assert.Contains(captured.OrderDetails, d => d.Id == detail2.Id);
        }
    }
}
