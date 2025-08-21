using FOCS.Common.Exceptions;
using FOCS.Infrastructure.Identity.Identity.Model;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FOCS.UnitTest.UserProfileServiceTest
{
    public class DeleteUserProfileTest : UserProfileServiceTestBase
    {
        [Fact]
        public async Task DeleteUserProfileAsync_WithValidUserId_ReturnsTrue()
        {
            // Arrange
            _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                           .ReturnsAsync(_testUser);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                           .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.DeleteUserProfileAsync(_testUserId);

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(um => um.FindByIdAsync(_testUserId), Times.Once);
            _mockUserManager.Verify(um => um.UpdateAsync(It.Is<User>(u =>
                u.IsActive == false &&
                u.IsDeleted == true &&
                u.UpdatedBy == _testUserId &&
                u.UpdatedAt != null)), Times.Once);
        }

        [Fact]
        public async Task DeleteUserProfileAsync_WithNullUser_ThrowsException()
        {
            // Arrange
            _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                           .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.DeleteUserProfileAsync(_testUserId));

            Assert.Equal(Errors.Common.UserNotFound + "@" + Errors.FieldName.UserId, exception.Message);
            _mockUserManager.Verify(um => um.FindByIdAsync(_testUserId), Times.Once);
            _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserProfileAsync_SetsCorrectDeletionFlags()
        {
            // Arrange
            var beforeDelete = DateTime.UtcNow;
            _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                           .ReturnsAsync(_testUser);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                           .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.DeleteUserProfileAsync(_testUserId);
            var afterDelete = DateTime.UtcNow;

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(um => um.UpdateAsync(It.Is<User>(u =>
                u.IsActive == false &&
                u.IsDeleted == true &&
                u.UpdatedBy == _testUserId &&
                u.UpdatedAt >= beforeDelete &&
                u.UpdatedAt <= afterDelete)), Times.Once);
        }

        [Fact]
        public async Task DeleteUserProfileAsync_WithAlreadyDeletedUser_StillReturnsTrue()
        {
            // Arrange
            _testUser.IsDeleted = true;
            _testUser.IsActive = false;

            _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                           .ReturnsAsync(_testUser);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                           .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.DeleteUserProfileAsync(_testUserId);

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(um => um.UpdateAsync(It.Is<User>(u =>
                u.IsDeleted == true && u.IsActive == false)), Times.Once);
        }
    }
}