using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FOCS.UnitTest.FeedbackServiceTest;

public class SubmitFeedbackTests : FeedbackServiceTestBase
{
    [Fact]
    public async Task SubmitFeedback_ShouldSucceed_WhenValidRequestWithoutImages()
    {
        // Arrange
        var request = CreateSampleRequest();
        request.Files = new List<IFormFile>();
        var storeId = Guid.NewGuid().ToString();

        var feedback = CreateSampleFeedback(request.OrderId, storeId);
        SetupMapper(request, feedback);

        // Mock trả về empty list thay vì null
        SetupUploadImageFeedback(new List<string>());

        _feedbackRepoMock.Setup(r => r.AddAsync(It.IsAny<Feedback>()))
                         .Returns(Task.CompletedTask);
        _feedbackRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _feedbackService.SubmitFeedbackAsync(request, storeId);

        // Assert
        Assert.True(result);
        VerifyUploadImageCalled(Times.Once());
        VerifyFeedbackAdded(Times.Once());
    }

    [Fact]
    public async Task SubmitFeedback_ShouldUploadImages_WhenFilesProvided()
    {
        // Arrange
        var request = CreateSampleRequest();
        var mockFile = new Mock<IFormFile>();
        request.Files = new List<IFormFile> { mockFile.Object };
        var storeId = Guid.NewGuid().ToString();

        var imageUrls = new List<string> { "image1.jpg", "image2.jpg" };
        SetupUploadImageFeedback(imageUrls);

        var feedback = CreateSampleFeedback(request.OrderId, storeId);
        SetupMapper(request, feedback);

        _feedbackRepoMock.Setup(r => r.AddAsync(It.IsAny<Feedback>())).Returns(Task.CompletedTask);
        _feedbackRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _feedbackService.SubmitFeedbackAsync(request, storeId);

        // Assert
        VerifyUploadImageCalledWithFiles(Times.Once());
        VerifyFeedbackAdded(Times.Once());
    }

    [Fact]
    public async Task SubmitFeedback_ShouldReturnFalse_WhenStoreIdIsInvalidGuid()
    {
        // Arrange
        var request = CreateSampleRequest();
        var invalidStoreId = "not-a-guid";

        // Act
        var result = await _feedbackService.SubmitFeedbackAsync(request, invalidStoreId);

        // Assert
        Assert.False(result);
        VerifyFeedbackAdded(Times.Never());
    }

    [Fact]
    public async Task SubmitFeedback_ShouldReturnFalse_WhenMappingFails()
    {
        // Arrange
        var request = CreateSampleRequest();
        var storeId = Guid.NewGuid().ToString();

        _mapperMock
            .Setup(m => m.Map<Feedback>(It.IsAny<CreateFeedbackRequest>()))
            .Throws(new Exception("Mapping failed"));

        // Act
        var result = await _feedbackService.SubmitFeedbackAsync(request, storeId);

        // Assert
        Assert.False(result); 
        VerifyFeedbackAdded(Times.Never()); 
    }

    [Fact]
    public async Task SubmitFeedback_ShouldCallUploadImageFeedbackAsync_WhenFilesProvided()
    {
        // Arrange
        var request = CreateSampleRequest();
        request.Files = new List<IFormFile> { new Mock<IFormFile>().Object };
        var storeId = Guid.NewGuid().ToString();

        var feedback = CreateSampleFeedback(request.OrderId, storeId);
        SetupMapper(request, feedback);
        SetupUploadImageFeedback(new List<string> { "img1", "img2" });

        _feedbackRepoMock.Setup(r => r.AddAsync(It.IsAny<Feedback>())).Returns(Task.CompletedTask);
        _feedbackRepoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _feedbackService.SubmitFeedbackAsync(request, storeId);

        // Assert
        VerifyUploadImageCalledWithFiles(Times.Once());
    }
}
