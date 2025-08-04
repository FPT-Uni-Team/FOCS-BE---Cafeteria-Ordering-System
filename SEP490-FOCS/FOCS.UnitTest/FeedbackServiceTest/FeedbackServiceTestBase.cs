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
    }
}
