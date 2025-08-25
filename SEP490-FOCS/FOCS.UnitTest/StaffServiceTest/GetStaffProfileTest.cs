using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.StaffServiceTest
{
    public class GetStaffProfileTest : StaffServiceTestBase
    {
        [Fact]
        public async Task GetStaffProfileAsync_WithValidStaffAndManager_ShouldReturnStaffProfile()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var staff = CreateTestUser(staffId);
            var manager = CreateTestUser(managerId);

            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);
            var managerUserStore = CreateTestUserStore(Guid.Parse(managerId), storeId);

            var expectedRoles = new List<string> { Roles.Staff };
            var expectedProfile = CreateTestStaffProfileDTO();

            // Setup user retrieval
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Setup role checks
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(expectedRoles);

            // Setup store authorization
            var staffStoreQueryable = new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(staffStoreQueryable.Object)
                .Returns(managerStoreQueryable.Object);

            // Setup mapper
            _mockMapper.Setup(x => x.Map<StaffProfileDTO>(staff))
                .Returns(expectedProfile);

            // Act
            var result = await _staffService.GetStaffProfileAsync(staffId, managerId);

            // Assert
            Assert.NotNull(result);
            //Assert.Equal(expectedProfile.Email, result.Email);
            Assert.Equal(expectedRoles, result.Roles);

            // Verify all interactions
            _mockUserManager.Verify(x => x.FindByIdAsync(staffId), Times.Once);
            _mockUserManager.Verify(x => x.FindByIdAsync(managerId), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(staff), Times.Exactly(2)); // Once for validation, once for result
            _mockMapper.Verify(x => x.Map<StaffProfileDTO>(staff), Times.Once);
        }

        [Fact]
        public async Task GetStaffProfileAsync_WithNonExistentStaff_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.GetStaffProfileAsync(staffId, managerId));

            // Verify no further processing occurred
            _mockUserManager.Verify(x => x.FindByIdAsync(managerId), Times.Never);
            _mockMapper.Verify(x => x.Map<StaffProfileDTO>(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetStaffProfileAsync_WithNonExistentManager_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();

            var staff = CreateTestUser(staffId);

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.GetStaffProfileAsync(staffId, managerId));

            // Verify staff was found but processing stopped at manager validation
            _mockUserManager.Verify(x => x.FindByIdAsync(staffId), Times.Once);
            _mockUserManager.Verify(x => x.FindByIdAsync(managerId), Times.Once);
            _mockMapper.Verify(x => x.Map<StaffProfileDTO>(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetStaffProfileAsync_WithStaffHavingUserRole_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();

            var staff = CreateTestUser(staffId);
            var manager = CreateTestUser(managerId);

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Staff has User role (not allowed)
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.User });

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.GetStaffProfileAsync(staffId, managerId));

            // Verify role check was performed
            _mockUserManager.Verify(x => x.GetRolesAsync(staff), Times.Once);
            _mockMapper.Verify(x => x.Map<StaffProfileDTO>(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetStaffProfileAsync_WithUnauthorizedStoreAccess_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var staffStoreId = Guid.NewGuid();
            var managerStoreId = Guid.NewGuid(); // Different store

            var staff = CreateTestUser(staffId);
            var manager = CreateTestUser(managerId);

            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), staffStoreId);
            var managerUserStore = CreateTestUserStore(Guid.Parse(managerId), managerStoreId);

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.Staff });

            // Setup store authorization with different stores
            var staffStoreQueryable = new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(staffStoreQueryable.Object)
                .Returns(managerStoreQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.GetStaffProfileAsync(staffId, managerId));

            // Verify store authorization was checked
            _mockUserStoreRepository.Verify(x => x.AsQueryable(), Times.Exactly(2));
            _mockMapper.Verify(x => x.Map<StaffProfileDTO>(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetStaffProfileAsync_WithStaffHavingNoStoreAccess_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var staff = CreateTestUser(staffId);
            var manager = CreateTestUser(managerId);
            var managerUserStore = CreateTestUserStore(Guid.Parse(managerId), storeId);

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.Staff });

            // Setup staff with no store access
            var emptyStaffStoreQueryable = new List<UserStore>().AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(emptyStaffStoreQueryable.Object)
                .Returns(managerStoreQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.GetStaffProfileAsync(staffId, managerId));

            // Verify store authorization was checked
            _mockUserStoreRepository.Verify(x => x.AsQueryable(), Times.Exactly(2));
            _mockMapper.Verify(x => x.Map<StaffProfileDTO>(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetStaffProfileAsync_WithKitchenStaffRole_ShouldSucceed()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var staff = CreateTestUser(staffId);
            var manager = CreateTestUser(managerId);

            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);
            var managerUserStore = CreateTestUserStore(Guid.Parse(managerId), storeId);

            var expectedRoles = new List<string> { Roles.KitchenStaff };
            var expectedProfile = CreateTestStaffProfileDTO();

            // Setup user retrieval
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Setup role checks - staff is kitchen staff, not regular staff
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(false);
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.KitchenStaff))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(expectedRoles);

            // Setup store authorization
            var staffStoreQueryable = new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(staffStoreQueryable.Object)
                .Returns(managerStoreQueryable.Object);

            // Setup mapper
            _mockMapper.Setup(x => x.Map<StaffProfileDTO>(staff))
                .Returns(expectedProfile);

            // Act
            var result = await _staffService.GetStaffProfileAsync(staffId, managerId);

            // Assert
            Assert.NotNull(result);
            //Assert.Equal(expectedProfile.Email, result.Email);
            Assert.Equal(expectedRoles, result.Roles);
        }
    }
}
