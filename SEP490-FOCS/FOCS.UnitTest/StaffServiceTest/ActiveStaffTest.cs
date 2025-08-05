using FOCS.Common.Constants;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.StaffServiceTest
{
    public class ActiveStaffTest : StaffServiceTestBase
    {
        [Fact]
        public async Task ActiveStaffAsync_WithValidStaffAndManager_ShouldActivateAndUpdateProperties()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var staff = CreateTestUser(staffId);
            staff.IsActive = false; // Initially inactive
            var manager = CreateTestUser(managerId);

            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);
            var managerUserStore = CreateTestUserStore(Guid.Parse(managerId), storeId);

            // Setup complete ValidatePermissionAsync flow
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Staff role validation (checkStaff = true)
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.KitchenStaff))
                .ReturnsAsync(false);

            // Manager role validation (checkAdmin = false by default)
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.Staff }); // Staff doesn't have User role

            // Setup store authorization
            var staffStoreQueryable = new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(staffStoreQueryable.Object)  // For staff store lookup
                .Returns(managerStoreQueryable.Object); // For manager store lookup

            // Setup UserManager update
            _mockUserManager.Setup(x => x.UpdateAsync(staff))
                .ReturnsAsync(IdentityResult.Success);

            var beforeTime = DateTime.UtcNow;

            // Act
            var result = await _staffService.ActiveStaffAsync(staffId, managerId);

            var afterTime = DateTime.UtcNow;

            // Assert
            Assert.True(result);

            // Verify staff properties were updated correctly
            Assert.True(staff.IsActive);
            Assert.Equal(staffId, staff.UpdatedBy); // UpdatedBy should be staffId, not managerId
            Assert.True(staff.UpdatedAt >= beforeTime && staff.UpdatedAt <= afterTime);

            // Verify all validation steps were executed
            _mockUserManager.Verify(x => x.FindByIdAsync(staffId), Times.Once);
            _mockUserManager.Verify(x => x.FindByIdAsync(managerId), Times.Once);
            _mockUserManager.Verify(x => x.IsInRoleAsync(staff, Roles.Staff), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(staff), Times.Once);
            _mockUserStoreRepository.Verify(x => x.AsQueryable(), Times.Exactly(2));
            _mockUserManager.Verify(x => x.UpdateAsync(staff), Times.Once);
        }

        [Fact]
        public async Task ActiveStaffAsync_WithKitchenStaffRole_ShouldPassValidationAndActivate()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var kitchenStaff = CreateTestUser(staffId);
            kitchenStaff.IsActive = false;
            var manager = CreateTestUser(managerId);

            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);
            var managerUserStore = CreateTestUserStore(Guid.Parse(managerId), storeId);

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(kitchenStaff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Kitchen staff role validation (fails Staff but passes KitchenStaff)
            _mockUserManager.Setup(x => x.IsInRoleAsync(kitchenStaff, Roles.Staff))
                .ReturnsAsync(false);
            _mockUserManager.Setup(x => x.IsInRoleAsync(kitchenStaff, Roles.KitchenStaff))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(kitchenStaff))
                .ReturnsAsync(new List<string> { Roles.KitchenStaff });

            var staffStoreQueryable = new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(staffStoreQueryable.Object)
                .Returns(managerStoreQueryable.Object);

            _mockUserManager.Setup(x => x.UpdateAsync(kitchenStaff))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _staffService.ActiveStaffAsync(staffId, managerId);

            // Assert
            Assert.True(result);
            Assert.True(kitchenStaff.IsActive);

            // Verify both role checks were performed
            _mockUserManager.Verify(x => x.IsInRoleAsync(kitchenStaff, Roles.Staff), Times.Once);
            _mockUserManager.Verify(x => x.IsInRoleAsync(kitchenStaff, Roles.KitchenStaff), Times.Once);
        }

        [Fact]
        public async Task ActiveStaffAsync_WithStaffNotInStaffOrKitchenStaffRole_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();

            var regularUser = CreateTestUser(staffId);
            var manager = CreateTestUser(managerId);

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(regularUser);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // User is neither Staff nor KitchenStaff
            _mockUserManager.Setup(x => x.IsInRoleAsync(regularUser, Roles.Staff))
                .ReturnsAsync(false);
            _mockUserManager.Setup(x => x.IsInRoleAsync(regularUser, Roles.KitchenStaff))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.ActiveStaffAsync(staffId, managerId));

            // Verify both role checks were performed but validation failed
            _mockUserManager.Verify(x => x.IsInRoleAsync(regularUser, Roles.Staff), Times.Once);
            _mockUserManager.Verify(x => x.IsInRoleAsync(regularUser, Roles.KitchenStaff), Times.Once);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ActiveStaffAsync_WithStaffHavingUserRole_ShouldThrowException()
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

            // Staff passes initial role check but has User role
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.KitchenStaff))
                .ReturnsAsync(false);

            // Staff has User role (not allowed)
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.Staff, Roles.User });

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.ActiveStaffAsync(staffId, managerId));

            // Verify role validation was performed
            _mockUserManager.Verify(x => x.GetRolesAsync(staff), Times.Once);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ActiveStaffAsync_WithStaffHavingNoStoreAccess_ShouldThrowException()
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

            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.Staff });

            // Staff has no store access, manager has store access
            var emptyStaffStoreQueryable = new List<UserStore>().AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(emptyStaffStoreQueryable.Object)
                .Returns(managerStoreQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.ActiveStaffAsync(staffId, managerId));

            // Verify store authorization was checked
            _mockUserStoreRepository.Verify(x => x.AsQueryable(), Times.Exactly(2));
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ActiveStaffAsync_WithManagerHavingNoStoreAccess_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var staff = CreateTestUser(staffId);
            var manager = CreateTestUser(managerId);
            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.Staff });

            // Staff has store access, manager has no store access
            var staffStoreQueryable = new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet();
            var emptyManagerStoreQueryable = new List<UserStore>().AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(staffStoreQueryable.Object)
                .Returns(emptyManagerStoreQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.ActiveStaffAsync(staffId, managerId));

            // Verify store authorization was checked
            _mockUserStoreRepository.Verify(x => x.AsQueryable(), Times.Exactly(2));
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
