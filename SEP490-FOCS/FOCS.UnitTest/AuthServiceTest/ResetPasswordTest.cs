using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FOCS.UnitTest.AuthServiceTest
{
    public class ResetPasswordAsyncTests : AuthServiceTestBase
    {
        [Fact]
        public async Task ResetPasswordAsync_WithValidRequest_ShouldReturnTrue_WhenResetSucceeds()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "valid_reset_token",
                NewPassword = "NewPassword123!"
            };
            var user = CreateValidUser(request.Email);
            var resetResult = IdentityResult.Success;

            SetupValidUser(request.Email, user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword))
                .ReturnsAsync(resetResult);

            // Act
            var result = await _authService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_WithValidRequest_ShouldReturnFalse_WhenResetFails()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "invalid_reset_token",
                NewPassword = "NewPassword123!"
            };
            var user = CreateValidUser(request.Email);
            var resetResult = IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidToken",
                Description = "Invalid token"
            });

            SetupValidUser(request.Email, user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword))
                .ReturnsAsync(resetResult);

            // Act
            var result = await _authService.ResetPasswordAsync(request);

            // Assert
            Assert.False(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_WithNonExistentUser_ShouldThrowException()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "nonexistent@example.com",
                Token = "valid_reset_token",
                NewPassword = "NewPassword123!"
            };
            User nullUser = null;

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(nullUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.ResetPasswordAsync(request));

            Assert.Equal(Errors.Common.UserNotFound + "@", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResetPasswordAsync_WithNullRequest_ShouldThrowException()
        {
            // Arrange
            ResetPasswordRequest nullRequest = null;

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(
                () => _authService.ResetPasswordAsync(nullRequest));

            _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResetPasswordAsync_WithNullEmail_ShouldThrowException()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = null,
                Token = "valid_reset_token",
                NewPassword = "NewPassword123!"
            };
            User nullUser = null;

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(nullUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.ResetPasswordAsync(request));

            Assert.Equal(Errors.Common.UserNotFound + "@", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResetPasswordAsync_WithEmptyEmail_ShouldThrowException()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = string.Empty,
                Token = "valid_reset_token",
                NewPassword = "NewPassword123!"
            };
            User nullUser = null;

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(nullUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.ResetPasswordAsync(request));

            Assert.Equal(Errors.Common.UserNotFound + "@", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResetPasswordAsync_WhenResetPasswordAsyncThrowsException_ShouldPropagateException()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "valid_reset_token",
                NewPassword = "NewPassword123!"
            };
            var user = CreateValidUser(request.Email);
            var expectedException = new InvalidOperationException("Password reset failed");

            SetupValidUser(request.Email, user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.ResetPasswordAsync(request));

            Assert.Equal("Password reset failed", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword), Times.Once);
        }

        [Theory]
        [InlineData("test@example.com", "token123", "NewPassword123!")]
        [InlineData("user@domain.org", "resetToken456", "SecurePass456!")]
        [InlineData("admin@company.net", "validToken789", "StrongPassword789!")]
        public async Task ResetPasswordAsync_WithDifferentValidRequests_ShouldReturnTrue(string email, string token, string newPassword)
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = email,
                Token = token,
                NewPassword = newPassword
            };
            var user = CreateValidUser(email);
            var resetResult = IdentityResult.Success;

            SetupValidUser(email, user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, token, newPassword))
                .ReturnsAsync(resetResult);

            // Act
            var result = await _authService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, token, newPassword), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_WithMultipleFailureReasons_ShouldReturnFalse()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "invalid_token",
                NewPassword = "weak"
            };
            var user = CreateValidUser(request.Email);
            var resetResult = IdentityResult.Failed(
                new IdentityError { Code = "InvalidToken", Description = "Invalid token" },
                new IdentityError { Code = "PasswordTooWeak", Description = "Password is too weak" }
            );

            SetupValidUser(request.Email, user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword))
                .ReturnsAsync(resetResult);

            // Act
            var result = await _authService.ResetPasswordAsync(request);

            // Assert
            Assert.False(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_WithValidUser_ShouldUseCorrectParametersInResetPassword()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "test@example.com",
                Token = "specific_token_123",
                NewPassword = "SpecificPassword123!"
            };
            var user = CreateValidUser(request.Email);
            var resetResult = IdentityResult.Success;

            SetupValidUser(request.Email, user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword))
                .ReturnsAsync(resetResult);

            // Act
            var result = await _authService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, "specific_token_123", "SpecificPassword123!"), Times.Once);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, It.Is<string>(t => t != "specific_token_123"), It.IsAny<string>()), Times.Never);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, It.IsAny<string>(), It.Is<string>(p => p != "SpecificPassword123!")), Times.Never);
        }
    }

    // Helper extension class for creating ResetPasswordRequest
    public static class ResetPasswordRequestExtensions
    {
        public static ResetPasswordRequest CreateValidResetPasswordRequest(
            string email = "test@example.com",
            string token = "valid_token",
            string newPassword = "NewPassword123!")
        {
            return new ResetPasswordRequest
            {
                Email = email,
                Token = token,
                NewPassword = newPassword
            };
        }
    }
}