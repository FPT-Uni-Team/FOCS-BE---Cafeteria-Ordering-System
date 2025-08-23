using FOCS.Common.Constants;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.StaffServiceTest
{
    public class RemoveStaffRoleTest : StaffServiceTestBase
    {
        [Fact]
        public async Task RemoveStaffRoleAsync_WithValidStaffRole_ShouldReturnTrue()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var role = "Staff";
            var storeId = Guid.NewGuid();

            var staff = CreateTestUser();
            staff.Id = staffId;
            var manager = CreateTestUser("manager@example.com");
            manager.Id = managerId;

            // Setup staff validation
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);

            // Setup manager validation
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Setup staff roles (not a regular User)
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.Staff });

            // Setup store authorization validation
            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);
            var managerUserStores = new List<UserStore>
            {
                CreateTestUserStore(Guid.Parse(managerId), storeId)
            };

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet().Object)
                .Returns(managerUserStores.AsQueryable().BuildMockDbSet().Object);

            // Setup role removal
            _mockUserManager.Setup(x => x.RemoveFromRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _staffService.RemoveStaffRoleAsync(role, staffId, managerId);

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(x => x.FindByIdAsync(staffId), Times.Once);
            _mockUserManager.Verify(x => x.FindByIdAsync(managerId), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(staff), Times.Once);
            _mockUserManager.Verify(x => x.RemoveFromRoleAsync(staff, Roles.Staff), Times.Once);
            _mockUserStoreRepository.Verify(x => x.AsQueryable(), Times.Exactly(2));
        }

        [Fact]
        public async Task RemoveStaffRoleAsync_WithValidKitchenStaffRole_ShouldReturnTrue()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var role = "KitchenStaff";
            var storeId = Guid.NewGuid();

            var staff = CreateTestUser();
            staff.Id = staffId;
            var manager = CreateTestUser("manager@example.com");
            manager.Id = managerId;

            // Setup staff validation
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);

            // Setup manager validation
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Setup staff roles (not a regular User)
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.KitchenStaff });

            // Setup store authorization validation
            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);
            var managerUserStores = new List<UserStore>
            {
                CreateTestUserStore(Guid.Parse(managerId), storeId)
            };

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet().Object)
                .Returns(managerUserStores.AsQueryable().BuildMockDbSet().Object);

            // Setup role removal
            _mockUserManager.Setup(x => x.RemoveFromRoleAsync(staff, Roles.KitchenStaff))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _staffService.RemoveStaffRoleAsync(role, staffId, managerId);

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(x => x.RemoveFromRoleAsync(staff, Roles.KitchenStaff), Times.Once);
        }

        [Fact]
        public async Task RemoveStaffRoleAsync_WithCaseInsensitiveRole_ShouldReturnTrue()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var role = "STAFF"; // uppercase
            var storeId = Guid.NewGuid();

            var staff = CreateTestUser();
            staff.Id = staffId;
            var manager = CreateTestUser("manager@example.com");
            manager.Id = managerId;

            // Setup staff validation
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);

            // Setup manager validation
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Setup staff roles (not a regular User)
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.Staff });

            // Setup store authorization validation
            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);
            var managerUserStores = new List<UserStore>
            {
                CreateTestUserStore(Guid.Parse(managerId), storeId)
            };

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet().Object)
                .Returns(managerUserStores.AsQueryable().BuildMockDbSet().Object);

            // Setup role removal
            _mockUserManager.Setup(x => x.RemoveFromRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _staffService.RemoveStaffRoleAsync(role, staffId, managerId);

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(x => x.RemoveFromRoleAsync(staff, Roles.Staff), Times.Once);
        }

        [Fact]
        public async Task RemoveStaffRoleAsync_WithNonExistentStaff_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var role = "Staff";

            // Setup staff validation (staff not found)
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _staffService.RemoveStaffRoleAsync(role, staffId, managerId));

            _mockUserManager.Verify(x => x.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoveStaffRoleAsync_WithUnauthorizedManager_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var role = "Staff";

            var staff = CreateTestUser();
            staff.Id = staffId;

            // Setup staff validation
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);

            // Setup manager validation (manager not found)
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _staffService.RemoveStaffRoleAsync(role, staffId, managerId));

            _mockUserManager.Verify(x => x.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoveStaffRoleAsync_WithUserRole_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var role = "Staff";

            var staff = CreateTestUser();
            staff.Id = staffId;
            var manager = CreateTestUser("manager@example.com");
            manager.Id = managerId;

            // Setup staff validation
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);

            // Setup manager validation
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Setup staff roles (has User role - should be rejected)
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.User });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _staffService.RemoveStaffRoleAsync(role, staffId, managerId));

            _mockUserManager.Verify(x => x.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoveStaffRoleAsync_WithDifferentStoreAccess_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var role = "Staff";
            var staffStoreId = Guid.NewGuid();
            var managerStoreId = Guid.NewGuid(); // Different store

            var staff = CreateTestUser();
            staff.Id = staffId;
            var manager = CreateTestUser("manager@example.com");
            manager.Id = managerId;

            // Setup staff validation
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);

            // Setup manager validation
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Setup staff roles (not a regular User)
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.Staff });

            // Setup store authorization validation with different stores
            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), staffStoreId);
            var managerUserStores = new List<UserStore>
            {
                CreateTestUserStore(Guid.Parse(managerId), managerStoreId) // Different store
            };

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet().Object)
                .Returns(managerUserStores.AsQueryable().BuildMockDbSet().Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _staffService.RemoveStaffRoleAsync(role, staffId, managerId));

            _mockUserManager.Verify(x => x.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }
    }
}
