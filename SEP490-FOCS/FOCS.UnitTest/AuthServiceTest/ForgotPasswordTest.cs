using FOCS.Common.Exceptions;
using FOCS.Infrastructure.Identity.Identity.Model;
using Moq;

namespace FOCS.UnitTest.AuthServiceTest
{
    public class ForgotPasswordTest : AuthServiceTestBase
    {
        [Fact]
        public async Task ForgotPasswordAsync_WithValidEmail_ShouldReturnTrue_WhenEmailSentSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var user = CreateValidUser(email);
            var resetToken = "reset_token_123";

            SetupValidUser(email, user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync(resetToken);
            _emailServiceMock.Setup(x => x.SendPasswordResetLinkAsync(email, resetToken))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.ForgotPasswordAsync(email);

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
            _emailServiceMock.Verify(x => x.SendPasswordResetLinkAsync(email, resetToken), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithValidEmail_ShouldReturnFalse_WhenEmailSendFails()
        {
            // Arrange
            var email = "test@example.com";
            var user = CreateValidUser(email);
            var resetToken = "reset_token_123";

            SetupValidUser(email, user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync(resetToken);
            _emailServiceMock.Setup(x => x.SendPasswordResetLinkAsync(email, resetToken))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.ForgotPasswordAsync(email);

            // Assert
            Assert.False(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
            _emailServiceMock.Verify(x => x.SendPasswordResetLinkAsync(email, resetToken), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithNonExistentEmail_ShouldThrowException()
        {
            // Arrange
            var email = "nonexistent@example.com";
            User nullUser = null;

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(nullUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.ForgotPasswordAsync(email));

            Assert.Equal(Errors.Common.UserNotFound + "@", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>()), Times.Never);
            _emailServiceMock.Verify(x => x.SendPasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithNullEmail_ShouldThrowException()
        {
            // Arrange
            string nullEmail = null;
            User nullUser = null;

            _userManagerMock.Setup(x => x.FindByEmailAsync(nullEmail))
                .ReturnsAsync(nullUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.ForgotPasswordAsync(nullEmail));

            Assert.Equal(Errors.Common.UserNotFound + "@", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(nullEmail), Times.Once);
            _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>()), Times.Never);
            _emailServiceMock.Verify(x => x.SendPasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WhenGeneratePasswordResetTokenAsyncThrowsException_ShouldPropagateException()
        {
            // Arrange
            var email = "test@example.com";
            var user = CreateValidUser(email);
            var expectedException = new InvalidOperationException("Token generation failed");

            SetupValidUser(email, user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.ForgotPasswordAsync(email));

            Assert.Equal("Token generation failed", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
            _emailServiceMock.Verify(x => x.SendPasswordResetLinkAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WhenEmailServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var email = "test@example.com";
            var user = CreateValidUser(email);
            var resetToken = "reset_token_123";
            var expectedException = new InvalidOperationException("Email service failed");

            SetupValidUser(email, user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync(resetToken);
            _emailServiceMock.Setup(x => x.SendPasswordResetLinkAsync(email, resetToken))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.ForgotPasswordAsync(email));

            Assert.Equal("Email service failed", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
            _emailServiceMock.Verify(x => x.SendPasswordResetLinkAsync(email, resetToken), Times.Once);
        }

        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user@domain.org")]
        [InlineData("admin@company.net")]
        public async Task ForgotPasswordAsync_WithDifferentValidEmails_ShouldReturnTrue(string email)
        {
            // Arrange
            var user = CreateValidUser(email);
            var resetToken = "reset_token_123";

            SetupValidUser(email, user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync(resetToken);
            _emailServiceMock.Setup(x => x.SendPasswordResetLinkAsync(email, resetToken))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.ForgotPasswordAsync(email);

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
            _emailServiceMock.Verify(x => x.SendPasswordResetLinkAsync(email, resetToken), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithValidUser_ShouldUseCorrectTokenInEmailService()
        {
            // Arrange
            var email = "test@example.com";
            var user = CreateValidUser(email);
            var expectedResetToken = "expected_reset_token_456";

            SetupValidUser(email, user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync(expectedResetToken);
            _emailServiceMock.Setup(x => x.SendPasswordResetLinkAsync(email, expectedResetToken))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.ForgotPasswordAsync(email);

            // Assert
            Assert.True(result);
            _emailServiceMock.Verify(x => x.SendPasswordResetLinkAsync(email, expectedResetToken), Times.Once);
            _emailServiceMock.Verify(x => x.SendPasswordResetLinkAsync(email, It.Is<string>(token => token != expectedResetToken)), Times.Never);
        }
    }
}