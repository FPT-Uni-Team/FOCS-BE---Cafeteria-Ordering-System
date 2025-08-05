using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.StaffServiceTest
{
    public class DeleteStaffAccountTest : StaffServiceTestBase
    {
        [Fact]
        public async Task DeleteStaffAsync_WithValidStaffAndManager_ShouldReturnTrue()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var staff = CreateTestUser(staffId);
            var manager = CreateTestUser(managerId);

            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);
            var managerUserStore = CreateTestUserStore(Guid.Parse(managerId), storeId);

            // Setup ValidatePermissionAsync components
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Setup staff role validation (checkStaff = true)
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.KitchenStaff))
                .ReturnsAsync(false);

            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.Staff });

            // Setup store authorization
            var staffStoreQueryable = new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(staffStoreQueryable.Object)
                .Returns(managerStoreQueryable.Object);

            // Setup UserManager update for soft delete
            _mockUserManager.Setup(x => x.UpdateAsync(staff))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _staffService.DeleteStaffAsync(staffId, managerId);

            // Assert
            Assert.True(result);

            // Verify soft delete properties were set
            Assert.False(staff.IsActive);
            Assert.True(staff.IsDeleted);
            Assert.Equal(managerId, staff.UpdatedBy);
            Assert.True(staff.UpdatedAt <= DateTime.UtcNow);
            Assert.True(staff.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));

            // Verify all interactions
            _mockUserManager.Verify(x => x.FindByIdAsync(staffId), Times.Once);
            _mockUserManager.Verify(x => x.FindByIdAsync(managerId), Times.Once);
            _mockUserManager.Verify(x => x.IsInRoleAsync(staff, Roles.Staff), Times.Once);
            _mockUserManager.Verify(x => x.UpdateAsync(staff), Times.Once);
        }

        [Fact]
        public async Task DeleteStaffAsync_WithNonExistentStaff_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.DeleteStaffAsync(staffId, managerId));

            // Verify no delete operations were attempted
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task DeleteStaffAsync_WithNonExistentManager_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();

            var staff = CreateTestUser(staffId);

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(staff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync((User)null);

            // User is not in Staff or KitchenStaff role
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.KitchenStaff))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.DeleteStaffAsync(staffId, managerId));

            // Verify validation stopped at manager check
            _mockUserManager.Verify(x => x.FindByIdAsync(staffId), Times.Once);
            _mockUserManager.Verify(x => x.FindByIdAsync(managerId), Times.Once);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task DeleteStaffAsync_WithNonStaffUser_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();

            var nonStaff = CreateTestUser(staffId);
            var manager = CreateTestUser(managerId);

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(nonStaff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // User is not in Staff or KitchenStaff role
            _mockUserManager.Setup(x => x.IsInRoleAsync(nonStaff, Roles.Staff))
                .ReturnsAsync(false);
            _mockUserManager.Setup(x => x.IsInRoleAsync(nonStaff, Roles.KitchenStaff))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.DeleteStaffAsync(staffId, managerId));

            // Verify role check was performed but no delete operations
            _mockUserManager.Verify(x => x.IsInRoleAsync(nonStaff, Roles.Staff), Times.Once);
            _mockUserManager.Verify(x => x.IsInRoleAsync(nonStaff, Roles.KitchenStaff), Times.Once);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task DeleteStaffAsync_WithUserHavingUserRole_ShouldThrowException()
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

            // Staff passes role check but has User role (not allowed)
            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.User, Roles.Staff });

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.DeleteStaffAsync(staffId, managerId));

            // Verify role validation was performed
            _mockUserManager.Verify(x => x.GetRolesAsync(staff), Times.Once);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task DeleteStuffAsync_WithUnauthorizedStoreAccess_ShouldThrowException()
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

            _mockUserManager.Setup(x => x.IsInRoleAsync(staff, Roles.Staff))
                .ReturnsAsync(true);
            _mockUserManager.Setup(x => x.GetRolesAsync(staff))
                .ReturnsAsync(new List<string> { Roles.Staff });

            // Setup different store access
            var staffStoreQueryable = new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(staffStoreQueryable.Object)
                .Returns(managerStoreQueryable.Object);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.DeleteStaffAsync(staffId, managerId));

            // Verify authorization was checked but no delete operations
            _mockUserStoreRepository.Verify(x => x.AsQueryable(), Times.Exactly(2));
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task DeleteStaffAsync_WithKitchenStaff_ShouldSucceed()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var kitchenStaff = CreateTestUser(staffId);
            var manager = CreateTestUser(managerId);

            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);
            var managerUserStore = CreateTestUserStore(Guid.Parse(managerId), storeId);

            // Setup ValidatePermissionAsync components
            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(kitchenStaff);
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(manager);

            // Kitchen staff role validation
            _mockUserManager.Setup(x => x.IsInRoleAsync(kitchenStaff, Roles.Staff))
                .ReturnsAsync(false);
            _mockUserManager.Setup(x => x.IsInRoleAsync(kitchenStaff, Roles.KitchenStaff))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(kitchenStaff))
                .ReturnsAsync(new List<string> { Roles.KitchenStaff });

            // Setup store authorization
            var staffStoreQueryable = new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(staffStoreQueryable.Object)
                .Returns(managerStoreQueryable.Object);

            _mockUserManager.Setup(x => x.UpdateAsync(kitchenStaff))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _staffService.DeleteStaffAsync(staffId, managerId);

            // Assert
            Assert.True(result);
            Assert.False(kitchenStaff.IsActive);
            Assert.True(kitchenStaff.IsDeleted);

            // Verify kitchen staff role was checked
            _mockUserManager.Verify(x => x.IsInRoleAsync(kitchenStaff, Roles.KitchenStaff), Times.Once);
        }
    }
}
