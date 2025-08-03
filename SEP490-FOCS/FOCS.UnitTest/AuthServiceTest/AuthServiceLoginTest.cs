using FOCS.Common.Constants;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using System.Security.Claims;

namespace FOCS.UnitTest.AuthServiceTest
{
    public class AuthServiceLoginTests : AuthServiceTestBase
    {
        [Fact]
        public async Task LoginAsync_WhenStoreNotFound_ReturnsFailureResult()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var request = CreateValidLoginRequest();

            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync((Store)null);

            // Act
            var result = await _authService.LoginAsync(request, storeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(Errors.Common.StoreNotFound, result.Errors);
        }

        [Fact]
        public async Task LoginAsync_WhenUserNotFound_ReturnsFailureResult()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var request = CreateValidLoginRequest("nonexistent@example.com");

            SetupValidStore(storeId, store);
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.LoginAsync(request, storeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(Errors.Common.NotFound, result.Errors);
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIncorrect_ReturnsFailureResult()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var user = CreateValidUser();
            var request = CreateValidLoginRequest(user.Email, "WrongPassword");

            SetupValidStore(storeId, store);
            SetupValidUser(user.Email, user);
            SetupPasswordCheck(user, request.Password, false);

            // Act
            var result = await _authService.LoginAsync(request, storeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(Errors.AuthError.WrongPassword, result.Errors);
        }

        [Fact]
        public async Task LoginAsync_WhenEmailNotConfirmed_SendsConfirmationEmailAndReturnsFailure()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var user = CreateValidUser(emailConfirmed: false);
            var request = CreateValidLoginRequest(user.Email);
            var confirmationToken = "confirmation_token";

            SetupValidStore(storeId, store);
            SetupValidUser(user.Email, user);
            SetupPasswordCheck(user, request.Password, true);
            SetupEmailConfirmationToken(user, confirmationToken);
            SetupEmailService(true);

            // Act
            var result = await _authService.LoginAsync(request, storeId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(Errors.AuthError.NotVerifyAccount, result.Errors);
            _emailServiceMock.Verify(x => x.SendEmailConfirmationAsync(user.Email, confirmationToken), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenUserNotInStore_CreatesUserStoreAndReturnsSuccess()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var user = CreateValidUser();
            var request = CreateValidLoginRequest(user.Email);
            var userStore = CreateValidUserStore(Guid.Parse(user.Id), storeId);
            var userStoreDto = CreateValidUserStoreDTO(Guid.Parse(user.Id), storeId);

            SetupValidStore(storeId, store);
            SetupValidUser(user.Email, user);
            SetupPasswordCheck(user, request.Password, true);
            SetupUserStoreQuery(new List<UserStore>()); // No existing user stores
            SetupMapperForUserStore(It.IsAny<UserStoreDTO>(), userStore);
            SetupRepositoryAdd(_userStoreRepositoryMock);
            SetupTokenGeneration();
            SetupConfiguration();
            SetupUserRoles(user, Roles.User);
            SetupMapperForUserRefreshToken(It.IsAny<UserRefreshTokenDTO>(), CreateValidUserRefreshToken(user.Id));
            SetupRepositoryAdd(_userRefreshTokenRepositoryMock);

            // Act
            var result = await _authService.LoginAsync(request, storeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            _userStoreRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserStore>()), Times.Once);
            _userStoreRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenUserStoreCreationFails_ContinuesWithLogin()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var user = CreateValidUser();
            var request = CreateValidLoginRequest(user.Email);
            var userStore = CreateValidUserStore(Guid.Parse(user.Id), storeId);

            SetupValidStore(storeId, store);
            SetupValidUser(user.Email, user);
            SetupPasswordCheck(user, request.Password, true);
            SetupUserStoreQuery(new List<UserStore>()); // No existing user stores
            SetupMapperForUserStore(It.IsAny<UserStoreDTO>(), userStore);

            // Setup repository to throw exception
            _userStoreRepositoryMock.Setup(x => x.AddAsync(It.IsAny<UserStore>()))
                .ThrowsAsync(new Exception("Database error"));

            SetupTokenGeneration();
            SetupConfiguration();
            SetupUserRoles(user, Roles.User);
            SetupMapperForUserRefreshToken(It.IsAny<UserRefreshTokenDTO>(), CreateValidUserRefreshToken(user.Id));
            SetupRepositoryAdd(_userRefreshTokenRepositoryMock);

            // Act
            var result = await _authService.LoginAsync(request, storeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
        }

        [Fact]
        public async Task LoginAsync_WhenUserAlreadyInStore_SkipsUserStoreCreation()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var user = CreateValidUser();
            var request = CreateValidLoginRequest(user.Email);
            var existingUserStore = CreateValidUserStore(Guid.Parse(user.Id), storeId);

            SetupValidStore(storeId, store);
            SetupValidUser(user.Email, user);
            SetupPasswordCheck(user, request.Password, true);
            SetupUserStoreQuery(new List<UserStore> { existingUserStore }); // User already in store
            SetupTokenGeneration();
            SetupConfiguration();
            SetupUserRoles(user, Roles.User);
            SetupMapperForUserRefreshToken(It.IsAny<UserRefreshTokenDTO>(), CreateValidUserRefreshToken(user.Id));
            SetupRepositoryAdd(_userRefreshTokenRepositoryMock);

            // Act
            var result = await _authService.LoginAsync(request, storeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            _userStoreRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserStore>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenUserHasMultipleStores_ChecksCorrectStore()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var otherStoreId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var user = CreateValidUser();
            var request = CreateValidLoginRequest(user.Email);
            var userStoreOtherStore = CreateValidUserStore(Guid.Parse(user.Id), otherStoreId);
            var userRoles = new List<string> { Roles.User };

            SetupValidStore(storeId, store);
            SetupValidUser(user.Email, user);
            SetupPasswordCheck(user, request.Password, true);
            SetupUserStoreQuery(new List<UserStore> { userStoreOtherStore }); // User in different store
            SetupMapperForUserStore(It.IsAny<UserStoreDTO>(), CreateValidUserStore(Guid.Parse(user.Id), storeId));
            SetupRepositoryAdd(_userStoreRepositoryMock);
            SetupTokenGeneration();
            SetupConfiguration();
            SetupUserRoles(user, Roles.User);
            SetupMapperForUserRefreshToken(It.IsAny<UserRefreshTokenDTO>(), CreateValidUserRefreshToken(user.Id));
            SetupRepositoryAdd(_userRefreshTokenRepositoryMock);

            // Act
            var result = await _authService.LoginAsync(request, storeId);

            // Assert
            Assert.True(result.IsSuccess);
            _userStoreRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserStore>()), Times.Once); // Should create new user store
        }

        [Fact]
        public async Task LoginAsync_WhenSuccessful_GeneratesValidAuthResult()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var user = CreateValidUser();
            var request = CreateValidLoginRequest(user.Email);
            var existingUserStore = CreateValidUserStore(Guid.Parse(user.Id), storeId);
            var accessToken = "test_access_token";
            var refreshToken = "test_refresh_token";

            SetupValidStore(storeId, store);
            SetupValidUser(user.Email, user);
            SetupPasswordCheck(user, request.Password, true);
            SetupUserStoreQuery(new List<UserStore> { existingUserStore });
            SetupTokenGeneration(accessToken, refreshToken);
            SetupConfiguration();
            SetupUserRoles(user, Roles.User);
            SetupMapperForUserRefreshToken(It.IsAny<UserRefreshTokenDTO>(), CreateValidUserRefreshToken(user.Id));
            SetupRepositoryAdd(_userRefreshTokenRepositoryMock);

            // Act
            var result = await _authService.LoginAsync(request, storeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(accessToken, result.AccessToken);
            Assert.Equal(refreshToken, result.RefreshToken);
            Assert.Null(result.Errors);

            // Verify token generation was called with correct claims
            _tokenServiceMock.Verify(x => x.GenerateAccessToken(It.Is<List<Claim>>(claims =>
                claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id) &&
                claims.Any(c => c.Type == ClaimTypes.Email && c.Value == user.Email) &&
                claims.Any(c => c.Type == "StoreId" && c.Value == storeId.ToString()) &&
                claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "User")
            )), Times.Once);

            // Verify refresh token was saved
            _userRefreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserRefreshToken>()), Times.Once);
            _userRefreshTokenRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenUserHasNoRoles_StillGeneratesToken()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var user = CreateValidUser();
            var request = CreateValidLoginRequest(user.Email);
            var existingUserStore = CreateValidUserStore(Guid.Parse(user.Id), storeId);

            SetupValidStore(storeId, store);
            SetupValidUser(user.Email, user);
            SetupPasswordCheck(user, request.Password, true);
            SetupUserStoreQuery(new List<UserStore> { existingUserStore });
            SetupTokenGeneration();
            SetupConfiguration();
            SetupUserRoles(user, ""); 
            SetupMapperForUserRefreshToken(It.IsAny<UserRefreshTokenDTO>(), CreateValidUserRefreshToken(user.Id));
            SetupRepositoryAdd(_userRefreshTokenRepositoryMock);

            // Act
            var result = await _authService.LoginAsync(request, storeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);

            // Verify token generation was called with basic claims only
            _tokenServiceMock.Verify(x => x.GenerateAccessToken(It.Is<List<Claim>>(claims =>
                claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id) &&
                claims.Any(c => c.Type == ClaimTypes.Email && c.Value == user.Email) &&
                claims.Any(c => c.Type == "StoreId" && c.Value == storeId.ToString())
            )), Times.Once);
        }
    }
}