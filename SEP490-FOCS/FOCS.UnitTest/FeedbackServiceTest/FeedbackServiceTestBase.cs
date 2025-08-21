using AutoMapper;
using FOCS.Application.Services;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Http;
using Moq;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.UnitTest.FeedbackServiceTest
{
    public abstract class FeedbackServiceTestBase
    {
        protected readonly Mock<IRepository<Feedback>> _feedbackRepoMock;
        protected readonly Mock<IRepository<OrderEntity>> _orderRepoMock;
        protected readonly Mock<IMapper> _mapperMock;
        protected readonly Mock<ICloudinaryService> _cloudinaryServiceMock;

        protected readonly FeedbackService _feedbackService;

        protected FeedbackServiceTestBase()
        {
            _feedbackRepoMock = new Mock<IRepository<Feedback>>();
            _orderRepoMock = new Mock<IRepository<OrderEntity>>();
            _mapperMock = new Mock<IMapper>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            _feedbackService = new FeedbackService(
                _feedbackRepoMock.Object,
                _orderRepoMock.Object,
                _mapperMock.Object,
                _cloudinaryServiceMock.Object
            );
        }

        protected void SetupUploadImageFeedback(List<string> imageUrls)
        {
            var uploadedImages = imageUrls.Select(url => new UploadedImageResult { Url = url }).ToList();
            _cloudinaryServiceMock
                .Setup(s => s.UploadImageFeedbackAsync(It.IsAny<List<IFormFile>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(uploadedImages);
        }

        protected void SetupMapper(CreateFeedbackRequest request, Feedback? overrideFeedback = null, Guid? storeId = null)
        {
            _mapperMock.Setup(m => m.Map<Feedback>(It.Is<CreateFeedbackRequest>(r => r == request)))
                       .Returns(() =>
                       {
                           if (overrideFeedback != null)
                               return overrideFeedback;

                           return new Feedback
                           {
                               Id = Guid.NewGuid(),
                               Rating = request.Rating,
                               Comment = request.Comment,
                               UserId = request.ActorId,
                               Images = new List<string>(),
                               IsPublic = false,
                               CreatedAt = DateTime.UtcNow,
                               OrderId = request.OrderId,
                               StoreId = storeId ?? Guid.NewGuid()
                           };
                       });
        }

        protected void VerifyFeedbackAdded(Times times)
        {
            _feedbackRepoMock.Verify(r => r.AddAsync(It.IsAny<Feedback>()), times);
            _feedbackRepoMock.Verify(r => r.SaveChangesAsync(), times);
        }

        protected void VerifyUploadImageCalled(Times times)
        {
            _cloudinaryServiceMock.Verify(s =>
                s.UploadImageFeedbackAsync(
                    It.IsAny<List<IFormFile>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                times);
        }

        protected void VerifyUploadImageCalledWithFiles(Times times)
        {
            _cloudinaryServiceMock.Verify(s =>
                s.UploadImageFeedbackAsync(
                    It.Is<List<IFormFile>>(files => files != null && files.Count > 0),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                times);
        }

        protected static CreateFeedbackRequest CreateSampleRequest()
        {
            return new CreateFeedbackRequest
            {
                Rating = 5,
                Comment = "Great service!",
                Files = new List<IFormFile>(),
                OrderId = Guid.NewGuid(),
                ActorId = Guid.NewGuid()
            };
        }

        protected static Feedback CreateSampleFeedback(Guid orderId, string storeId)
        {
            return new Feedback
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                StoreId = Guid.Parse(storeId),
                Comment = "Sample feedback",
                Rating = 4,
                Images = new List<string> { "url1", "url2" },
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid().ToString()
            };
        }

        protected static UrlQueryParameters CreateSampleQueryParameters()
        {
            return new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SearchBy = "comment",
                SearchValue = "great",
                SortBy = "created_date",
                SortOrder = "desc",
                Filters = new Dictionary<string, string>
                {
                    { "rating", "5" }
                }
            };
        }

        // Add these methods to your FeedbackServiceTestBase class

        protected Feedback CreateTestFeedback(
            Guid? id = null,
            int rating = 5,
            string comment = "Test comment",
            Guid? userId = null,
            List<string> images = null,
            bool isPublic = true,
            DateTime? createdAt = null,
            Guid? storeId = null,
            Guid? orderId = null,
            string reply = null)
        {
            return new Feedback
            {
                Id = id ?? Guid.NewGuid(),
                Rating = rating,
                Comment = comment,
                UserId = userId,
                Images = images ?? new List<string>(),
                IsPublic = isPublic,
                CreatedAt = createdAt ?? DateTime.UtcNow,
                CreatedBy = "test-user",
                StoreId = storeId ?? Guid.NewGuid(),
                OrderId = orderId ?? Guid.NewGuid()
            };
        }

        protected FeedbackDTO CreateTestFeedbackDTO(
            Guid? id = null,
            int rating = 5,
            string comment = "Test comment",
            Guid? userId = null,
            List<string> images = null,
            bool isPublic = true,
            DateTime? createdAt = null,
            Guid? orderId = null,
            string reply = null)
        {
            return new FeedbackDTO
            {
                Id = id ?? Guid.NewGuid(),
                Rating = rating,
                Comment = comment,
                UserId = userId ?? Guid.NewGuid(),
                Images = images ?? new List<string>(),
                IsPublic = isPublic,
                CreatedAt = createdAt ?? DateTime.UtcNow,
                OrderId = orderId ?? Guid.NewGuid(),
                Reply = reply
            };
        }

        protected List<Feedback> CreateTestFeedbacksForStore(Guid storeId, int count = 5)
        {
            var feedbacks = new List<Feedback>();
            var comments = new[]
            {
                "Excellent service! Highly recommended!",
                "Good experience overall",
                "Average service, could improve",
                "Poor experience, needs improvement",
                "Terrible, would not recommend"
            };

            for (int i = 0; i < count; i++)
            {
                feedbacks.Add(CreateTestFeedback(
                    rating: 5 - i, // Ratings from 5 to 1
                    comment: comments[i],
                    isPublic: i % 2 == 0, // Alternate public status
                    createdAt: DateTime.UtcNow.AddDays(-i), // Different dates
                    storeId: storeId,
                    userId: Guid.NewGuid(),
                    images: i % 3 == 0 ? new List<string> { $"image_{i}.jpg" } : null // Some with images
                ));
            }
            return feedbacks;
        }

        protected void SetupRepositoryWithFeedbacks(List<Feedback> feedbacks)
        {
            _feedbackRepoMock.Setup(r => r.AsQueryable()).Returns(feedbacks.AsQueryable());
        }

        protected void SetupMapperForFeedbackDTO(List<Feedback> feedbacks = null)
        {
            _mapperMock.Setup(m => m.Map<List<FeedbackDTO>>(It.IsAny<List<Feedback>>()))
                .Returns((List<Feedback> source) => source.Select(f => new FeedbackDTO
                {
                    Id = f.Id,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    UserId = f.UserId ?? Guid.Empty,
                    Images = f.Images,
                    IsPublic = f.IsPublic,
                    CreatedAt = f.CreatedAt ?? DateTime.MinValue,
                    OrderId = f.OrderId,
                    Reply = null // Assuming reply isn't mapped from Feedback entity
                }).ToList());
        }

        protected void SetupMapperForSingleFeedback(Feedback feedback)
        {
            _mapperMock.Setup(m => m.Map<FeedbackDTO>(feedback))
                .Returns(new FeedbackDTO
                {
                    Id = feedback.Id,
                    Rating = feedback.Rating,
                    Comment = feedback.Comment,
                    UserId = feedback.UserId ?? Guid.Empty,
                    Images = feedback.Images,
                    IsPublic = feedback.IsPublic,
                    CreatedAt = feedback.CreatedAt ?? DateTime.MinValue,
                    OrderId = feedback.OrderId
                });
        }

        protected UrlQueryParameters CreateQueryParameters(
            int page = 1,
            int pageSize = 10,
            string searchBy = null,
            string searchValue = null,
            string sortBy = null,
            string sortOrder = null,
            Dictionary<string, string> filters = null)
        {
            return new UrlQueryParameters
            {
                Page = page,
                PageSize = pageSize,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Filters = filters
            };
        }
    }
}
