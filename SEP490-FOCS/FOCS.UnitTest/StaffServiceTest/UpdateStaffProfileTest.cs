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
    public class UpdateStaffProfileTest : StaffServiceTestBase
    {
        [Fact]
        public async Task UpdateStaffProfileAsync_WithValidData_ShouldReturnUpdatedStaffProfile()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            var staff = CreateTestUser(staffId);
            staff.Email = "original@test.com";
            var manager = CreateTestUser(managerId);

            var updateDto = new StaffProfileDTO
            {
                FirstName = "UpdatedFirstName",
                LastName = "UpdatedLastName",
                PhoneNumber = "9876543210",
                Email = "updated@test.com" // This should be ignored
            };

            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);
            var managerUserStore = CreateTestUserStore(Guid.Parse(managerId), storeId);
            var expectedRoles = new List<string> { Roles.Staff };

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
                .ReturnsAsync(expectedRoles);

            // Setup store authorization
            var staffStoreQueryable = new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(staffStoreQueryable.Object)
                .Returns(managerStoreQueryable.Object);

            // Setup mapper
            _mockMapper.Setup(x => x.Map(updateDto, staff))
                .Callback<StaffProfileDTO, User>((dto, user) =>
                {
                    user.FirstName = dto.FirstName;
                    user.LastName = dto.LastName;
                    user.PhoneNumber = dto.PhoneNumber;
                    // Email should not be updated from DTO
                });

            _mockMapper.Setup(x => x.Map<StaffProfileDTO>(staff))
                .Returns(new StaffProfileDTO
                {
                    Email = staff.Email,
                    FirstName = updateDto.FirstName,
                    LastName = updateDto.LastName,
                    PhoneNumber = updateDto.PhoneNumber
                });

            // Setup UserManager update
            _mockUserManager.Setup(x => x.UpdateAsync(staff))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _staffService.UpdateStaffProfileAsync(updateDto, staffId, managerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("original@test.com", result.Email); // Email should remain original
            Assert.Equal("UpdatedFirstName", result.FirstName);
            Assert.Equal("UpdatedLastName", result.LastName);
            Assert.Equal("9876543210", result.PhoneNumber);
            Assert.Equal(expectedRoles, result.Roles);

            // Verify email was preserved in DTO before mapping
            Assert.Equal("original@test.com", updateDto.Email);

            // Verify all interactions
            _mockUserManager.Verify(x => x.FindByIdAsync(staffId), Times.Once);
            _mockUserManager.Verify(x => x.FindByIdAsync(managerId), Times.Once);
            _mockUserManager.Verify(x => x.IsInRoleAsync(staff, Roles.Staff), Times.Once);
            _mockMapper.Verify(x => x.Map(updateDto, staff), Times.Once);
            _mockUserManager.Verify(x => x.UpdateAsync(staff), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(staff), Times.Exactly(2)); // Once for validation, once for result

            // Verify UpdatedAt and UpdatedBy were set
            Assert.True(staff.UpdatedAt <= DateTime.UtcNow);
            Assert.True(staff.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
            Assert.Equal(staffId, staff.UpdatedBy);
        }

        [Fact]
        public async Task UpdateStaffProfileAsync_WithNonExistentStaff_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var updateDto = CreateTestStaffProfileDTO();

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.UpdateStaffProfileAsync(updateDto, staffId, managerId));

            // Verify no update operations were attempted
            _mockMapper.Verify(x => x.Map(It.IsAny<StaffProfileDTO>(), It.IsAny<User>()), Times.Never);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStaffProfileAsync_WithNonStaffUser_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var updateDto = CreateTestStaffProfileDTO();

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
                _staffService.UpdateStaffProfileAsync(updateDto, staffId, managerId));

            // Verify role check was performed but no update operations
            _mockUserManager.Verify(x => x.IsInRoleAsync(nonStaff, Roles.Staff), Times.Once);
            _mockUserManager.Verify(x => x.IsInRoleAsync(nonStaff, Roles.KitchenStaff), Times.Once);
            _mockMapper.Verify(x => x.Map(It.IsAny<StaffProfileDTO>(), It.IsAny<User>()), Times.Never);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStaffProfileAsync_WithUnauthorizedManager_ShouldThrowException()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var updateDto = CreateTestStaffProfileDTO();
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
                _staffService.UpdateStaffProfileAsync(updateDto, staffId, managerId));

            // Verify authorization was checked but no update operations
            _mockUserStoreRepository.Verify(x => x.AsQueryable(), Times.Exactly(2));
            _mockMapper.Verify(x => x.Map(It.IsAny<StaffProfileDTO>(), It.IsAny<User>()), Times.Never);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStaffProfileAsync_WithKitchenStaff_ShouldSucceed()
        {
            // Arrange
            var staffId = Guid.NewGuid().ToString();
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();
            var updateDto = CreateTestStaffProfileDTO();

            var kitchenStaff = CreateTestUser(staffId);
            var manager = CreateTestUser(managerId);

            var staffUserStore = CreateTestUserStore(Guid.Parse(staffId), storeId);
            var managerUserStore = CreateTestUserStore(Guid.Parse(managerId), storeId);
            var expectedRoles = new List<string> { Roles.KitchenStaff };

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
                .ReturnsAsync(expectedRoles);

            // Setup store authorization
            var staffStoreQueryable = new List<UserStore> { staffUserStore }.AsQueryable().BuildMockDbSet();
            var managerStoreQueryable = new List<UserStore> { managerUserStore }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.SetupSequence(x => x.AsQueryable())
                .Returns(staffStoreQueryable.Object)
                .Returns(managerStoreQueryable.Object);

            // Setup mapper and UserManager
            _mockMapper.Setup(x => x.Map(updateDto, kitchenStaff));
            _mockMapper.Setup(x => x.Map<StaffProfileDTO>(kitchenStaff))
                .Returns(updateDto);
            _mockUserManager.Setup(x => x.UpdateAsync(kitchenStaff))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _staffService.UpdateStaffProfileAsync(updateDto, staffId, managerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedRoles, result.Roles);

            // Verify kitchen staff role was checked
            _mockUserManager.Verify(x => x.IsInRoleAsync(kitchenStaff, Roles.KitchenStaff), Times.Once);
        }
    }
}
