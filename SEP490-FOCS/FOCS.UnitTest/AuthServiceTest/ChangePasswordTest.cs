using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace FOCS.UnitTest.AuthServiceTest
{
    public class ChangePasswordTests : AuthServiceTestBase
    {
        [Fact]
        public async Task ChangePassword_WhenOldPasswordSameAsNewPassword_ShouldThrowException()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                OldPassword = "Password123!",
                NewPassword = "Password123!" // Same as old password
            };
            var email = "test@example.com";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.ChangePassword(request, email));

            Assert.Equal(Errors.AuthError.PasswordReuse + "@" + Errors.FieldName.NewPassword, exception.Message);
        }

        [Fact]
        public async Task ChangePassword_WhenUserNotFound_ShouldThrowException()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!"
            };
            var email = "nonexistent@example.com";

            // Setup user not found
            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.ChangePassword(request, email));

            Assert.Equal(Errors.Common.UserNotFound + "@", exception.Message);
        }

        [Fact]
        public async Task ChangePassword_WhenChangePasswordFails_ShouldThrowException()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!"
            };
            var email = "test@example.com";
            var user = CreateValidUser(email);

            // Setup user found
            SetupValidUser(email, user);

            // Setup change password to fail
            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" },
                new IdentityError { Code = "PasswordRequiresDigit", Description = "Password must contain a digit" }
            };

            var changePasswordResult = IdentityResult.Failed(identityErrors.ToArray());
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword))
                .ReturnsAsync(changePasswordResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.ChangePassword(request, email));

            var expectedErrorMessage = "Password is too short; Password must contain a digit@";
            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        [Fact]
        public async Task ChangePassword_WhenAllConditionsAreValid_ShouldReturnTrueAndRefreshSignIn()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!"
            };
            var email = "test@example.com";
            var user = CreateValidUser(email);

            // Setup user found
            SetupValidUser(email, user);

            // Setup change password to succeed
            var changePasswordResult = IdentityResult.Success;
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword))
                .ReturnsAsync(changePasswordResult);

            _signInManagerMock.Setup(x => x.RefreshSignInAsync(user))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.ChangePassword(request, email);

            // Assert
            Assert.True(result);

            // Verify all expected calls were made
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword), Times.Once);
            _signInManagerMock.Verify(x => x.RefreshSignInAsync(user), Times.Once);

            // Verify logger was called with success message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User changed their password successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangePassword_WhenSuccessful_ShouldLogInformationMessage()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!"
            };
            var email = "test@example.com";
            var user = CreateValidUser(email);

            // Setup user found
            SetupValidUser(email, user);

            // Setup change password to succeed
            var changePasswordResult = IdentityResult.Success;
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword))
                .ReturnsAsync(changePasswordResult);

            _signInManagerMock.Setup(x => x.RefreshSignInAsync(user))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.ChangePassword(request, email);

            // Assert
            Assert.True(result);

            // Verify the specific log message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User changed their password successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("", "NewPassword123!")]
        [InlineData("OldPassword123!", "")]
        [InlineData("", "")]
        public async Task ChangePassword_WhenPasswordsAreEmpty_ShouldStillProcessRequest(string oldPassword, string newPassword)
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                OldPassword = oldPassword,
                NewPassword = newPassword
            };
            var email = "test@example.com";
            var user = CreateValidUser(email);

            // Setup user found
            SetupValidUser(email, user);

            // Setup change password to fail due to empty passwords
            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Code = "PasswordEmpty", Description = "Password cannot be empty" }
            };

            var changePasswordResult = IdentityResult.Failed(identityErrors.ToArray());
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword))
                .ReturnsAsync(changePasswordResult);

            // Act & Assert
            if (oldPassword == newPassword)
            {
                // Should throw password reuse exception first
                var exception = await Assert.ThrowsAsync<Exception>(
                    () => _authService.ChangePassword(request, email));
                Assert.Equal(Errors.AuthError.PasswordReuse + "@" + Errors.FieldName.NewPassword, exception.Message);
            }
            else
            {
                // Should proceed to change password and fail with validation error
                var exception = await Assert.ThrowsAsync<Exception>(
                    () => _authService.ChangePassword(request, email));
                Assert.Equal("Password cannot be empty@", exception.Message);
            }
        }

        [Fact]
        public async Task ChangePassword_WhenChangePasswordSucceeds_ShouldNotThrowException()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!"
            };
            var email = "test@example.com";
            var user = CreateValidUser(email);

            // Setup user found
            SetupValidUser(email, user);

            // Setup change password to succeed
            var changePasswordResult = IdentityResult.Success;
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword))
                .ReturnsAsync(changePasswordResult);

            _signInManagerMock.Setup(x => x.RefreshSignInAsync(user))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.ChangePassword(request, email);

            // Assert
            Assert.True(result);

            // Verify all method calls were made in the correct order
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword), Times.Once);
            _signInManagerMock.Verify(x => x.RefreshSignInAsync(user), Times.Once);
        }
    }
}