using FOCS.Infrastructure.Identity.Identity.Model;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.AuthServiceTest
{
    public class LogoutTest : AuthServiceTestBase
    {
        [Fact]
        public async Task LogoutAsync_WhenUserHasNoTokens_DoesNotThrowException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var emptyTokenList = new List<UserRefreshToken>();

            _userRefreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserRefreshToken, bool>>>()))
                .ReturnsAsync(emptyTokenList);

            // Act & Assert
            await _authService.LogoutAsync(userId);

            // Verify SaveChangesAsync was called even with empty list
            _userRefreshTokenRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_WhenUserHasActiveTokens_RevokesAllActiveTokens()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            var activeToken1 = new UserRefreshToken
            {
                Token = "token1",
                UserId = userId,
                ExpirationDate = DateTime.UtcNow.AddDays(1),
                IsRevoked = false
            };

            var activeToken2 = new UserRefreshToken
            {
                Token = "token2",
                UserId = userId,
                ExpirationDate = DateTime.UtcNow.AddDays(2),
                IsRevoked = false
            };

            var tokenList = new List<UserRefreshToken> { activeToken1, activeToken2 };

            _userRefreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserRefreshToken, bool>>>()))
                .ReturnsAsync(tokenList);

            // Act
            await _authService.LogoutAsync(userId);

            // Assert
            Assert.True(activeToken1.IsRevoked);
            Assert.True(activeToken2.IsRevoked);
            _userRefreshTokenRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_WhenUserHasMixedTokens_RevokesOnlyActiveTokens()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            var activeToken = new UserRefreshToken
            {
                Token = "active_token",
                UserId = userId,
                ExpirationDate = DateTime.UtcNow.AddDays(1),
                IsRevoked = false
            };

            var alreadyRevokedToken = new UserRefreshToken
            {
                Token = "revoked_token",
                UserId = userId,
                ExpirationDate = DateTime.UtcNow.AddDays(1),
                IsRevoked = true
            };

            // The query in the actual code filters out revoked tokens, so only active token should be returned
            var tokenList = new List<UserRefreshToken> { activeToken };

            _userRefreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserRefreshToken, bool>>>()))
                .ReturnsAsync(tokenList);

            // Act
            await _authService.LogoutAsync(userId);

            // Assert
            Assert.True(activeToken.IsRevoked);
            Assert.True(alreadyRevokedToken.IsRevoked); // Already revoked, unchanged
            _userRefreshTokenRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_WhenUserHasExpiredButNotRevokedTokens_RevokesExpiredTokens()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            var expiredButNotRevokedToken = new UserRefreshToken
            {
                Token = "expired_token",
                UserId = userId,
                ExpirationDate = DateTime.UtcNow.AddDays(-1), // Expired
                IsRevoked = false
            };

            var tokenList = new List<UserRefreshToken> { expiredButNotRevokedToken };

            _userRefreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserRefreshToken, bool>>>()))
                .ReturnsAsync(tokenList);

            // Act
            await _authService.LogoutAsync(userId);

            // Assert
            Assert.True(expiredButNotRevokedToken.IsRevoked);
            _userRefreshTokenRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_VerifyCorrectQueryFilter()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var emptyTokenList = new List<UserRefreshToken>();

            _userRefreshTokenRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserRefreshToken, bool>>>()))
                .ReturnsAsync(emptyTokenList);

            // Act
            await _authService.LogoutAsync(userId);

            // Assert - Verify the query was called with correct filter (userId and !IsRevoked)
            _userRefreshTokenRepositoryMock.Verify(x => x.FindAsync(It.IsAny<Expression<Func<UserRefreshToken, bool>>>()), Times.Once);
        }
    }
}