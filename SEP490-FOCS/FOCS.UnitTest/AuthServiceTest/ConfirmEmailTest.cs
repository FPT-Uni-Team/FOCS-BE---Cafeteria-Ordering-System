using FOCS.Common.Exceptions;
using FOCS.Infrastructure.Identity.Identity.Model;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FOCS.UnitTest.AuthServiceTest
{
    public class ConfirmEmailAsyncTests : AuthServiceTestBase
    {
        [Fact]
        public async Task ConfirmEmailAsync_WithValidEmailAndToken_ShouldReturnTrue()
        {
            // Arrange
            var email = "test@example.com";
            var token = "valid_token";
            var user = CreateValidUser(email);

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.ConfirmEmailAsync(email, token);

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithValidEmailAndToken_ShouldReturnFalse_WhenConfirmationFails()
        {
            // Arrange
            var email = "test@example.com";
            var token = "invalid_token";
            var user = CreateValidUser(email);

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Code = "InvalidToken", Description = "Invalid token." }
            };

            _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _authService.ConfirmEmailAsync(email, token);

            // Assert
            Assert.False(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithNullUser_ShouldThrowException()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var token = "any_token";

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.ConfirmEmailAsync(email, token));

            Assert.Equal(Errors.Common.UserNotFound + "@", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithEmptyEmail_ShouldThrowException()
        {
            // Arrange
            var email = "";
            var token = "any_token";

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.ConfirmEmailAsync(email, token));

            Assert.Equal(Errors.Common.UserNotFound + "@", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithNullEmail_ShouldThrowException()
        {
            // Arrange
            string email = null;
            var token = "any_token";

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _authService.ConfirmEmailAsync(email, token));

            Assert.Equal(Errors.Common.UserNotFound + "@", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithValidEmailAndNullToken_ShouldCallConfirmEmailAsync()
        {
            // Arrange
            var email = "test@example.com";
            string token = null;
            var user = CreateValidUser(email);

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token." }));

            // Act
            var result = await _authService.ConfirmEmailAsync(email, token);

            // Assert
            Assert.False(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithValidEmailAndEmptyToken_ShouldCallConfirmEmailAsync()
        {
            // Arrange
            var email = "test@example.com";
            var token = "";
            var user = CreateValidUser(email);

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token." }));

            // Act
            var result = await _authService.ConfirmEmailAsync(email, token);

            // Assert
            Assert.False(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithDifferentCaseEmail_ShouldStillFindUser()
        {
            // Arrange
            var email = "TEST@EXAMPLE.COM";
            var token = "valid_token";
            var user = CreateValidUser("test@example.com");

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.ConfirmEmailAsync(email, token);

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WhenUserManagerThrowsException_ShouldPropagateException()
        {
            // Arrange
            var email = "test@example.com";
            var token = "valid_token";

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.ConfirmEmailAsync(email, token));

            Assert.Equal("Database connection failed", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WhenConfirmEmailAsyncThrowsException_ShouldPropagateException()
        {
            // Arrange
            var email = "test@example.com";
            var token = "valid_token";
            var user = CreateValidUser(email);

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
                .ThrowsAsync(new InvalidOperationException("Email confirmation failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.ConfirmEmailAsync(email, token));

            Assert.Equal("Email confirmation failed", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
        }

        [Theory]
        [InlineData("user@example.com", "valid_token")]
        [InlineData("another@test.com", "another_token")]
        [InlineData("special.email+tag@domain.co.uk", "special_token")]
        public async Task ConfirmEmailAsync_WithVariousValidInputs_ShouldReturnTrue(string email, string token)
        {
            // Arrange
            var user = CreateValidUser(email);

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.ConfirmEmailAsync(email, token);

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithMultipleIdentityErrors_ShouldReturnFalse()
        {
            // Arrange
            var email = "test@example.com";
            var token = "invalid_token";
            var user = CreateValidUser(email);

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Code = "InvalidToken", Description = "Invalid token." },
                new IdentityError { Code = "TokenExpired", Description = "Token has expired." }
            };

            _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _authService.ConfirmEmailAsync(email, token);

            // Assert
            Assert.False(result);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
        }
    }
}