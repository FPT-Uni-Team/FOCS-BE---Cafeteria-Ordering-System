using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.UnitTest.FeedbackServiceTest
{
    public class GetFeedbackByMenuItemTests : FeedbackServiceTestBase
    {
        private readonly Guid _menuItemId = Guid.NewGuid();
        private readonly Guid _storeId = Guid.NewGuid();
        private readonly string _storeIdString;
        private readonly Feedback _feedback1;
        private readonly Feedback _feedback2;
        private readonly List<FeedbackDTO> _expectedDtos;

        public GetFeedbackByMenuItemTests()
        {
            _storeIdString = _storeId.ToString();

            // =========== TẠO DỮ LIỆU MẪU ===========
            var order1 = CreateOrderWithMenuItem(_storeId, _menuItemId);
            var order2 = CreateOrderWithMenuItem(_storeId, _menuItemId);

            _feedback1 = CreateTestFeedback(orderId: order1.Id, storeId: _storeId, rating: 5, comment: "Great", isPublic: true);
            _feedback1.Order = order1;

            _feedback2 = CreateTestFeedback(orderId: order2.Id, storeId: _storeId, rating: 4, comment: "Good", isPublic: true);
            _feedback2.Order = order2;

            _expectedDtos = new List<FeedbackDTO>
            {
                CreateTestFeedbackDTO(id: _feedback1.Id, orderId: _feedback1.OrderId, rating: _feedback1.Rating, comment:_feedback1.Comment),
                CreateTestFeedbackDTO(id: _feedback2.Id, orderId: _feedback2.OrderId, rating: _feedback2.Rating, comment:_feedback2.Comment)
            };

            // Map List<FeedbackDTO>
            _mapperMock.Setup(m => m.Map<List<FeedbackDTO>>(It.Is<List<Feedback>>(l => l.Count == 2)))
                       .Returns(_expectedDtos);
        }

        private static OrderEntity CreateOrderWithMenuItem(Guid storeId, Guid menuItemId)
        {
            return new OrderEntity
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        MenuItemId = menuItemId,
                        Quantity = 1,
                        UnitPrice = 10_000
                    }
                }
            };
        }

        [Fact]
        public async Task GetFeedbackByMenuItemAsync_ShouldReturnFeedbacks_WhenExist()
        {
            // Arrange
            var mockQueryable = new[] { _feedback1, _feedback2 }.AsQueryable().BuildMockDbSet();
            _feedbackRepoMock.Setup(r => r.AsQueryable()).Returns(mockQueryable.Object);

            // Act
            var result = await _feedbackService.GetFeedbackByMenuItemAsync(_menuItemId, _storeIdString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Collection(result,
                r1 => Assert.Equal(_feedback1.Id, r1.Id),
                r2 => Assert.Equal(_feedback2.Id, r2.Id));
        }

        [Fact]
        public async Task GetFeedbackByMenuItemAsync_ShouldReturnEmptyList_WhenNoFeedbacks()
        {
            // Arrange
            var mockQueryable = Array.Empty<Feedback>().AsQueryable().BuildMockDbSet();
            _feedbackRepoMock.Setup(r => r.AsQueryable()).Returns(mockQueryable.Object);

            // Act
            var result = await _feedbackService.GetFeedbackByMenuItemAsync(_menuItemId, _storeIdString);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetFeedbackByMenuItemAsync_ShouldReturnEmptyList_WhenStoreIdDoesNotMatch()
        {
            // Arrange
            var wrongStoreId = Guid.NewGuid().ToString();
            var mockQueryable = new[] { _feedback1, _feedback2 }.AsQueryable().BuildMockDbSet();
            _feedbackRepoMock.Setup(r => r.AsQueryable()).Returns(mockQueryable.Object);

            // Act
            var result = await _feedbackService.GetFeedbackByMenuItemAsync(_menuItemId, wrongStoreId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetFeedbackByMenuItemAsync_ShouldMapAllPropertiesCorrectly()
        {
            // Arrange
            var mockQueryable = new[] { _feedback1, _feedback2 }.AsQueryable().BuildMockDbSet();
            _feedbackRepoMock.Setup(r => r.AsQueryable()).Returns(mockQueryable.Object);

            // Act
            var result = await _feedbackService.GetFeedbackByMenuItemAsync(_menuItemId, _storeIdString);

            // Assert
            _mapperMock.Verify(m => m.Map<List<FeedbackDTO>>(It.Is<List<Feedback>>(l => l.Count == 2)), Times.Once);
            Assert.Equal(_expectedDtos, result); // reference equality vì _mapper trả về chính object này
        }
    }
}
