using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.AuthServiceTest
{
    public class RefreshTokenTest : AuthServiceTestBase
    {
        [Fact]
        public async Task RefreshTokenAsync_WhenTokenNotFound_ReturnsFailureResult()
        {
            // Arrange
            var refreshToken = "invalid_token";
            var storeId = Guid.NewGuid();
            var emptyTokenList = new List<UserRefreshToken>();

            _userRefreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserRefreshToken, bool>>>()))
                .ReturnsAsync(emptyTokenList);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken, storeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(Errors.AuthError.InvalidRefreshToken, result.Errors);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenTokenExpired_ReturnsFailureResult()
        {
            // Arrange
            var refreshToken = "expired_token";
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var expiredToken = new UserRefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                ExpirationDate = DateTime.UtcNow.AddDays(-1), // Expired
                IsRevoked = false
            };

            var tokenList = new List<UserRefreshToken> { expiredToken };

            _userRefreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserRefreshToken, bool>>>()))
                .ReturnsAsync(tokenList);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken, storeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(Errors.AuthError.InvalidRefreshToken, result.Errors);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenTokenValidButUserNotFound_ReturnsFailureResult()
        {
            // Arrange
            var refreshToken = "valid_token";
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var validToken = new UserRefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                ExpirationDate = DateTime.UtcNow.AddDays(1), // Valid
                IsRevoked = false
            };

            var tokenList = new List<UserRefreshToken> { validToken };

            _userRefreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserRefreshToken, bool>>>()))
                .ReturnsAsync(tokenList);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken, storeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(Errors.Common.NotFound, result.Errors);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenTokenValidAndUserExists_RevokesOldTokenAndGeneratesNewAuthResult()
        {
            // Arrange
            var refreshToken = "valid_token";
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var user = CreateValidUser();
            user.Id = userId;

            var validToken = new UserRefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                ExpirationDate = DateTime.UtcNow.AddDays(1), // Valid
                IsRevoked = false
            };

            var tokenList = new List<UserRefreshToken> { validToken };

            _userRefreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserRefreshToken, bool>>>()))
                .ReturnsAsync(tokenList);

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            SetupConfiguration();
            SetupRepositoryAdd(_userRefreshTokenRepositoryMock);
            SetupUserRoles(user, new List<string> { "User" });
            SetupTokenGeneration("new_access_token", "new_refresh_token");
            SetupMapperForUserRefreshToken(It.IsAny<UserRefreshTokenDTO>(), new UserRefreshToken());

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken, storeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("new_access_token", result.AccessToken);
            Assert.Equal("new_refresh_token", result.RefreshToken);
            Assert.Null(result.Errors);

            // Verify old token was revoked
            Assert.True(validToken.IsRevoked);

            // Verify repository operations
            _userRefreshTokenRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2)); // Once for revoking, once for new token
            _userRefreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserRefreshToken>()), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenTokenIsRevoked_ReturnsFailureResult()
        {
            // Arrange
            var refreshToken = "revoked_token";
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var revokedToken = new UserRefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                ExpirationDate = DateTime.UtcNow.AddDays(1), // Valid expiration
                IsRevoked = true // But revoked
            };

            var tokenList = new List<UserRefreshToken> { revokedToken };

            _userRefreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserRefreshToken, bool>>>()))
                .ReturnsAsync(new List<UserRefreshToken>()); // Empty list because query filters out revoked tokens

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken, storeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(Errors.AuthError.InvalidRefreshToken, result.Errors);
        }
    }
}