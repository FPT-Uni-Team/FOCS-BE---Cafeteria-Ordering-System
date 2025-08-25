using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FOCS.UnitTest.StaffServiceTest
{
    public class CreateStaffTest : StaffServiceTestBase
    {
        [Fact]
        public async Task CreateStaffAsync_WithValidData_ShouldReturnStaffProfileDTO()
        {
            // Arrange
            var request = CreateValidRegisterRequest();
            var storeId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var user = CreateTestUser();
            var expectedStaffProfile = CreateTestStaffProfileDTO();

            SetupValidStoreExists(Guid.Parse(storeId));
            SetupValidManagerStoreAccess(managerId, Guid.Parse(storeId));
            SetupSuccessfulUserCreation(user, request.Password);
            SetupMapperForUserStore();
            SetupAddToRoleSuccess(user, Roles.Staff);
            SetupMapperForStaffProfile(user);

            _mockUserStoreRepository.Setup(x => x.AddAsync(It.IsAny<UserStore>()))
                .Returns(Task.CompletedTask);
            _mockUserStoreRepository.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);
            _mockEmailService.Setup(x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            User capturedUser = null;
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>((u, p) =>
                {
                    capturedUser = u;
                    u.Id = user.Id;
                    SetupEmailConfirmationToken(u, "test-token");
                });

            // Act
            await _staffService.CreateStaffAsync(request, storeId, managerId);

            // Assert
            Assert.NotNull(capturedUser);
            //Assert.Equal(request.Email, capturedUser.Email);
            Assert.Equal(request.FirstName, capturedUser.FirstName);
            Assert.Equal(request.LastName, capturedUser.LastName);
            Assert.Equal(request.Phone, capturedUser.PhoneNumber);
            //Assert.Equal(request.Email.Split("@")[0], capturedUser.UserName);
            Assert.True(capturedUser.IsActive);

            // Verify all interactions
            _mockStoreRepository.Verify(x => x.GetByIdAsync(Guid.Parse(storeId)), Times.Once);
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
            _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), Roles.Staff), Times.Once);
            _mockUserStoreRepository.Verify(x => x.AddAsync(It.IsAny<UserStore>()), Times.Once);
            _mockUserStoreRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
            _mockEmailService.Verify(x => x.SendEmailConfirmationAsync(user.Email, "test-token"), Times.Once);
        }

        [Fact]
        public async Task CreateStaffAsync_WithInvalidStoreId_ShouldThrowException()
        {
            // Arrange
            var request = CreateValidRegisterRequest();
            var invalidStoreId = "invalid-guid";
            var managerId = Guid.NewGuid().ToString();

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.CreateStaffAsync(request, invalidStoreId, managerId));

            // Verify that no other operations were attempted
            _mockStoreRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateStaffAsync_WithNonExistentStore_ShouldThrowException()
        {
            // Arrange
            var request = CreateValidRegisterRequest();
            var storeId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();

            _mockStoreRepository.Setup(x => x.GetByIdAsync(Guid.Parse(storeId)))
                .ReturnsAsync((Store)null);

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.CreateStaffAsync(request, storeId, managerId));

            // Verify store check was made but no user creation attempted
            _mockStoreRepository.Verify(x => x.GetByIdAsync(Guid.Parse(storeId)), Times.Once);
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateStaffAsync_WithUnauthorizedManager_ShouldThrowException()
        {
            // Arrange
            var request = CreateValidRegisterRequest();
            var storeId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var differentStoreId = Guid.NewGuid();

            SetupValidStoreExists(Guid.Parse(storeId));

            // Setup manager with access to different store
            var managerUserStores = new List<UserStore>
            {
                CreateTestUserStore(Guid.Parse(managerId), differentStoreId)
            };
            var mockQueryable = managerUserStores.AsQueryable();
            _mockUserStoreRepository.Setup(x => x.AsQueryable())
                .Returns(mockQueryable);

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.CreateStaffAsync(request, storeId, managerId));

            // Verify store and manager checks were made but no user creation attempted
            _mockStoreRepository.Verify(x => x.GetByIdAsync(Guid.Parse(storeId)), Times.Once);
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateStaffAsync_WithManagerHavingNoStores_ShouldThrowException()
        {
            // Arrange
            var request = CreateValidRegisterRequest();
            var storeId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();

            SetupValidStoreExists(Guid.Parse(storeId));

            // Setup empty manager stores
            var emptyManagerUserStores = new List<UserStore>();
            var mockQueryable = emptyManagerUserStores.AsQueryable();
            _mockUserStoreRepository.Setup(x => x.AsQueryable())
                .Returns(mockQueryable);

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.CreateStaffAsync(request, storeId, managerId));

            // Verify checks were made but no user creation attempted
            _mockStoreRepository.Verify(x => x.GetByIdAsync(Guid.Parse(storeId)), Times.Once);
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateStaffAsync_WithUserCreationFailure_ShouldThrowException()
        {
            // Arrange
            var request = CreateValidRegisterRequest();
            var storeId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();

            SetupValidStoreExists(Guid.Parse(storeId));
            SetupValidManagerStoreAccess(managerId, Guid.Parse(storeId));
            SetupFailedUserCreation(request.Password, "Email already exists", "Password too weak");

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.CreateStaffAsync(request, storeId, managerId));

            // Verify user creation was attempted but failed
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
            _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _mockUserStoreRepository.Verify(x => x.AddAsync(It.IsAny<UserStore>()), Times.Never);
        }

        [Fact]
        public async Task CreateStaffAsync_WithUserStoreRepositoryFailure_ShouldThrowException()
        {
            // Arrange
            var request = CreateValidRegisterRequest();
            var storeId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var user = CreateTestUser();

            SetupValidStoreExists(Guid.Parse(storeId));
            SetupValidManagerStoreAccess(managerId, Guid.Parse(storeId));
            SetupSuccessfulUserCreation(user, request.Password);
            SetupMapperForUserStore();

            _mockUserStoreRepository.Setup(x => x.AddAsync(It.IsAny<UserStore>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _staffService.CreateStaffAsync(request, storeId, managerId));

            Assert.Equal("Database error", exception.Message);

            // Verify user creation succeeded but UserStore addition failed
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
            _mockUserStoreRepository.Verify(x => x.AddAsync(It.IsAny<UserStore>()), Times.Once);
            _mockUserStoreRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateStaffAsync_ShouldCreateUserWithCorrectProperties()
        {
            // Arrange
            var request = CreateValidRegisterRequest();
            var storeId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var user = CreateTestUser();

            SetupValidStoreExists(Guid.Parse(storeId));
            SetupValidManagerStoreAccess(managerId, Guid.Parse(storeId));
            SetupMapperForUserStore();
            SetupEmailConfirmationToken(user, "test-token");
            SetupAddToRoleSuccess(user, Roles.Staff);
            SetupMapperForStaffProfile(user);

            _mockUserStoreRepository.Setup(x => x.AddAsync(It.IsAny<UserStore>()))
                .Returns(Task.CompletedTask);
            _mockUserStoreRepository.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);
            _mockEmailService.Setup(x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            User capturedUser = null;
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>((u, p) =>
                {
                    capturedUser = u;
                    u.Id = user.Id;
                });

            // Act
            await _staffService.CreateStaffAsync(request, storeId, managerId);

            // Assert
            Assert.NotNull(capturedUser);
            //Assert.Equal(request.Email, capturedUser.Email);
            Assert.Equal(request.FirstName, capturedUser.FirstName);
            Assert.Equal(request.LastName, capturedUser.LastName);
            Assert.Equal(request.Phone, capturedUser.PhoneNumber);
            //Assert.Equal(request.Email.Split("@")[0], capturedUser.UserName);
            Assert.True(capturedUser.IsActive);
        }

        [Fact]
        public async Task CreateStaffAsync_ShouldCreateUserStoreWithCorrectProperties()
        {
            // Arrange
            var request = CreateValidRegisterRequest();
            var storeId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var user = CreateTestUser();

            SetupValidStoreExists(Guid.Parse(storeId));
            SetupValidManagerStoreAccess(managerId, Guid.Parse(storeId));
            SetupSuccessfulUserCreation(user, request.Password);
            SetupEmailConfirmationToken(user, "test-token");
            SetupAddToRoleSuccess(user, Roles.Staff);
            SetupMapperForStaffProfile(user);

            _mockUserStoreRepository.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);
            _mockEmailService.Setup(x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            UserStore capturedUserStore = null;
            _mockMapper.Setup(x => x.Map<UserStore>(It.IsAny<UserStoreDTO>()))
                .Returns((UserStoreDTO dto) =>
                {
                    capturedUserStore = new UserStore
                    {
                        Id = dto.Id,
                        UserId = dto.UserId,
                        StoreId = dto.StoreId,
                        JoinDate = dto.JoinDate,
                        Status = dto.Status,
                        BlockReason = dto.BlockReason
                    };
                    return capturedUserStore;
                });

            _mockUserStoreRepository.Setup(x => x.AddAsync(It.IsAny<UserStore>()))
                .Returns(Task.CompletedTask);

            // Act
            await _staffService.CreateStaffAsync(request, storeId, managerId);

            // Assert
            Assert.NotNull(capturedUserStore);
            Assert.Equal(Guid.Parse(user.Id), capturedUserStore.UserId);
            Assert.Equal(Guid.Parse(storeId), capturedUserStore.StoreId);
            Assert.Null(capturedUserStore.BlockReason);
            Assert.Equal(Common.Enums.UserStoreStatus.Active, capturedUserStore.Status);
            Assert.True(capturedUserStore.JoinDate <= DateTime.UtcNow);
            Assert.True(capturedUserStore.JoinDate > DateTime.UtcNow.AddMinutes(-1)); // Recent creation
        }
    }
}
