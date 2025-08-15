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
using FOCS.Application.DTOs;
using Microsoft.AspNetCore.Identity.Data;

namespace FOCS.UnitTest
{
    public class UserProfileUnitTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserProfileService _userProfileService;

        public UserProfileUnitTest()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            _mapperMock = new Mock<IMapper>();
            _userProfileService = new UserProfileService(_userManagerMock.Object, _mapperMock.Object);
        }

        #region GetUserProfileAsync Tests

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetUserProfileAsync_WithVariousUserIds_ReturnsExpectedResult(bool userExists)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = userExists ? new User { Id = userId, Email = "phuc@user.com" } : null;
            var expectedDto = userExists ? new UserProfileDTO { Email = "phuc@user.com" } : null;

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            if (userExists)
            {
                _mapperMock.Setup(x => x.Map<UserProfileDTO>(user))
                    .Returns(expectedDto);
            }

            // Act & Assert
            if (userExists)
            {
                var result = await _userProfileService.GetUserProfileAsync(userId);
                Assert.NotNull(result);
                Assert.Equal(expectedDto.Email, result.Email);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(() => _userProfileService.GetUserProfileAsync(userId));
            }
        }

        #endregion

        #region UpdateUserProfileAsync Tests

        [Theory]
        [InlineData("phucuser.com", "0987654321", "Nguyen", "Phuc")]
        [InlineData(null, "0987654321", "Nguyen", "Phuc")]
        [InlineData("phuc@user.com", "966777888", "Nguyen", "Phuc")]
        [InlineData("phuc@user.com", null, "Nguyen", "Phuc")]
        public async Task UpdateUserProfileAsync_WithInvalidInput_ShouldValidateFalse(string email, string phone, string firstName, string lastName)
        {
            // Arrange
            var dto = new UserProfileDTO
            {
                Email = email,
                PhoneNumber = phone,
                Firstname = firstName,
                Lastname = lastName
            };

            // Act
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

            //Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("phuc@user.com", "0987654321", "Nguyen", "Phuc")] // User not found
        public async Task UpdateUserProfileAsync_WithInvalidInput_ShouldReturnFalse(string email, string phone, string firstName, string lastName)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            var inputDto = new UserProfileDTO
            {
                Email = email,
                PhoneNumber = phone,
                Firstname = firstName,
                Lastname = lastName
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _userProfileService.UpdateUserProfileAsync(inputDto, userId));
        }

        [Theory]
        [InlineData("phuc@user.com", "0987654321", "Nguyen", "Phuc")]
        [InlineData("phuc@user.com", "0987654321", null, "Phuc")]
        [InlineData("phuc@user.com", "0987654321", "Nguyen", null)]
        public async Task UpdateUserProfileAsync_WithValidInput_ShouldReturnTrue(string email, string phone, string firstName, string lastName)
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = userId,
                Email = email,
                IsActive = true,
                IsDeleted = false
            };

            var inputDto = new UserProfileDTO
            {
                Email = email,
                PhoneNumber = phone,
                Firstname = firstName,
                Lastname = lastName
            };

            var expectedDto = new UserProfileDTO
            {
                Email = email,
                PhoneNumber = phone,
                Firstname = firstName,
                Lastname = lastName
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _mapperMock.Setup(x => x.Map(inputDto, user))
                .Callback<UserProfileDTO, User>((dto, u) =>
                {
                    u.PhoneNumber = dto.PhoneNumber;
                    u.FirstName = dto.Firstname;
                    u.LastName = dto.Lastname;
                });

            _mapperMock.Setup(x => x.Map<UserProfileDTO>(user))
                .Returns(expectedDto);

            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act 
            var result = await _userProfileService.UpdateUserProfileAsync(inputDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Email, result.Email);
            Assert.Equal(expectedDto.PhoneNumber, result.PhoneNumber);
            Assert.Equal(expectedDto.Firstname, result.Firstname);
            Assert.Equal(expectedDto.Lastname, result.Lastname);
            Assert.Equal(userId, user.UpdatedBy);
            Assert.True(user.UpdatedAt <= DateTime.UtcNow);
        }

        #endregion
    }
}
