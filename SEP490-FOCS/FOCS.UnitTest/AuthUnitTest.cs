using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services;
using FOCS.Common.Constants;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using static QRCoder.PayloadGenerator;
using System.ComponentModel.DataAnnotations;

namespace FOCS.UnitTest
{
    public class AuthUnitTest
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IRepository<UserRefreshToken>> _mockUserRefreshTokenRepository;
        private readonly Mock<IRepository<Store>> _mockStoreRepository;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly Mock<IRepository<UserStore>> _mockUserStoreRepository;
        private readonly Mock<IRepository<MobileTokenDevice>> _mockMobileTokenDevice;
        private readonly AuthService _authService;

        public AuthUnitTest()
        {
            // Setup UserManager mock
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);

            // Setup SignInManager mock
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                null, null, null, null);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockMapper = new Mock<IMapper>();
            _mockEmailService = new Mock<IEmailService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockUserRefreshTokenRepository = new Mock<IRepository<UserRefreshToken>>();
            _mockStoreRepository = new Mock<IRepository<Store>>();
            _mockLogger = new Mock<ILogger<AuthService>>();
            _mockUserStoreRepository = new Mock<IRepository<UserStore>>();
            _mockMobileTokenDevice = new Mock<IRepository<MobileTokenDevice>>();

            // Setup configuration
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("your-secret-key-here-must-be-long-enough-for-jwt-signing");

            _authService = new AuthService(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockConfiguration.Object,
                _mockMapper.Object,
                _mockEmailService.Object,
                _mockTokenService.Object,
                _mockUserRefreshTokenRepository.Object,
                _mockStoreRepository.Object,
                _mockLogger.Object,
                _mockUserStoreRepository.Object,
                _mockMobileTokenDevice.Object);
        }

        #region Login Tests - CM-01

        [Theory]
        [InlineData("phucemail", "Phuc1@")]
        [InlineData(null, "Phuc1@")]
        [InlineData("phuc@user.com", "p")]
        [InlineData("phuc@user.com", null)]
        public async Task LoginAsync_WithInvalidInput_ShouldValidateFalse(
            string email,
            string password)
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            // Act
            var validationContext = new ValidationContext(loginRequest);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(loginRequest, validationContext, validationResults, true);

            //Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("phuc@user.com", "Phuc1@", "4fa85f64-5717-4562-b3fc-2c963f66afa7", true, true, true)]
        [InlineData("phuc@user.com", "Phuc1@", "3fa85f64-5717-4562-b3fc", true, true, true)]
        [InlineData("phuc@user.com", "Phuc1@", "3fa85f64-5717-4562-b3fc-2c963f66afa6", false, true, true)]
        [InlineData("phuc@user.com", "Phuc1@", "3fa85f64-5717-4562-b3fc-2c963f66afa6", true, false, true)]
        [InlineData("phuc@user.com", "Phuc1@", "3fa85f64-5717-4562-b3fc-2c963f66afa6", true, true, false)]
        public async Task LoginAsync_WithInvalidInput_ShouldReturnFalse(
            string email,
            string password,
            string storeId,
            bool userExists,
            bool passwordCorrect,
            bool emailConfirmed)
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            User user = null;
            if (userExists)
            {
                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = email,
                    EmailConfirmed = emailConfirmed
                };
            }

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            if (user != null)
            {
                _mockUserManager.Setup(x => x.CheckPasswordAsync(user, password))
                    .ReturnsAsync(passwordCorrect);

                if (passwordCorrect && emailConfirmed)
                {
                    _mockUserManager.Setup(x => x.GetRolesAsync(user))
                        .ReturnsAsync(new List<string> { Roles.User });
                    _mockTokenService.Setup(x => x.GenerateAccessToken(It.IsAny<IEnumerable<System.Security.Claims.Claim>>()))
                        .Returns("access-token");
                    _mockTokenService.Setup(x => x.GenerateRefreshToken())
                        .Returns("refresh-token");
                }

                if (!emailConfirmed)
                {
                    _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                        .ReturnsAsync("email-token");
                    _mockEmailService.Setup(x => x.SendEmailConfirmationAsync(user.Email, "email-token"))
                        .ReturnsAsync(true);
                }
            }
            var validStoreId = true;

            // Setup store if storeId is provided and valid
            if (Guid.TryParse(storeId, out var parsedStoreId))
            {
                var store = storeId == "4fa85f64-5717-4562-b3fc-2c963f66afa7" ? null : new Store { Id = parsedStoreId };
                _mockStoreRepository.Setup(x => x.GetByIdAsync(parsedStoreId))
                    .ReturnsAsync(store);

                _mockUserStoreRepository.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserStore, bool>>>()))
                    .ReturnsAsync(new List<UserStore>());
                _mockUserManager.Setup(x => x.IsInRoleAsync(user, Roles.User))
                    .ReturnsAsync(true);
            }
            else
            {
                validStoreId = false;
            }

            // Act & Assert
            if (validStoreId)
            {
                var result = await _authService.LoginAsync(loginRequest, storeId);

                Assert.False(result.IsSuccess);
                Assert.Null(result.AccessToken);
                Assert.Null(result.RefreshToken);
            }
            else
            {
                await Assert.ThrowsAsync<FormatException>(() => _authService.LoginAsync(loginRequest, storeId));
            }
        }

        [Theory]
        [InlineData("phuc@user.com", "Phuc1@", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("phuc@user.com", "Phuc1@", null)]
        public async Task LoginAsync_WithValidInput_ShouldReturnSuccessAuthResult(
            string email,
            string password,
            string storeId)
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            User user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                EmailConfirmed = true
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.User });
            _mockTokenService.Setup(x => x.GenerateAccessToken(It.IsAny<IEnumerable<System.Security.Claims.Claim>>()))
                .Returns("access-token");
            _mockTokenService.Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            // Setup store if storeId is provided and valid
            if (!string.IsNullOrEmpty(storeId) && Guid.TryParse(storeId, out var parsedStoreId))
            {
                _mockStoreRepository.Setup(x => x.GetByIdAsync(parsedStoreId))
                    .ReturnsAsync(new Store { Id = parsedStoreId });
                _mockUserStoreRepository.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserStore, bool>>>()))
                    .ReturnsAsync(new List<UserStore>());
                _mockUserManager.Setup(x => x.IsInRoleAsync(user, Roles.User))
                    .ReturnsAsync(true);
            }

            // Act
            var result = await _authService.LoginAsync(loginRequest, storeId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
        }

        #endregion

        #region Register Tests - CM-02

        [Theory]
        [InlineData("phucuser", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData(null, "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("phuc@user.com", "p", "p", "0987654321", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("phuc@user.com", null, "phucpassword", "0987654321", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("phuc@user.com", "phucpassword", "p", "0987654321", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("phuc@user.com", "phucpassword", null, "0987654321", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("phuc@user.com", "phucpassword", "phucpassword", "966777888", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task RegisterAsync_WithInvalidInput_ShouldValidateFalse(
            string email, string password, string confirmPassword, string phone,
            string firstName, string lastName, string storeId)
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                Phone = phone,
                FirstName = firstName,
                LastName = lastName
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            //Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("phuc@exist.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("phuc@user.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc", "4fa85f64-5717-4562-b3fc-2c963f66afa7")]
        [InlineData("phuc@user.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc")]
        [InlineData("phuc@user.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc", null)]
        public async Task RegisterAsync_WithInvalidInput_ShouldReturnFalse(
            string email, string password, string confirmPassword, string phone,
            string firstName, string lastName, string storeId)
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                Phone = phone,
                FirstName = firstName,
                LastName = lastName
            };

            var parsedStoreId = Guid.Empty;
            var validStoreId = !string.IsNullOrEmpty(storeId) && Guid.TryParse(storeId, out parsedStoreId);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), password))
                .ReturnsAsync(IdentityResult.Failed());

            // Setup store repository
            Store store = validStoreId && storeId != "4fa85f64-5717-4562-b3fc-2c963f66afa7" ? new Store { Id = parsedStoreId } : null;
            _mockStoreRepository.Setup(x => x.GetByIdAsync(parsedStoreId))
                .ReturnsAsync(store);

            // Act & Assert
            if (!validStoreId && !string.IsNullOrEmpty(storeId))
            {
                await Assert.ThrowsAsync<FormatException>(() => _authService.RegisterAsync(request, Guid.Parse(storeId), Roles.User));
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(request, parsedStoreId, Roles.User));
            }
        }

        [Theory]
        [InlineData("phuc@user.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("phuc@user.com", "phucpassword", "phucpassword", null, "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("phuc@user.com", "phucpassword", "phucpassword", "0987654321", null, "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("phuc@user.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", null, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task RegisterAsync_WithValidInput_ShouldReturnTrue(
            string email, string password, string confirmPassword, string phone,
            string firstName, string lastName, string storeIdStr)
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                Phone = phone,
                FirstName = firstName,
                LastName = lastName
            };

            var storeId = Guid.Parse(storeIdStr);

            // Setup user creation result
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), password))
                .ReturnsAsync(IdentityResult.Success);

            // Setup role assignment
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), Roles.User))
                .ReturnsAsync(IdentityResult.Success);

            // Setup store repository
            Store store = new Store { Id = storeId };
            _mockStoreRepository.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync(store);

            // Setup email confirmation
            _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("email-token");
            _mockEmailService.Setup(x => x.SendEmailConfirmationAsync(email, "email-token"))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterAsync(request, storeId, Roles.User);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region Reset Password Tests - CM-03

        [Theory]
        [InlineData("phucemail", "validToken", "newpassword", "newpassword")]
        [InlineData(null, "validToken", "newpassword", "newpassword")]
        [InlineData("phuc@user.com", null, "newpassword", "newpassword")]
        [InlineData("phuc@user.com", "validToken", "newpassword", "p")]
        [InlineData("phuc@user.com", "validToken", "p", "p")]
        [InlineData("phuc@user.com", "validToken", null, "newpassword")]
        [InlineData("phuc@user.com", "validToken", "newpassword", null)]
        public async Task ResetPasswordAsync_WithInvalidInput_ShouldValidateFalse(string email, string token, string newPassword, string confirmPassword)
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = email,
                Token = token,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            //Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("phuc@wrong.com", "validToken", "newpassword", "newpassword", false, true)]
        [InlineData("phuc@user.com", "invalidToken", "newpassword", "newpassword", true, false)]
        public async Task ResetPasswordAsync_WithInvalidInput_ShouldReturnFalse(
            string email, string token, string newPassword, string confirmPassword,
            bool userExists, bool tokenValid)
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = email,
                Token = token,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            User user = userExists ? new User { Email = email } : (User)null;

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            if (!tokenValid)
            {
                var resetResult = IdentityResult.Failed(new IdentityError { Description = "Reset password failed. The token might be invalid or expired." });

                _mockUserManager.Setup(x => x.ResetPasswordAsync(user, token, newPassword))
                    .ReturnsAsync(resetResult);
            }

            // Act & Assert
            if (userExists)
            {
                var result = await _authService.ResetPasswordAsync(request);
                Assert.False(result);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(() => _authService.ResetPasswordAsync(request));
            }
        }

        [Theory]
        [InlineData("phuc@user.com", "validToken", "newpassword", "newpassword")]
        public async Task ResetPasswordAsync_VariousScenarios_ReturnsExpectedResult(string email, string token, string newPassword, string confirmPassword)
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = email,
                Token = token,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            User user = new User { Email = email };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.ResetPasswordAsync(user, token, newPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result);
        }


        #endregion

        #region Change Password Tests - CM-04

        [Theory]
        [InlineData("p", "newpassword", "newpassword")]
        [InlineData(null, "newpassword", "newpassword")]
        [InlineData("phucpassword", "p", "p")]
        [InlineData("phucpassword", null, "newpassword")]
        [InlineData("phucpassword", "newpassword", "phucpassword")]
        [InlineData("phucpassword", "newpassword", null)]
        public async Task ChangePasswordAsync_WithInvalidInput_ShouldValidateFalse(string oldPassword, string newPassword, string confirmPassword)
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            //Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("phucpassword", "newpassword", "newpassword", "test@user.com")]
        public async Task ChangePasswordAsync_WithValidInout_ShouldReturnTrue(
            string oldPassword,
            string newPassword,
            string confirmPassword,
            string email)
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            User user = new User { Email = email };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(user, oldPassword, newPassword))
                .ReturnsAsync(IdentityResult.Success);
            _mockSignInManager.Setup(x => x.RefreshSignInAsync(user))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.ChangePassword(request, email);

            // Assert
            Assert.True(result);
            _mockSignInManager.Verify(x => x.RefreshSignInAsync(user), Times.Once);
        }

        [Theory]
        [InlineData("phucpassword", "newpassword", "newpassword", "test@user.com", false)]
        [InlineData("phucpassword", "phucpassword", "phucpassword", "test@user.com", true)]
        public async Task ChangePasswordAsync_WithInvalidInput_ShouldReturnFalse(
            string oldPassword,
            string newPassword,
            string confirmPassword,
            string email,
            bool oldPasswordCorrect)
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            User user = new User { Email = email };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            if (!oldPasswordCorrect)
            {
                var changeResult = IdentityResult.Failed(new IdentityError { Description = "Incorrect password." });

                _mockUserManager.Setup(x => x.ChangePasswordAsync(user, oldPassword, newPassword))
                    .ReturnsAsync(changeResult);
            }

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authService.ChangePassword(request, email));
        }

        #endregion
    }
}
