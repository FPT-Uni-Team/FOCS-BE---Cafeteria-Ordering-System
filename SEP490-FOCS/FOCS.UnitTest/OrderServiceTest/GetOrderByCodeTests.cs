using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.UnitTest.OrderServiceTest
{
    public class GetOrderByCodeTests : OrderServiceTestBase
    {
        [Fact]
        public async Task GetOrderByCodeAsync_ShouldReturnOrderDTO_WhenOrderExists()
        {
            // Arrange
            long code = 123456L;
            var fakeOrder = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderCode = code,
                StoreId = _validStoreId,
                TableId = _validTableId,
                OrderDetails = new List<OrderDetail>()
            };

            var expectedDto = new OrderDTO
            {
                Id = fakeOrder.Id,
                OrderCode = code.ToString(),
                StoreId = fakeOrder.StoreId,
            };

            var ordersMock = new List<OrderEntity> { fakeOrder }
                .AsQueryable()
                .BuildMockDbSet();

            _mockOrderRepository
                .Setup(r => r.AsQueryable())
                .Returns(ordersMock.Object);

            var emptyVariants = new List<MenuItemVariant>().AsQueryable().BuildMockDbSet();
            _mockVariantRepository
                .Setup(r => r.AsQueryable())
                .Returns(emptyVariants.Object);

            var emptyMenuItems = new List<MenuItem>().AsQueryable().BuildMockDbSet();
            _mockMenuItemRepository
                .Setup(r => r.AsQueryable())
                .Returns(emptyMenuItems.Object);

            // Mock mapper
            _mockMapper
                .Setup(m => m.Map<OrderDTO>(fakeOrder))
                .Returns(expectedDto);

            // Act
            var result = await _orderService.GetOrderByCodeAsync(code);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.OrderCode, result.OrderCode);
            Assert.Equal(expectedDto.StoreId, result.StoreId);
            Assert.Empty(result.OrderDetails);
        }

        [Fact]
        public async Task GetOrderByCodeAsync_ShouldReturnEmptyOrderDTO_WhenOrderNotFound()
        {
            // Arrange
            long code = 999999;
            var emptyMock = new List<OrderEntity>()
                .AsQueryable()
                .BuildMockDbSet();

            _mockOrderRepository
                .Setup(r => r.AsQueryable())
                .Returns(emptyMock.Object);

            // Act
            var result = await _orderService.GetOrderByCodeAsync(code);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(default(Guid), result.Id);
            Assert.Null(result.OrderCode);
            Assert.Equal(default(Guid), result.StoreId);
            Assert.Empty(result.OrderDetails);
        }

        [Fact]
        public async Task GetOrderByCodeAsync_ShouldPopulateVariant_WhenVariantExists()
        {
            // Arrange
            long code = 2222L;
            var variantId = Guid.NewGuid();
            var fakeVariant = new MenuItemVariant { Id = variantId, Price = 5.5 };

            var detail = new OrderDetail
            {
                Id = Guid.NewGuid(),
                MenuItemId = Guid.NewGuid(),
                VariantId = variantId,
                Quantity = 3
            };

            var fakeOrder = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderCode = code,
                StoreId = _validStoreId,
                TableId = _validTableId,
                OrderDetails = new List<OrderDetail> { detail }
            };

            // mock orders
            var ordersMock = new List<OrderEntity> { fakeOrder }
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(ordersMock.Object);

            // mock variants
            var variantsMock = new List<MenuItemVariant> { fakeVariant }
                .AsQueryable()
                .BuildMockDbSet();
            _mockVariantRepository.Setup(r => r.AsQueryable()).Returns(variantsMock.Object);

            // menu items & other repos (empty)
            _mockMenuItemRepository.Setup(r => r.AsQueryable())
                .Returns(new List<MenuItem>().AsQueryable().BuildMockDbSet().Object);

            // capture the mapped entity to inspect Variant
            OrderEntity captured = null!;
            _mockMapper.Setup(m => m.Map<OrderDTO>(It.IsAny<OrderEntity>()))
                       .Callback((object src) => captured = src as OrderEntity)
                       .Returns(new OrderDTO());

            // Act
            await _orderService.GetOrderByCodeAsync(code);

            // Assert: service should have set detail.Variant = fakeVariant
            Assert.NotNull(captured);
            Assert.Single(captured.OrderDetails);
            var firstDetail = captured.OrderDetails.First();
            Assert.NotNull(firstDetail.Variant);
            Assert.Equal(variantId, firstDetail.Variant.Id);
        }

        [Fact]
        public async Task GetOrderByCodeAsync_ShouldHandleMultipleDetailsCorrectly()
        {
            // Arrange
            long code = 3333L;
            var v1 = Guid.NewGuid();
            var v2 = Guid.NewGuid();
            var detail1 = new OrderDetail { Id = Guid.NewGuid(), MenuItemId = Guid.NewGuid(), VariantId = v1, Quantity = 1 };
            var detail2 = new OrderDetail { Id = Guid.NewGuid(), MenuItemId = Guid.NewGuid(), VariantId = v2, Quantity = 2 };

            var fakeOrder = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderCode = code,
                StoreId = _validStoreId,
                TableId = _validTableId,
                OrderDetails = new List<OrderDetail> { detail1, detail2 }
            };

            var ordersMock = new List<OrderEntity> { fakeOrder }
                .AsQueryable()
                .BuildMockDbSet();
            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(ordersMock.Object);

            var variants = new[]
            {
                new MenuItemVariant { Id = v1, Price = 1 },
                new MenuItemVariant { Id = v2, Price = 2 }
            };
            var variantsMock = variants.AsQueryable().BuildMockDbSet();
            _mockVariantRepository.Setup(r => r.AsQueryable()).Returns(variantsMock.Object);

            _mockMenuItemRepository.Setup(r => r.AsQueryable())
                .Returns(new List<MenuItem>().AsQueryable().BuildMockDbSet().Object);

            // capture and verify
            OrderEntity captured = null!;
            _mockMapper.Setup(m => m.Map<OrderDTO>(It.IsAny<OrderEntity>()))
                       .Callback((object src) => captured = src as OrderEntity)
                       .Returns(new OrderDTO());

            // Act
            await _orderService.GetOrderByCodeAsync(code);

            // Assert all details have their Variant populated
            Assert.NotNull(captured);
            Assert.Equal(2, captured.OrderDetails.Count);
            Assert.Equal(v1, captured.OrderDetails.Single(d => d.Id == detail1.Id).Variant!.Id);
            Assert.Equal(v2, captured.OrderDetails.Single(d => d.Id == detail2.Id).Variant!.Id);
        }

        [Fact]
        public async Task GetOrderByCodeAsync_ShouldIncludeMenuItemReference()
        {
            // Arrange
            long code = 4444L;
            var menuItemId = Guid.NewGuid();
            var fakeMenuItem = new MenuItem { Id = menuItemId, Name = "TestItem" };

            var variantId = Guid.NewGuid();
            var fakeVariant = new MenuItemVariant { Id = variantId, Price = 9.9 };

            var detail = new OrderDetail
            {
                Id = Guid.NewGuid(),
                MenuItemId = menuItemId,
                VariantId = variantId,
                Quantity = 5,
                MenuItem = fakeMenuItem
            };

            var fakeOrder = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderCode = code,
                StoreId = _validStoreId,
                TableId = _validTableId,
                OrderDetails = new List<OrderDetail> { detail }
            };

            // Mock orders
            _mockOrderRepository
                .Setup(r => r.AsQueryable())
                .Returns(new List<OrderEntity> { fakeOrder }
                    .AsQueryable()
                    .BuildMockDbSet()
                    .Object);

            // Mock variants
            _mockVariantRepository
                .Setup(r => r.AsQueryable())
                .Returns(new List<MenuItemVariant> { fakeVariant }
                    .AsQueryable()
                    .BuildMockDbSet()
                    .Object);

            _mockMenuItemRepository
                .Setup(r => r.AsQueryable())
                .Returns(new List<MenuItem>().AsQueryable().BuildMockDbSet().Object);

            OrderEntity captured = null!;
            _mockMapper
                .Setup(m => m.Map<OrderDTO>(It.IsAny<OrderEntity>()))
                .Callback((object src) => captured = src as OrderEntity)
                .Returns(new OrderDTO());

            // Act
            await _orderService.GetOrderByCodeAsync(code);

            // Assert
            Assert.NotNull(captured);
            var onlyDetail = captured.OrderDetails.First();

            // MenuItem đã được gán ngay từ Arrange
            Assert.NotNull(onlyDetail.MenuItem);
            Assert.Equal(menuItemId, onlyDetail.MenuItem.Id);
            // Variant được service gán sau
            Assert.NotNull(onlyDetail.Variant);
            Assert.Equal(variantId, onlyDetail.Variant.Id);
        }

    }
}
