using FOCS.Application.DTOs;
using FOCS.Common.Exceptions;
using FOCS.Infrastructure.Identity.Identity.Model;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FOCS.UnitTest.UserProfileServiceTest
{
    public class UpdateUserProfileTest : UserProfileServiceTestBase
    {
        [Fact]
        public async Task UpdateUserProfileAsync_WithValidData_ReturnsUpdatedUserProfileDTO()
        {
            // Arrange
            var updateDTO = new UserProfileDTO
            {
                Email = "newemail@example.com", // This should be overwritten
                Firstname = "UpdatedFirst",
                Lastname = "UpdatedLast",
                PhoneNumber = "9999999999"
            };

            var updatedUser = new User
            {
                Id = _testUserId,
                Email = _testUser.Email, // Email should remain unchanged
                FirstName = "UpdatedFirst",
                LastName = "UpdatedLast",
                PhoneNumber = "9999999999",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = _testUserId
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                           .ReturnsAsync(_testUser);
            _mockMapper.Setup(m => m.Map(updateDTO, _testUser))
                      .Callback<UserProfileDTO, User>((dto, user) =>
                      {
                          user.FirstName = dto.Firstname;
                          user.LastName = dto.Lastname;
                          user.PhoneNumber = dto.PhoneNumber;
                      });
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                           .ReturnsAsync(IdentityResult.Success);
            _mockMapper.Setup(m => m.Map<UserProfileDTO>(It.IsAny<User>()))
                      .Returns(new UserProfileDTO
                      {
                          Email = _testUser.Email,
                          Firstname = "UpdatedFirst",
                          Lastname = "UpdatedLast"
                      });

            // Act
            var result = await _service.UpdateUserProfileAsync(updateDTO, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testUser.Email, updateDTO.Email); // Email should be preserved

            _mockUserManager.Verify(um => um.FindByIdAsync(_testUserId), Times.Once);
            _mockMapper.Verify(m => m.Map(updateDTO, _testUser), Times.Once);
            _mockUserManager.Verify(um => um.UpdateAsync(It.Is<User>(u =>
                u.UpdatedAt != null && u.UpdatedBy == _testUserId)), Times.Once);
            _mockMapper.Verify(m => m.Map<UserProfileDTO>(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_WithNullUser_ThrowsException()
        {
            // Arrange
            _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                           .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _service.UpdateUserProfileAsync(_testUserProfileDTO, _testUserId));

            Assert.Equal(Errors.Common.UserNotFound + "@" + Errors.FieldName.UserId, exception.Message);
            _mockUserManager.Verify(um => um.FindByIdAsync(_testUserId), Times.Once);
            _mockMapper.Verify(m => m.Map(It.IsAny<UserProfileDTO>(), It.IsAny<User>()), Times.Never);
            _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_PreservesEmailFromExistingUser()
        {
            // Arrange
            var originalEmail = "original@example.com";
            _testUser.Email = originalEmail;

            var updateDTO = new UserProfileDTO
            {
                Email = "different@example.com",
                Firstname = "Updated"
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                           .ReturnsAsync(_testUser);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                           .ReturnsAsync(IdentityResult.Success);
            _mockMapper.Setup(m => m.Map<UserProfileDTO>(It.IsAny<User>()))
                      .Returns(new UserProfileDTO());

            // Act
            await _service.UpdateUserProfileAsync(updateDTO, _testUserId);

            // Assert
            Assert.Equal(originalEmail, updateDTO.Email);
            _mockMapper.Verify(m => m.Map(It.Is<UserProfileDTO>(dto =>
                dto.Email == originalEmail), _testUser), Times.Once);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_SetsCorrectAuditFields()
        {
            // Arrange
            var beforeUpdate = DateTime.UtcNow;
            _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                           .ReturnsAsync(_testUser);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                           .ReturnsAsync(IdentityResult.Success);
            _mockMapper.Setup(m => m.Map<UserProfileDTO>(It.IsAny<User>()))
                      .Returns(new UserProfileDTO());

            // Act
            await _service.UpdateUserProfileAsync(_testUserProfileDTO, _testUserId);
            var afterUpdate = DateTime.UtcNow;

            // Assert
            _mockUserManager.Verify(um => um.UpdateAsync(It.Is<User>(u =>
                u.UpdatedBy == _testUserId &&
                u.UpdatedAt >= beforeUpdate &&
                u.UpdatedAt <= afterUpdate)), Times.Once);
        }
    }
}