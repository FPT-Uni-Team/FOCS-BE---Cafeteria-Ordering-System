using FOCS.Application.DTOs;
using FOCS.Common.Exceptions;
using FOCS.Infrastructure.Identity.Identity.Model;
using Moq;

namespace FOCS.UnitTest.UserProfileServiceTest
{
    public class GetUserProfileTest : UserProfileServiceTestBase
    {
        [Fact]
        public async Task GetUserProfileAsync_WithValidUserId_ReturnsUserProfileDTO()
        {
            // Arrange
            _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                           .ReturnsAsync(_testUser);
            _mockMapper.Setup(m => m.Map<UserProfileDTO>(_testUser))
                      .Returns(_testUserProfileDTO);

            // Act
            var result = await _service.GetUserProfileAsync(_testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testUserProfileDTO.Email, result.Email);
            _mockUserManager.Verify(um => um.FindByIdAsync(_testUserId), Times.Once);
            _mockMapper.Verify(m => m.Map<UserProfileDTO>(_testUser), Times.Once);
        }

        [Fact]
        public async Task GetUserProfileAsync_WithNullUser_ThrowsException()
        {
            // Arrange
            _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                           .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.GetUserProfileAsync(_testUserId));

            Assert.Equal(Errors.Common.UserNotFound + "@" + Errors.FieldName.UserId, exception.Message);
            _mockUserManager.Verify(um => um.FindByIdAsync(_testUserId), Times.Once);
            _mockMapper.Verify(m => m.Map<UserProfileDTO>(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetUserProfileAsync_WithEmptyUserId_ReturnsNull()
        {
            // Arrange
            var emptyUserId = string.Empty;
            _mockUserManager.Setup(um => um.FindByIdAsync(emptyUserId))
                           .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.GetUserProfileAsync(emptyUserId));

            _mockUserManager.Verify(um => um.FindByIdAsync(emptyUserId), Times.Once);
        }
    }
}