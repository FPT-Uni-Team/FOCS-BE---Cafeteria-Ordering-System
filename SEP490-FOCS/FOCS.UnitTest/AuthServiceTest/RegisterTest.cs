using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FOCS.UnitTest.AuthServiceTest
{
    public class RegisterTest : AuthServiceTestBase
    {
        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnsTrue()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var request = CreateValidRegisterRequest();

            SetupValidStore(storeId, store);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("email_confirmation_token");

            SetupRepositoryAdd(_userStoreRepositoryMock);
            SetupEmailService(true);

            // Setup mapper for UserStore
            _mapperMock.Setup(x => x.Map<UserStore>(It.IsAny<UserStoreDTO>()))
                .Returns(new UserStore());

            // Act
            var result = await _authService.RegisterAsync(request, storeId);

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), "User"), Times.Once);
            _userStoreRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserStore>()), Times.Once);
            _userStoreRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _emailServiceMock.Verify(x => x.SendEmailConfirmationAsync(request.Email, "email_confirmation_token"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_StoreNotFound_ThrowsConditionCheckException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var request = CreateValidRegisterRequest();


            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync((Store)null);

            // Act
            await Assert.ThrowsAsync<Exception>(() =>
                _authService.RegisterAsync(request, storeId));

            // Assert
            _storeRepositoryMock.Verify(x => x.GetByIdAsync(storeId), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_UserCreationFails_ThrowsConditionCheckException()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var request = CreateValidRegisterRequest();

            SetupValidStore(storeId, store);

            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" },
                new IdentityError { Code = "WeakPassword", Description = "Password too weak" }
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _authService.RegisterAsync(request, storeId));

            Assert.Contains("Email already exists", exception.Message);
            Assert.Contains("Password too weak", exception.Message);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_UserCreationSucceeds_CreatesUserWithCorrectProperties()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var request = CreateValidRegisterRequest("test@example.com");

            SetupValidStore(storeId, store);

            User capturedUser = null;
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .Callback<User, string>((user, password) => capturedUser = user)
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("email_confirmation_token");

            SetupRepositoryAdd(_userStoreRepositoryMock);
            SetupEmailService(true);

            _mapperMock.Setup(x => x.Map<UserStore>(It.IsAny<UserStoreDTO>()))
                .Returns(new UserStore());

            // Act
            await _authService.RegisterAsync(request, storeId);

            // Assert
            Assert.NotNull(capturedUser);
            Assert.Equal(request.Email, capturedUser.Email);
            Assert.Equal(request.FirstName, capturedUser.FirstName);
            Assert.Equal(request.LastName, capturedUser.LastName);
            Assert.Equal(request.Phone, capturedUser.PhoneNumber);
        }

        [Fact]
        public async Task RegisterAsync_Success_CreatesUserStoreWithCorrectProperties()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var request = CreateValidRegisterRequest();

            SetupValidStore(storeId, store);

            var createdUser = new User { Id = Guid.NewGuid().ToString() };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("email_confirmation_token");

            SetupRepositoryAdd(_userStoreRepositoryMock);
            SetupEmailService(true);

            UserStoreDTO capturedUserStoreDTO = null;
            _mapperMock.Setup(x => x.Map<UserStore>(It.IsAny<object>()))
                .Callback<object>(obj => capturedUserStoreDTO = obj as UserStoreDTO)
                .Returns(new UserStore());

            // Act
            await _authService.RegisterAsync(request, storeId);

            // Assert
            Assert.NotNull(capturedUserStoreDTO);
            Assert.NotEqual(Guid.Empty, capturedUserStoreDTO.Id);
            Assert.Equal(storeId, capturedUserStoreDTO.StoreId);
            Assert.Null(capturedUserStoreDTO.BlockReason);
            Assert.Equal(Common.Enums.UserStoreStatus.Active, capturedUserStoreDTO.Status);
            Assert.True(capturedUserStoreDTO.JoinDate <= DateTime.UtcNow);
            Assert.True(capturedUserStoreDTO.JoinDate >= DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task RegisterAsync_Success_AddsUserToUserRole()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var request = CreateValidRegisterRequest();

            SetupValidStore(storeId, store);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("email_confirmation_token");

            SetupRepositoryAdd(_userStoreRepositoryMock);
            SetupEmailService(true);

            _mapperMock.Setup(x => x.Map<UserStore>(It.IsAny<UserStoreDTO>()))
                .Returns(new UserStore());

            // Act
            await _authService.RegisterAsync(request, storeId);

            // Assert
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), "User"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_Success_GeneratesAndSendsEmailConfirmationToken()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var request = CreateValidRegisterRequest("test@example.com");

            SetupValidStore(storeId, store);

            var expectedToken = "generated_email_token";

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync(expectedToken);

            SetupRepositoryAdd(_userStoreRepositoryMock);
            SetupEmailService(true);

            _mapperMock.Setup(x => x.Map<UserStore>(It.IsAny<UserStoreDTO>()))
                .Returns(new UserStore());

            // Act
            await _authService.RegisterAsync(request, storeId);

            // Assert
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()), Times.Once);
            _emailServiceMock.Verify(x => x.SendEmailConfirmationAsync(request.Email, expectedToken), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_Success_SavesUserStoreToRepository()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var request = CreateValidRegisterRequest();

            SetupValidStore(storeId, store);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("email_confirmation_token");

            SetupRepositoryAdd(_userStoreRepositoryMock);
            SetupEmailService(true);

            var mappedUserStore = new UserStore();
            _mapperMock.Setup(x => x.Map<UserStore>(It.IsAny<UserStoreDTO>()))
                .Returns(mappedUserStore);

            // Act
            await _authService.RegisterAsync(request, storeId);

            // Assert
            _userStoreRepositoryMock.Verify(x => x.AddAsync(mappedUserStore), Times.Once);
            _userStoreRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithDifferentStoreId_UsesCorrectStoreId()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var request = CreateValidRegisterRequest();

            SetupValidStore(storeId, store);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("email_confirmation_token");

            SetupRepositoryAdd(_userStoreRepositoryMock);
            SetupEmailService(true);

            UserStoreDTO capturedUserStoreDTO = null;
            _mapperMock.Setup(x => x.Map<UserStore>(It.IsAny<object>()))
                .Callback<object>(obj => capturedUserStoreDTO = obj as UserStoreDTO)
                .Returns(new UserStore());

            // Act
            await _authService.RegisterAsync(request, storeId);

            // Assert
            Assert.Equal(storeId, capturedUserStoreDTO.StoreId);
        }

        [Fact]
        public async Task RegisterAsync_WithDifferentEmail_CreatesUserWithCorrectEmail()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var customEmail = "custom@test.com";
            var request = CreateValidRegisterRequest(customEmail);

            SetupValidStore(storeId, store);

            User capturedUser = null;
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .Callback<User, string>((user, password) => capturedUser = user)
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("email_confirmation_token");

            SetupRepositoryAdd(_userStoreRepositoryMock);
            SetupEmailService(true);

            _mapperMock.Setup(x => x.Map<UserStore>(It.IsAny<UserStoreDTO>()))
                .Returns(new UserStore());

            // Act
            await _authService.RegisterAsync(request, storeId);

            // Assert
            Assert.Equal(customEmail, capturedUser.Email);
            _emailServiceMock.Verify(x => x.SendEmailConfirmationAsync(customEmail, "email_confirmation_token"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_MultipleIdentityErrors_ThrowsConditionCheckExceptionWithAllErrors()
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var store = CreateValidStore(storeId);
            var request = CreateValidRegisterRequest();

            SetupValidStore(storeId, store);

            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Code = "DuplicateEmail", Description = "Email 'test@example.com' is already taken." },
                new IdentityError { Code = "PasswordTooShort", Description = "Passwords must be at least 6 characters." },
                new IdentityError { Code = "PasswordRequiresDigit", Description = "Passwords must have at least one digit ('0'-'9')." }
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _authService.RegisterAsync(request, storeId));

            Assert.Contains("Email 'test@example.com' is already taken.", exception.Message);
            Assert.Contains("Passwords must be at least 6 characters.", exception.Message);
            Assert.Contains("Passwords must have at least one digit ('0'-'9').", exception.Message);
        }
    }
}