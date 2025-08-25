using FOCS.Application.DTOs;
using FOCS.Common.Constants;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.StaffServiceTest
{
    public class GetStaffListTest : StaffServiceTestBase
    {
        [Fact]
        public async Task GetStaffListAsync_WithValidStoreAndStaffUsers_ShouldReturnPagedStaffList()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };

            var staffUser1 = CreateTestUser("staff1");
            var staffUser2 = CreateTestUser("staff2");
            var kitchenStaffUser = CreateTestUser("kitchen1");

            var userStores = new List<UserStore>
            {
                CreateTestUserStore(Guid.Parse(staffUser1.Id), Guid.Parse(storeId)),
                CreateTestUserStore(Guid.Parse(staffUser2.Id), Guid.Parse(storeId)),
                CreateTestUserStore(Guid.Parse(kitchenStaffUser.Id), Guid.Parse(storeId))
            };

            var allUsers = new List<User> { staffUser1, staffUser2, kitchenStaffUser };

            // Setup repository to return user stores for the given store
            _mockUserStoreRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);

            // Setup UserManager to return users
            var mockUserQueryable = allUsers.AsQueryable();
            _mockUserManager.Setup(x => x.Users)
                .Returns(mockUserQueryable);

            // Setup role assignments
            _mockUserManager.Setup(x => x.GetRolesAsync(staffUser1))
                .ReturnsAsync(new List<string> { Roles.Staff });
            _mockUserManager.Setup(x => x.GetRolesAsync(staffUser2))
                .ReturnsAsync(new List<string> { Roles.Staff });
            _mockUserManager.Setup(x => x.GetRolesAsync(kitchenStaffUser))
                .ReturnsAsync(new List<string> { Roles.KitchenStaff });

            // Setup mapper
            _mockMapper.Setup(x => x.Map<StaffProfileDTO>(It.IsAny<User>()))
                .Returns((User u) => new StaffProfileDTO
                {
                    //Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber
                });

            // Act
            var result = await _staffService.GetStaffListAsync(query, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count);
            Assert.Equal(1, result.PageIndex);
            Assert.Equal(10, result.PageSize);

            // Verify all staff have correct roles
            Assert.All(result.Items, staff =>
                Assert.True(staff.Roles.Contains(Roles.Staff) || staff.Roles.Contains(Roles.KitchenStaff)));

            // Verify repository and UserManager calls
            _mockUserStoreRepository.Verify(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Exactly(3));
        }

        [Fact]
        public async Task GetStaffListAsync_WithInvalidStoreId_ShouldThrowException()
        {
            // Arrange
            var invalidStoreId = "invalid-guid";
            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                _staffService.GetStaffListAsync(query, invalidStoreId));

            // Verify no repository calls were made
            _mockUserStoreRepository.Verify(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task GetStaffListAsync_WithNoUserStores_ShouldReturnEmptyList()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };

            _mockUserStoreRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(new List<UserStore>());

            var mockUserQueryable = new List<User>().AsQueryable();
            _mockUserManager.Setup(x => x.Users)
                .Returns(mockUserQueryable);

            // Act
            var result = await _staffService.GetStaffListAsync(query, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetStaffListAsync_WithUsersHavingNoStaffRoles_ShouldReturnEmptyList()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };

            var regularUser = CreateTestUser("user1");
            var userStores = new List<UserStore>
            {
                CreateTestUserStore(Guid.Parse(regularUser.Id), Guid.Parse(storeId))
            };

            _mockUserStoreRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);

            var mockUserQueryable = new List<User> { regularUser }.AsQueryable();
            _mockUserManager.Setup(x => x.Users)
                .Returns(mockUserQueryable);

            // User has non-staff role
            _mockUserManager.Setup(x => x.GetRolesAsync(regularUser))
                .ReturnsAsync(new List<string> { Roles.User });

            // Act
            var result = await _staffService.GetStaffListAsync(query, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetStaffListAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var query = new UrlQueryParameters { Page = 2, PageSize = 2 };

            var users = new List<User>();
            var userStores = new List<UserStore>();

            for (int i = 1; i <= 5; i++)
            {
                var user = CreateTestUser($"staff{i}");
                user.Email = $"staff{i}@test.com";
                users.Add(user);
                userStores.Add(CreateTestUserStore(Guid.Parse(user.Id), Guid.Parse(storeId)));
            }

            _mockUserStoreRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);

            var mockUserQueryable = users.AsQueryable();
            _mockUserManager.Setup(x => x.Users)
                .Returns(mockUserQueryable);

            foreach (var user in users)
            {
                _mockUserManager.Setup(x => x.GetRolesAsync(user))
                    .ReturnsAsync(new List<string> { Roles.Staff });
            }

            //_mockMapper.Setup(x => x.Map<StaffProfileDTO>(It.IsAny<User>()))
            //    .Returns((User u) => new StaffProfileDTO { Email = u.Email });

            // Act
            var result = await _staffService.GetStaffListAsync(query, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.Items.Count); // Page size
            Assert.Equal(2, result.PageIndex);
            Assert.Equal(2, result.PageSize);
        }

        [Fact]
        public async Task GetStaffListAsync_WithFilters_ShouldApplyRoleFilter()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                Filters = new Dictionary<string, string> { { "role", Roles.Staff } }
            };

            var staffUser = CreateTestUser("staff1");
            var kitchenStaffUser = CreateTestUser("kitchen1");

            var userStores = new List<UserStore>
            {
                CreateTestUserStore(Guid.Parse(staffUser.Id), Guid.Parse(storeId)),
                CreateTestUserStore(Guid.Parse(kitchenStaffUser.Id), Guid.Parse(storeId))
            };

            _mockUserStoreRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);

            var mockUserQueryable = new List<User> { staffUser, kitchenStaffUser }.AsQueryable();
            _mockUserManager.Setup(x => x.Users)
                .Returns(mockUserQueryable);

            _mockUserManager.Setup(x => x.GetRolesAsync(staffUser))
                .ReturnsAsync(new List<string> { Roles.Staff });
            _mockUserManager.Setup(x => x.GetRolesAsync(kitchenStaffUser))
                .ReturnsAsync(new List<string> { Roles.KitchenStaff });

            _mockMapper.Setup(x => x.Map<StaffProfileDTO>(It.IsAny<User>()))
                .Returns((User u) => new StaffProfileDTO
                {
                    //Email = u.Email,
                    Roles = u.Id == staffUser.Id ? new List<string> { Roles.Staff } : new List<string> { Roles.KitchenStaff }
                });

            // Act
            var result = await _staffService.GetStaffListAsync(query, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Items.Count); // Only staff role should be returned
            Assert.True(result.Items.First().Roles.Contains(Roles.Staff));
        }

        [Fact]
        public async Task GetStaffListAsync_WithSearch_ShouldApplyEmailSearch()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SearchBy = "email",
                SearchValue = "john"
            };

            var matchingUser = CreateTestUser("match1");
            matchingUser.Email = "john@test.com";
            var nonMatchingUser = CreateTestUser("nomatch1");
            nonMatchingUser.Email = "jane@test.com";

            var userStores = new List<UserStore>
            {
                CreateTestUserStore(Guid.Parse(matchingUser.Id), Guid.Parse(storeId)),
                CreateTestUserStore(Guid.Parse(nonMatchingUser.Id), Guid.Parse(storeId))
            };

            _mockUserStoreRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);

            var mockUserQueryable = new List<User> { matchingUser, nonMatchingUser }.AsQueryable();
            _mockUserManager.Setup(x => x.Users)
                .Returns(mockUserQueryable);

            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { Roles.Staff });

            //_mockMapper.Setup(x => x.Map<StaffProfileDTO>(It.IsAny<User>()))
            //    .Returns((User u) => new StaffProfileDTO { Email = u.Email });

            // Act
            var result = await _staffService.GetStaffListAsync(query, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Items.Count);
            //Assert.Contains("john", result.Items.First().Email.ToLower());
        }

        [Fact]
        public async Task GetStaffListAsync_WithSort_ShouldApplyEmailSort()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SortBy = "email",
                SortOrder = "desc"
            };

            var user1 = CreateTestUser("user1");
            user1.Email = "a@test.com";
            var user2 = CreateTestUser("user2");
            user2.Email = "z@test.com";

            var userStores = new List<UserStore>
            {
                CreateTestUserStore(Guid.Parse(user1.Id), Guid.Parse(storeId)),
                CreateTestUserStore(Guid.Parse(user2.Id), Guid.Parse(storeId))
            };

            _mockUserStoreRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<UserStore, bool>>>()))
                .ReturnsAsync(userStores);

            var mockUserQueryable = new List<User> { user1, user2 }.AsQueryable();
            _mockUserManager.Setup(x => x.Users)
                .Returns(mockUserQueryable);

            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { Roles.Staff });

            //_mockMapper.Setup(x => x.Map<StaffProfileDTO>(It.IsAny<User>()))
            //    .Returns((User u) => new StaffProfileDTO { Email = u.Email });

            // Act
            var result = await _staffService.GetStaffListAsync(query, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            //Assert.Equal("z@test.com", result.Items.First().Email); // Descending order
            //Assert.Equal("a@test.com", result.Items.Last().Email);
        }
    }
}
