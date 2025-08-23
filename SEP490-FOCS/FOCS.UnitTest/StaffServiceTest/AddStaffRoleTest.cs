using FOCS.Common.Constants;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.StaffServiceTest
{
    public class AddStaffRoleTest : StaffServiceTestBase
    {
        [Fact]
        public async Task AddStaffRoleAsync_WithValidStaffRole_ShouldReturnTrue()
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

            // Setup role assignment
            _mockUserManager.Setup(x => x.AddToRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _staffService.AddStaffRoleAsync(role, staffId, managerId);

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(x => x.FindByIdAsync(staffId), Times.Once);
            _mockUserManager.Verify(x => x.FindByIdAsync(managerId), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(staff), Times.Once);
            _mockUserManager.Verify(x => x.AddToRoleAsync(staff, Roles.Staff), Times.Once);
            _mockUserStoreRepository.Verify(x => x.AsQueryable(), Times.Exactly(2));
        }

        [Fact]
        public async Task AddStaffRoleAsync_WithValidKitchenStaffRole_ShouldReturnTrue()
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

            // Setup role assignment
            _mockUserManager.Setup(x => x.AddToRoleAsync(staff, Roles.KitchenStaff))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _staffService.AddStaffRoleAsync(role, staffId, managerId);

            // Assert
            Assert.True(result);
            _mockUserManager.Verify(x => x.AddToRoleAsync(staff, Roles.KitchenStaff), Times.Once);
        }

        [Fact]
        public async Task AddStaffRoleAsync_WithNonExistentStaff_ShouldThrowException()
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
                () => _staffService.AddStaffRoleAsync(role, staffId, managerId));

            _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddStaffRoleAsync_WithUnauthorizedManager_ShouldThrowException()
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
                () => _staffService.AddStaffRoleAsync(role, staffId, managerId));

            _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddStaffRoleAsync_WithUserRole_ShouldThrowException()
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
                () => _staffService.AddStaffRoleAsync(role, staffId, managerId));

            _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }
    }
}
