using FOCS.Common.Constants;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Common.Models.CartModels;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace FOCS.UnitTest.OrderServiceTest
{
    public class CreateOrderTests : OrderServiceTestBase
    {
        [Fact]
        public async Task CreateOrderAsync_ShouldThrow_WhenStoreNotFound()
        {
            // Arrange
            var request = CreateValidOrderRequest();
            _mockStoreRepository.Setup(r => r.GetByIdAsync(_validStoreId))
                                .ReturnsAsync((Store)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _orderService.CreateOrderAsync(request, _validUserId));
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrow_WhenTableNotFound()
        {
            // Arrange
            var request = CreateValidOrderRequest();
            _mockStoreRepository.Setup(r => r.GetByIdAsync(_validStoreId))
                                .ReturnsAsync(new Store { Id = _validStoreId });
            _mockTableRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Table, bool>>>()))
                                .ReturnsAsync(new List<Table>());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _orderService.CreateOrderAsync(request, _validUserId));
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrow_WhenMenuItemNotFound()
        {
            // Arrange
            var request = CreateValidOrderRequest();
            SetupDefaultStoreAndTable();
            SetupDefaultPricing(100);
            // no menu item
            _mockMenuItemRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MenuItem, bool>>>()))
                                   .ReturnsAsync(new List<MenuItem>());
            _mockVariantRepository.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MenuItemVariant, bool>>>()))
                                   .ReturnsAsync(new List<MenuItemVariant>());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _orderService.CreateOrderAsync(request, _validUserId));
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrow_WhenStoreSettingNotFound()
        {
            // Arrange
            var request = CreateValidOrderRequest();
            SetupDefaultStoreAndTable();
            SetupDefaultPricing(100);
            SetupDefaultMenuAndVariant();
            _mockStoreSettingService.Setup(s => s.GetStoreSettingAsync(_validStoreId, _validUserId))
                                    .ReturnsAsync((StoreSettingDTO)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _orderService.CreateOrderAsync(request, _validUserId));
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldReturn_DiscountResultAndPersistOrder_WhenRequestValid()
        {
            // Arrange
            var request = CreateValidOrderRequest();
            SetupDefaultStoreAndTable();
            SetupDefaultPricing(150.75);
            SetupDefaultMenuAndVariant();
            SetupDefaultStoreSetting();

            var menuItems = request.Items
        .Select(i => new MenuItem { Id = i.MenuItemId, Name = "TestItem" })
        .ToList();
            _mockMenuItemRepository
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<MenuItem, bool>>>()))
                .ReturnsAsync(menuItems);

            _mockVariantRepository
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<MenuItemVariant, bool>>>()))
                .ReturnsAsync(new List<MenuItemVariant>()); // vì không có variant

            // Act
            var result = await _orderService.CreateOrderAsync(request, _validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((decimal)150.75, result.TotalPrice);
            _mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<FOCS.Order.Infrastucture.Entities.Order>()), Times.Once);
            _mockOrderDetailRepository.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<OrderDetail>>()), Times.Once);
            _mockOrderRepository.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
        }
    }
}