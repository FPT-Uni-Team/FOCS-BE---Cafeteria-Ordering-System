using AutoMapper;
using FOCS.Application.Services;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Http;
using MockQueryable;
using Moq;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.UnitTest
{
    public class FeedbackUnitTest
    {
        private readonly Mock<IRepository<Feedback>> _feedbackRepoMock = new();
        private readonly Mock<IRepository<OrderEntity>> _orderRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ICloudinaryService> _cloudinaryServiceMock = new();

        private readonly FeedbackService _feedbackService;

        public FeedbackUnitTest()
        {
            _feedbackService = new FeedbackService(
                _feedbackRepoMock.Object,
                _orderRepoMock.Object,
                _mapperMock.Object,
                _cloudinaryServiceMock.Object
            );
        }

        #region SubmitFeedbackAsync CM-73
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", 0, "Comment String", true)]
        [InlineData(null, "a2fca33f-2ff6-4697-903f-1cbe644f5139", 0, "Comment String", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, 0, "Comment String", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", 1, "Comment String", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", null, "Comment String", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", 0, "", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", 0, null, true)]
        [InlineData(null, null, null, null, true)]
        public async Task SubmitFeedbackAsync_SimpleRun_ChecksIfServiceRuns(
            string orderIdStr, string actorIdStr, int rating, string comment, bool shouldSucceed)
        {
            var orderId = string.IsNullOrEmpty(orderIdStr) ? (Guid?)null : Guid.Parse(orderIdStr);
            var actorId = string.IsNullOrEmpty(actorIdStr) ? (Guid?)null : Guid.Parse(actorIdStr);
            // Arrange
            var request = new CreateFeedbackRequest
            {
                OrderId = orderId ?? Guid.Empty,
                ActorId = actorId ?? Guid.Empty,
                Rating = rating,
                Comment = comment,
                Files = new List<IFormFile>()
            };

            var storeId = Guid.NewGuid().ToString();

            // Setup mock
            _cloudinaryServiceMock.Setup(c => c.UploadImageFeedbackAsync(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new List<UploadedImageResult>
            {
                new UploadedImageResult { Url = "https://example.com/image1.jpg" }
            });

            _mapperMock.Setup(m => m.Map<Feedback>(It.IsAny<CreateFeedbackRequest>()))
                .Returns(new Feedback());

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _feedbackService.SubmitFeedbackAsync(request, storeId);
            });

            // Assert
            Assert.Null(exception);
        }
        #endregion

        #region GetAllFeedbacksAsync CM-74
        [Theory]
        [InlineData(1, 10, "comment", "search comment", "created_date", "desc")]
        [InlineData(5, 10, "comment", "search comment", "created_date", "desc")]
        [InlineData(null, 10, "comment", "search comment", "created_date", "desc")]
        [InlineData(1, 20, "comment", "search comment", "created_date", "desc")]
        [InlineData(1, null, "comment", "search comment", "created_date", "desc")]
        [InlineData(1, 10, null, "search comment", "created_date", "desc")]
        [InlineData(1, 10, "comment", null, "created_date", "desc")]
        [InlineData(1, 10, "comment", "search comment", "rating", "desc")]
        [InlineData(1, 10, "comment", "search comment", null, "desc")]
        [InlineData(1, 10, "comment", "search comment", "created_date", "asc")]
        [InlineData(1, 10, "comment", "search comment", "created_date", null)]
        [InlineData(null, null, null, null, null, null)]
        public async Task GetAllFeedbacksAsync_SimpleRun_ChecksIfServiceRuns(
            int? page, int? pageSize, string searchBy, string searchValue, string sortBy, string sortOrder)
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 10,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Filters = new Dictionary<string, string>()
            };

            var storeId = Guid.NewGuid().ToString();

            // Setup mock
            _feedbackRepoMock.Setup(r => r.AsQueryable())
                .Returns(new List<Feedback>().BuildMock());

            _mapperMock.Setup(m => m.Map<List<FeedbackDTO>>(It.IsAny<List<Feedback>>()))
                .Returns(new List<FeedbackDTO>());

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _feedbackService.GetAllFeedbacksAsync(query, storeId);
            });

            // Assert
            Assert.Null(exception);
        }
        #endregion

        #region GetFeedbackByOrderIdAsync CM-75
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", "a2fca33f-2ff6-4697-903f-1cbe644f5139", false)]
        [InlineData(null, "a2fca33f-2ff6-4697-903f-1cbe644f5139", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, false)]
        [InlineData(null, null, false)]
        public async Task GetFeedbackByOrderIdAsync_SimpleRun_ChecksIfServiceRuns(
            string orderIdStr, string storeIdStr, bool shouldSucceed)
        {
            // Arrange
            var orderId = string.IsNullOrEmpty(orderIdStr) ? (Guid?)null : Guid.Parse(orderIdStr);
            var storeId = string.IsNullOrEmpty(storeIdStr) ? (Guid?)null : Guid.Parse(storeIdStr);

            // Setup mock
            if (shouldSucceed)
            {
                _feedbackRepoMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Feedback>
                    {
                        new Feedback { OrderId = orderId ?? Guid.Empty, StoreId = storeId ?? Guid.Empty }
                    }.BuildMock());
            }
            else
            {
                _feedbackRepoMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Feedback>().BuildMock());
            }

            _mapperMock.Setup(m => m.Map<FeedbackDTO>(It.IsAny<Feedback>()))
                .Returns(new FeedbackDTO());

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _feedbackService.GetFeedbackByOrderIdAsync(orderId.ToString() ?? string.Empty, storeId.ToString());
            });
            if (shouldSucceed) 
            {
                // Assert
                Assert.Null(exception);
            }
            else
            {
                Assert.NotNull(exception);
            }

        }
        #endregion

        #region GetFeedbackByMenuItemAsync CM-76
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "a2fca33f-2ff6-4697-903f-1cbe644f5139", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", "a2fca33f-2ff6-4697-903f-1cbe644f5139", false)]
        [InlineData(null, "a2fca33f-2ff6-4697-903f-1cbe644f5139", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, false)]
        [InlineData(null, null, false)]
        public async Task GetFeedbackByMenuItemAsync_SimpleRun_ChecksIfServiceRuns(
            string menuItemIdStr, string storeIdStr, bool shouldSucceed)
        {
            // Arrange
            var menuItemId = string.IsNullOrEmpty(menuItemIdStr) ? (Guid?)null : Guid.Parse(menuItemIdStr);
            var storeId = storeIdStr;

            // Setup mock
            if (shouldSucceed)
            {
                _feedbackRepoMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Feedback>
                    {
                        new Feedback
                        {
                            Order = new OrderEntity
                            {
                                OrderDetails = new List<OrderDetail>
                                {
                                    new OrderDetail { MenuItemId = menuItemId ?? Guid.Empty }
                                },
                                StoreId = Guid.Parse(storeId)
                            }
                        }
                    }.BuildMock());
            }
            else
            {
                _feedbackRepoMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Feedback>().BuildMock());
            }

            _mapperMock.Setup(m => m.Map<List<FeedbackDTO>>(It.IsAny<List<Feedback>>()))
                .Returns(new List<FeedbackDTO>());

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _feedbackService.GetFeedbackByMenuItemAsync(menuItemId ?? Guid.Empty, storeId);
            });

            // Assert
            Assert.Null(exception);
        }
        #endregion
    }
}
