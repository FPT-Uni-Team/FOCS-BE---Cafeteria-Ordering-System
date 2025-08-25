using AutoMapper;
using FOCS.Application.DTOs;
using FOCS.Application.Services;
using FOCS.Common.Constants;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using System.ComponentModel.DataAnnotations;
using static QRCoder.PayloadGenerator;
using System.Numerics;

namespace FOCS.UnitTest
{
    public class StaffUnitTest
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IRepository<UserStore>> _mockUserStoreRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IRepository<Store>> _storeRepositoryMock;
        private readonly StaffService _staffService;

        public StaffUnitTest()
        {
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
            _mockUserStoreRepository = new Mock<IRepository<UserStore>>();
            _mockMapper = new Mock<IMapper>();
            _mockEmailService = new Mock<IEmailService>();
            _storeRepositoryMock = new Mock<IRepository<Store>>();

            _staffService = new StaffService(
                _mockUserManager.Object,
                _mockUserStoreRepository.Object,
                _mockMapper.Object,
                _mockEmailService.Object,
                _storeRepositoryMock.Object
            );
        }

        #region CreateStaff Tests (CM-52)

        [Theory]
        [InlineData("phucstaff", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc")]
        [InlineData(null, "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc")]
        [InlineData("phuc@staff.com", "phucpassword", "p", "0987654321", "Nguyen", "Phuc")]
        [InlineData("phuc@staff.com", "p", "p", "0987654321", "Nguyen", "Phuc")]
        [InlineData("phuc@staff.com", null, "phucpassword", "0987654321", "Nguyen", "Phuc")]
        [InlineData("phuc@staff.com", "phucpassword", null, "0987654321", "Nguyen", "Phuc")]
        [InlineData("phuc@staff.com", "phucpassword", "phucpassword", "966777888", "Nguyen", "Phuc")]
        public async Task CreateStaffAsync_WithInvalidInput_ShouldValidateFalse(
            string email,
            string password,
            string confirmPassword,
            string phone,
            string firstName,
            string lastName)
        {
            // Arrange
            var request = new RegisterRequest
            {
                //Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                Phone = phone,
                FirstName = firstName,
                LastName = lastName
            };

            // Act
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("phuc@exist.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("phuc@staff.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc", "4fa85f64-5717-4562-b3fc-2c963f66afa7")]
        [InlineData("phuc@staff.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc", "3fa85f64-5717-4562-b3fc")]
        [InlineData("phuc@staff.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc", null)]
        public async Task CreateStaffAsync_ShouldHandleVariousInputs_WithInvalidInput_ShouldReturnFalse(
            string email,
            string password,
            string confirmPassword,
            string phone,
            string firstName,
            string lastName,
            string storeId)
        {
            // Arrange
            var request = new RegisterRequest
            {
                //Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                Phone = phone,
                FirstName = firstName,
                LastName = lastName
            };

            var managerId = Guid.NewGuid();

            // Setup store repository
            Store store = storeId == "3fa85f64-5717-4562-b3fc-2c963f66afa6" ? new Store { Id = Guid.Parse(storeId) } : null;
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync(store);

            // Setup user manager
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());
            // Setup user store repository
            var userStores = new List<UserStore>
            {
                new UserStore
                {
                    UserId = managerId,
                    StoreId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _staffService.CreateStaffAsync(request, storeId, managerId.ToString()));

        }

        [Theory]
        [InlineData("phuc@staff.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", "Phuc")]
        [InlineData("phuc@staff.com", "phucpassword", "phucpassword", null, "Nguyen", "Phuc")]
        [InlineData("phuc@staff.com", "phucpassword", "phucpassword", "0987654321", null, "Phuc")]
        [InlineData("phuc@staff.com", "phucpassword", "phucpassword", "0987654321", "Nguyen", null)]
        public async Task CreateStaffAsync_WithValidInput_ShouldReturnTrue(
            string email,
            string password,
            string confirmPassword,
            string phone,
            string firstName,
            string lastName)
        {
            // Arrange
            var request = new RegisterRequest
            {
                //Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                Phone = phone,
                FirstName = firstName,
                LastName = lastName
            };

            var managerId = Guid.NewGuid();
            var storeId = Guid.NewGuid();
            // Setup store repository
            var store = new Store { Id = storeId };
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync(store);

            // Setup user manager
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                //Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            }; 
            
            var dto = new StaffProfileDTO
            {
                Id = Guid.Parse(user.Id),
                Email = email,
                PhoneNumber = phone,
                FirstName = firstName,
                LastName = lastName,
                Roles = new List<string> { Roles.Staff }
            };


            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockMapper.Setup(x => x.Map<StaffProfileDTO>(It.IsAny<User>()))
                .Returns(dto);

            // Setup user store repository
            var userStores = new List<UserStore>
            {
                new UserStore
                {
                    UserId = managerId,
                    StoreId = storeId
                }
            }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.Setup(x => x.AsQueryable())
                .Returns(userStores.Object);

            _mockUserStoreRepository.Setup(x => x.AddAsync(It.IsAny<UserStore>()))
                .Returns(Task.CompletedTask);

            _mockUserStoreRepository.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act & Assert
            var result = await _staffService.CreateStaffAsync(request, storeId.ToString(), managerId.ToString());

            // Assert
            Assert.NotNull(result);
            Assert.IsType<StaffProfileDTO>(result);
            Assert.True(result.Roles.Contains(Roles.Staff));
        }

        #endregion

        #region EditProfile Tests (CM-53)

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "phucstaff.com", "0987654321", "Nguyen", "Phuc")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", null, "0987654321", "Nguyen", "Phuc")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "phuc@staff.com", "966777888", "Nguyen", "Phuc")]
        public async Task UpdateStaffProfileAsync_WithInvalidInput_ShouldValidateFalse(
            string staffId,
            string email,
            string phone,
            string firstName,
            string lastName)
        {
            // Arrange
            var dto = new StaffProfileDTO
            {
                Id = Guid.Parse(staffId),
                Email = email,
                PhoneNumber = phone,
                FirstName = firstName,
                LastName = lastName
            };

            // Act
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("4fa85f64-5717-4562-b3fc-2c963f66afa7", "phuc@staff.com", "0987654321", "Nguyen", "Phuc")]
        [InlineData("5fa85f64-5717-4562-b3fc-2c963f66afa7", "phuc@staff.com", "0987654321", "Nguyen", "Phuc")]
        [InlineData("3fa85f64-5717-4562-b3fc", "phuc@staff.com", "0987654321", "Nguyen", "Phuc")]
        [InlineData(null, "phuc@staff.com", "0987654321", "Nguyen", "Phuc")]
        public async Task UpdateStaffProfileAsync_WithInvalidInput_ShouldReturnFalse(
            string staffId,
            string email,
            string phone,
            string firstName,
            string lastName)
        {
            // Arrange
            var validStaffId = Guid.TryParse(staffId, out var staffIdGuid);
            var dto = new StaffProfileDTO
            {
                Id = staffIdGuid,
                Email = email,
                PhoneNumber = phone,
                FirstName = firstName,
                LastName = lastName
            };

            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            // Setup store repository
            Store store = new Store { Id = storeId };
            _storeRepositoryMock.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync(store);

            // Setup user manager
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(new User { Id = managerId });

            if (validStaffId && staffId == "5fa85f64-5717-4562-b3fc-2c963f66afa7")
            {
                _mockUserManager.Setup(x => x.FindByIdAsync(staffIdGuid.ToString()))
                    .ReturnsAsync(new User { Id = staffIdGuid.ToString() });

                _mockUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                    .ReturnsAsync(true);

                _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                    .ReturnsAsync(new List<string> { Roles.Staff });
            }

            // Setup user store repository
            var userStores = new List<UserStore>
            {
                new UserStore
                {
                    UserId = Guid.Parse(managerId),
                    StoreId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
                },
                new UserStore
                {
                    UserId = staffIdGuid,
                    StoreId = staffIdGuid
                }
            }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.Setup(x => x.AsQueryable())
                .Returns(userStores.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _staffService.UpdateStaffProfileAsync(dto, staffId, managerId));
        }

        [Theory]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "phuc@staff.com", "0987654321", "Nguyen", "Phuc")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "phuc@staff.com", null, "Nguyen", "Phuc")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "phuc@staff.com", "0987654321", null, "Phuc")]
        [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6", "phuc@staff.com", "0987654321", "Nguyen", null)]
        public async Task UpdateStaffProfileAsync_WithValidInput_ShouldReturnTrue(
            string staffId,
            string email,
            string phone,
            string firstName,
            string lastName)
        {
            // Arrange
            var dto = new StaffProfileDTO
            {
                Id = Guid.Parse(staffId),
                Email = email,
                PhoneNumber = phone,
                FirstName = firstName,
                LastName = lastName
            };

            var storeId = Guid.NewGuid();
            var managerId = Guid.NewGuid().ToString();

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(new User { Id = staffId });

            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(new User { Id = managerId });

            _mockUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { Roles.Staff });

            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockMapper.Setup(x => x.Map<StaffProfileDTO>(It.IsAny<User>()))
                .Returns(new StaffProfileDTO());

            // Setup user store validation
            var userStores = new List<UserStore>
            {
                new UserStore
                {
                    UserId = Guid.Parse(managerId),
                    StoreId = storeId
                },
                new UserStore
                {
                    UserId = Guid.Parse(staffId),
                    StoreId = storeId
                }
            }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.Setup(x => x.AsQueryable())
                .Returns(userStores.Object);

            // Act
            var result = await _staffService.UpdateStaffProfileAsync(dto, staffId, managerId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<StaffProfileDTO>(result);
        }

        #endregion

        #region AssignRole Tests (CM-54 & CM-55)

        [Theory]
        [InlineData("S", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData(null, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("Staff", "4fa85f64-5717-4562-b3fc-2c963f66afa7")]
        [InlineData("Staff", "5fa85f64-5717-4562-b3fc-2c963f66afa7")]
        [InlineData("Staff", "3fa85f64-5717-4562-b3fc")]
        [InlineData("Staff", null)]
        public async Task AddStaffRoleAsync_WithInvalidInput_ShouldReturnFalse(
            string role,
            string staffId)
        {
            // Arrange
            var validStaffId = Guid.TryParse(staffId, out var staffIdGuid);

            var managerId = Guid.NewGuid().ToString();

            // Setup user manager
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(new User { Id = managerId });

            if (validStaffId && staffId != "4fa85f64-5717-4562-b3fc-2c963f66afa7")
            {
                _mockUserManager.Setup(x => x.FindByIdAsync(staffIdGuid.ToString()))
                    .ReturnsAsync(new User { Id = staffIdGuid.ToString() });

                _mockUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                    .ReturnsAsync(true);

                _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                    .ReturnsAsync(new List<string> { Roles.Staff });
            }

            // Setup user store repository
            var userStores = new List<UserStore>
            {
                new UserStore
                {
                    UserId = Guid.Parse(managerId),
                    StoreId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
                },
                new UserStore
                {
                    UserId = staffIdGuid,
                    StoreId = staffIdGuid
                }
            }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.Setup(x => x.AsQueryable())
                .Returns(userStores.Object);

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _staffService.AddStaffRoleAsync(role, staffId, managerId));
        }

        [Theory]
        [InlineData("Staff", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task AddStaffRoleAsync_WithValidInput_ShouldReturnTrue(string role, string staffId)
        {
            // Arrange
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(new User { Id = staffId });

            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(new User { Id = managerId });

            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { Roles.Staff });

            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Setup user store validation
            var userStores = new List<UserStore>
                {
                    new UserStore { UserId = Guid.Parse(staffId), StoreId = storeId },
                    new UserStore { UserId = Guid.Parse(managerId), StoreId = storeId }
                }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.Setup(x => x.AsQueryable())
                .Returns(userStores.Object);

            // Act
            var result = await _staffService.AddStaffRoleAsync(role, staffId, managerId);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("S", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData(null, "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        [InlineData("Staff", "4fa85f64-5717-4562-b3fc-2c963f66afa7")]
        [InlineData("Staff", "5fa85f64-5717-4562-b3fc-2c963f66afa7")]
        [InlineData("Staff", "3fa85f64-5717-4562-b3fc")]
        [InlineData("Staff", null)]
        public async Task RemoveStaffRoleAsync_WithInvalidInput_ShouldReturnFalse(
            string role,
            string staffId)
        {
            // Arrange
            var validStaffId = Guid.TryParse(staffId, out var staffIdGuid);

            var managerId = Guid.NewGuid().ToString();

            // Setup user manager
            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(new User { Id = managerId });

            if (validStaffId && staffId != "4fa85f64-5717-4562-b3fc-2c963f66afa7")
            {
                _mockUserManager.Setup(x => x.FindByIdAsync(staffIdGuid.ToString()))
                    .ReturnsAsync(new User { Id = staffIdGuid.ToString() });

                _mockUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                    .ReturnsAsync(true);

                _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                    .ReturnsAsync(new List<string> { Roles.Staff });
            }

            // Setup user store repository
            var userStores = new List<UserStore>
            {
                new UserStore
                {
                    UserId = Guid.Parse(managerId),
                    StoreId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")
                },
                new UserStore
                {
                    UserId = staffIdGuid,
                    StoreId = staffIdGuid
                }
            }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.Setup(x => x.AsQueryable())
                .Returns(userStores.Object);

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _staffService.RemoveStaffRoleAsync(role, staffId, managerId));
        }

        [Theory]
        [InlineData("Staff", "3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task RemoveStaffRoleAsync_WithValidInput_ShouldReturnTrue(string role, string staffId)
        {
            // Arrange
            var managerId = Guid.NewGuid().ToString();
            var storeId = Guid.NewGuid();

            _mockUserManager.Setup(x => x.FindByIdAsync(staffId))
                .ReturnsAsync(new User { Id = staffId });

            _mockUserManager.Setup(x => x.FindByIdAsync(managerId))
                .ReturnsAsync(new User { Id = managerId });

            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { Roles.Staff });

            _mockUserManager.Setup(x => x.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Setup user store validation
            var userStores = new List<UserStore>
                {
                    new UserStore { UserId = Guid.Parse(staffId), StoreId = storeId },
                    new UserStore { UserId = Guid.Parse(managerId), StoreId = storeId }
                }.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.Setup(x => x.AsQueryable())
                .Returns(userStores.Object);

            // Act
            var result = await _staffService.RemoveStaffRoleAsync(role, staffId, managerId);

            // Assert
            Assert.True(result);
        }

        #endregion
    }
}
