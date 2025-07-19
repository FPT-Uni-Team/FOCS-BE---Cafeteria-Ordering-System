using AutoMapper;
using FOCS.Application.DTOs;
using FOCS.Application.Services;
using FOCS.Infrastructure.Identity.Identity.Model;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FOCS.UnitTest.UserProfileServiceTest
{
    public abstract class UserProfileServiceTestBase
    {
        protected readonly Mock<UserManager<User>> _mockUserManager;
        protected readonly Mock<IMapper> _mockMapper;
        protected readonly UserProfileService _service;
        protected readonly string _testUserId = Guid.NewGuid().ToString();
        protected readonly User _testUser;
        protected readonly UserProfileDTO _testUserProfileDTO;

        protected UserProfileServiceTestBase()
        {
            // Setup UserManager mock
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

            // Setup Mapper mock
            _mockMapper = new Mock<IMapper>();

            // Create test user
            _testUser = new User
            {
                Id = _testUserId,
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "0912345678",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            // Create test DTO
            _testUserProfileDTO = new UserProfileDTO
            {
                Email = "test@example.com",
                Firstname = "Updated",
                Lastname = "User",
                PhoneNumber = "0987654321"
            };

            // Create service instance
            _service = new UserProfileService(_mockUserManager.Object, _mockMapper.Object);
        }
    }
}