using AutoMapper;
using FOCS.Application.DTOs;
using FOCS.Application.Services;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.StaffServiceTest
{
    public abstract class StaffServiceTestBase
    {
        protected readonly Mock<UserManager<User>> _mockUserManager;
        protected readonly Mock<IRepository<UserStore>> _mockUserStoreRepository;
        protected readonly Mock<IMapper> _mockMapper;
        protected readonly Mock<IEmailService> _mockEmailService;
        protected readonly Mock<IRepository<Store>> _mockStoreRepository;
        protected readonly StaffService _staffService;

        protected StaffServiceTestBase()
        {
            // Setup UserManager mock
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            _mockUserStoreRepository = new Mock<IRepository<UserStore>>();
            _mockMapper = new Mock<IMapper>();
            _mockEmailService = new Mock<IEmailService>();
            _mockStoreRepository = new Mock<IRepository<Store>>();

            _staffService = new StaffService(
                _mockUserManager.Object,
                _mockUserStoreRepository.Object,
                _mockMapper.Object,
                _mockEmailService.Object,
                _mockStoreRepository.Object);
        }

        protected RegisterRequest CreateValidRegisterRequest()
        {
            return new RegisterRequest
            {
                //Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Phone = "1234567890",
                Password = "Password123!"
            };
        }

        protected User CreateTestUser(string email = null)
        {
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = email ?? "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                UserName = "test",
                PhoneNumber = "1234567890",
                IsActive = true,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                SecurityStamp = Guid.NewGuid().ToString()
            };
        }

        protected Store CreateTestStore(Guid? id = null)
        {
            return new Store
            {
                Id = id ?? Guid.NewGuid()
                // Add other required properties based on your Store entity
            };
        }

        protected UserStore CreateTestUserStore(Guid userId, Guid storeId)
        {
            return new UserStore
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoreId = storeId,
                JoinDate = DateTime.UtcNow,
                Status = Common.Enums.UserStoreStatus.Active
            };
        }

        protected UserStoreDTO CreateTestUserStoreDTO(Guid userId, Guid storeId)
        {
            return new UserStoreDTO
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoreId = storeId,
                BlockReason = null,
                JoinDate = DateTime.UtcNow,
                Status = Common.Enums.UserStoreStatus.Active
            };
        }

        protected StaffProfileDTO CreateTestStaffProfileDTO()
        {
            return new StaffProfileDTO
            {
                //Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "1234567890"
            };
        }

        protected void SetupValidStoreExists(Guid storeId)
        {
            var store = CreateTestStore(storeId);
            _mockStoreRepository.Setup(x => x.GetByIdAsync(storeId))
                .ReturnsAsync(store);
        }

        protected void SetupValidManagerStoreAccess(string managerId, Guid storeId)
        {
            var managerUserStores = new List<UserStore>
            {
                CreateTestUserStore(Guid.Parse(managerId), storeId)
            };

            var mockQueryable = managerUserStores.AsQueryable().BuildMockDbSet();

            _mockUserStoreRepository.Setup(x => x.AsQueryable())
                .Returns(mockQueryable.Object);
        }

        protected void SetupSuccessfulUserCreation(User user, string password)
        {
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), password))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>((u, p) =>
                {
                    u.Id = user.Id;
                    u.SecurityStamp = user.SecurityStamp;
                    u.ConcurrencyStamp = user.ConcurrencyStamp;
                });
        }

        protected void SetupFailedUserCreation(string password, params string[] errors)
        {
            var identityError = errors.Select(e => new IdentityError { Description = e }).ToArray();
            var identityResult = IdentityResult.Failed(identityError);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), password))
                .ReturnsAsync(identityResult);
        }

        protected void SetupEmailConfirmationToken(User user, string token)
        {
            _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);
        }

        protected void SetupAddToRoleSuccess(User user, string role)
        {
            _mockUserManager.Setup(x => x.AddToRoleAsync(user, role))
                .ReturnsAsync(IdentityResult.Success);
        }

        protected void SetupMapperForUserStore()
        {
            _mockMapper.Setup(x => x.Map<UserStore>(It.IsAny<UserStoreDTO>()))
                .Returns((UserStoreDTO dto) => new UserStore
                {
                    Id = dto.Id,
                    UserId = dto.UserId,
                    StoreId = dto.StoreId,
                    JoinDate = dto.JoinDate,
                    Status = dto.Status,
                    BlockReason = dto.BlockReason
                });
        }

        protected void SetupMapperForStaffProfile(User user)
        {
            _mockMapper.Setup(x => x.Map<StaffProfileDTO>(user))
                .Returns(new StaffProfileDTO
                {
                    //Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber
                });
        }
    }
}
