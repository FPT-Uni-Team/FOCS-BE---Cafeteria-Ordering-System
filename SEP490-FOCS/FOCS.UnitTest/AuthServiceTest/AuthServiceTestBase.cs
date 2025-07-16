using AutoMapper;
using FOCS.Application.Services;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.AuthServiceTest
{
    public class AuthServiceTestBase
    {
        protected readonly Mock<UserManager<User>> _userManagerMock;
        protected readonly Mock<SignInManager<User>> _signInManagerMock;
        protected readonly Mock<IConfiguration> _configurationMock;
        protected readonly Mock<IMapper> _mapperMock;
        protected readonly Mock<IEmailService> _emailServiceMock;
        protected readonly Mock<ITokenService> _tokenServiceMock;
        protected readonly Mock<IRepository<UserRefreshToken>> _userRefreshTokenRepositoryMock;
        protected readonly Mock<IRepository<Store>> _storeRepositoryMock;
        protected readonly Mock<ILogger<AuthService>> _loggerMock;
        protected readonly Mock<IRepository<UserStore>> _userStoreRepositoryMock;
        protected readonly AuthService _authService;

        public AuthServiceTestBase()
        {
            // Mock UserManager
            var userStore = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            _configurationMock = new Mock<IConfiguration>();
            _mapperMock = new Mock<IMapper>();
            _emailServiceMock = new Mock<IEmailService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _userRefreshTokenRepositoryMock = new Mock<IRepository<UserRefreshToken>>();
            _storeRepositoryMock = new Mock<IRepository<Store>>();
            _loggerMock = new Mock<ILogger<AuthService>>();
            _userStoreRepositoryMock = new Mock<IRepository<UserStore>>();

            // Create a simple SignInManager mock or use null since it's not used in LoginAsync
            var signInManagerMock = CreateSignInManagerMock();

            _authService = new AuthService(
                _userManagerMock.Object,
                signInManagerMock,
                _configurationMock.Object,
                _mapperMock.Object,
                _emailServiceMock.Object,
                _tokenServiceMock.Object,
                _userRefreshTokenRepositoryMock.Object,
                _storeRepositoryMock.Object,
                _loggerMock.Object,
                _userStoreRepositoryMock.Object
            );
        }

        private SignInManager<User> CreateSignInManagerMock()
        {
            // Create a minimal SignInManager mock that doesn't cause constructor issues
            var httpContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();

            try
            {
                var signInManager = new Mock<SignInManager<User>>(
                    _userManagerMock.Object,
                    httpContextAccessor.Object,
                    userPrincipalFactory.Object,
                    null, null, null, null
                );
                return signInManager.Object;
            }
            catch
            {
                // If mocking fails, return null since SignInManager is not used in LoginAsync
                return null;
            }
        }

        // Helper methods for creating test data
        protected User CreateValidUser(string email = "test@example.com", bool emailConfirmed = true)
        {
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email,
                EmailConfirmed = emailConfirmed,
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "1234567890"
            };
        }

        protected Store CreateValidStore(Guid? storeId = null)
        {
            return new Store
            {
                Id = storeId ?? Guid.NewGuid(),
                Name = "Test Store",
                Address = "Test Address",
                IsActive = true
            };
        }

        protected UserStore CreateValidUserStore(Guid userId, Guid storeId)
        {
            return new UserStore
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoreId = storeId,
                JoinDate = DateTime.UtcNow,
                Status = FOCS.Common.Enums.UserStoreStatus.Active,
                BlockReason = null
            };
        }

        protected UserStoreDTO CreateValidUserStoreDTO(Guid userId, Guid storeId)
        {
            return new UserStoreDTO
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoreId = storeId,
                JoinDate = DateTime.UtcNow,
                Status = FOCS.Common.Enums.UserStoreStatus.Active,
                BlockReason = null
            };
        }

        protected LoginRequest CreateValidLoginRequest(string email = "test@example.com", string password = "Password123!")
        {
            return new LoginRequest
            {
                Email = email,
                Password = password
            };
        }

        protected RegisterRequest CreateValidRegisterRequest(string email = "test@example.com")
        {
            return new RegisterRequest
            {
                Email = email,
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Phone = "1234567890"
            };
        }

        protected UserRefreshToken CreateValidUserRefreshToken(string userId, string token = "refresh_token")
        {
            return new UserRefreshToken
            {
                UserId = userId,
                Token = token,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
        }

        protected void SetupValidStore(Guid storeId, Store store)
        {
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync(store);
        }

        protected void SetupValidUser(string email, User user)
        {
            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);
        }

        protected void SetupPasswordCheck(User user, string password, bool isValid)
        {
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(isValid);
        }

        protected void SetupEmailConfirmationToken(User user, string token)
        {
            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);
        }

        protected void SetupUserStoreQuery(List<UserStore> userStores)
        {
            _userStoreRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);
        }

        protected void SetupUserRoles(User user, List<string> roles)
        {
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);
        }

        protected void SetupTokenGeneration(string accessToken = "access_token", string refreshToken = "refresh_token")
        {
            _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<List<System.Security.Claims.Claim>>()))
                .Returns(accessToken);
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
                .Returns(refreshToken);
        }

        protected void SetupConfiguration(string jwtKey = "test_jwt_key_that_is_long_enough_for_hmac")
        {
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(x => x.Value).Returns(jwtKey);
            _configurationMock.Setup(x => x["Jwt:Key"]).Returns(jwtKey);
        }

        protected void SetupEmailService(bool sendResult = true)
        {
            _emailServiceMock.Setup(x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(sendResult);
        }

        protected void SetupMapperForUserStore(UserStoreDTO dto, UserStore entity)
        {
            _mapperMock.Setup(x => x.Map<UserStore>(dto))
                .Returns(entity);
        }

        protected void SetupMapperForUserRefreshToken(UserRefreshTokenDTO dto, UserRefreshToken entity)
        {
            _mapperMock.Setup(x => x.Map<UserRefreshToken>(dto))
                .Returns(entity);
        }

        protected void SetupRepositoryAdd<T>(Mock<IRepository<T>> repositoryMock) where T : class
        {
            repositoryMock.Setup(x => x.AddAsync(It.IsAny<T>())).Returns(Task.CompletedTask);
            repositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        }
    }
}