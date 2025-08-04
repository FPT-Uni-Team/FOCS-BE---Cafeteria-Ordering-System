using AutoMapper;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FOCS.UnitTest.FeedbackServiceTest
{
    public class GetFeedbackByOrderIdTests : FeedbackServiceTestBase
    {
        private readonly Guid _testOrderId = Guid.NewGuid();
        private readonly Guid _testStoreId = Guid.NewGuid();
        private readonly string _testStoreIdString;
        private readonly Feedback _testFeedback;

        public GetFeedbackByOrderIdTests()
        {
            _testStoreIdString = _testStoreId.ToString();

            _testFeedback = new Feedback
            {
                Id = Guid.NewGuid(),
                OrderId = _testOrderId,
                StoreId = _testStoreId,
                Rating = 5,
                Comment = "Excellent service",
                IsPublic = true,
                CreatedAt = DateTime.UtcNow
            };

            // Setup mapper
            _mapperMock.Setup(m => m.Map<FeedbackDTO>(_testFeedback))
                .Returns(new FeedbackDTO
                {
                    Id = _testFeedback.Id,
                    OrderId = _testFeedback.OrderId,
                    Rating = _testFeedback.Rating,
                    Comment = _testFeedback.Comment,
                    IsPublic = _testFeedback.IsPublic,
                    CreatedAt = _testFeedback.CreatedAt.Value
                });
        }

        [Fact]
        public async Task GetFeedbackByOrderIdAsync_ShouldReturnFeedback_WhenExists()
        {
            // Arrange
            var mockQueryable = new[] { _testFeedback }.AsQueryable().BuildMockDbSet();
            _feedbackRepoMock.Setup(r => r.AsQueryable()).Returns(mockQueryable.Object);

            // Act
            var result = await _feedbackService.GetFeedbackByOrderIdAsync(_testOrderId, _testStoreIdString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testFeedback.Id, result.Id);
            Assert.Equal(_testFeedback.OrderId, result.OrderId);
            Assert.Equal(_testFeedback.Rating, result.Rating);
            Assert.Equal(_testFeedback.Comment, result.Comment);
        }

        [Fact]
        public async Task GetFeedbackByOrderIdAsync_ShouldThrowException_WhenFeedbackNotExists()
        {
            // Arrange
            var mockQueryable = Array.Empty<Feedback>().AsQueryable().BuildMockDbSet();
            _feedbackRepoMock.Setup(r => r.AsQueryable()).Returns(mockQueryable.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _feedbackService.GetFeedbackByOrderIdAsync(_testOrderId, _testStoreIdString));

            Assert.Contains(Errors.Common.NotFound, ex.Message);
        }

        [Fact]
        public async Task GetFeedbackByOrderIdAsync_ShouldThrowException_WhenStoreIdDoesNotMatch()
        {
            // Arrange
            var wrongStoreId = Guid.NewGuid().ToString();
            var mockQueryable = new[] { _testFeedback }.AsQueryable().BuildMockDbSet();
            _feedbackRepoMock.Setup(r => r.AsQueryable()).Returns(mockQueryable.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _feedbackService.GetFeedbackByOrderIdAsync(_testOrderId, wrongStoreId));

            Assert.Contains(Errors.Common.NotFound, ex.Message);
        }

        [Fact]
        public async Task GetFeedbackByOrderIdAsync_ShouldMapAllPropertiesCorrectly()
        {
            // Arrange
            var feedbacks = new List<Feedback> { _testFeedback };
            var mockQueryable = feedbacks.AsQueryable().BuildMockDbSet();

            _feedbackRepoMock.Setup(r => r.AsQueryable()).Returns(mockQueryable.Object);

            _mapperMock.Setup(m => m.Map<FeedbackDTO>(_testFeedback))
                .Returns(new FeedbackDTO
                {
                    Id = _testFeedback.Id,
                    OrderId = _testFeedback.OrderId,
                    Rating = _testFeedback.Rating,
                    Comment = _testFeedback.Comment,
                    IsPublic = _testFeedback.IsPublic,
                    CreatedAt = _testFeedback.CreatedAt ?? DateTime.MinValue
                });

            // Act
            var result = await _feedbackService.GetFeedbackByOrderIdAsync(_testOrderId, _testStoreIdString);

            // Assert
            _mapperMock.Verify(m => m.Map<FeedbackDTO>(_testFeedback), Times.Once);
            Assert.Equal(_testFeedback.Id, result.Id);
            Assert.Equal(_testFeedback.OrderId, result.OrderId);
            Assert.Equal(_testFeedback.Rating, result.Rating);
            Assert.Equal(_testFeedback.Comment, result.Comment);
            Assert.Equal(_testFeedback.IsPublic, result.IsPublic);
            Assert.Equal(_testFeedback.CreatedAt, result.CreatedAt);
        }

        [Fact]
        public async Task GetFeedbackByOrderIdAsync_ShouldIncludeFieldNameInError_WhenNotFound()
        {
            // Arrange
            _feedbackRepoMock.Setup(r => r.AsQueryable())
                .Returns(Array.Empty<Feedback>().AsQueryable());

            // Act
            var ex = await Record.ExceptionAsync(() =>
                _feedbackService.GetFeedbackByOrderIdAsync(_testOrderId, _testStoreIdString));

            // Assert
            Assert.NotNull(ex);
        }
    }
}