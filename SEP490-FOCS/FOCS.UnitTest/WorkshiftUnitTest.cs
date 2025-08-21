using AutoMapper;
using FOCS.Application.Services;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace FOCS.UnitTest
{
    public class WorkshiftUnitTest
    {
        private readonly Mock<IRepository<WorkshiftSchedule>> _mockWorkshiftScheduleRepository;
        private readonly Mock<IRepository<Workshift>> _mockWorkshiftRepository;
        private readonly Mock<IRepository<StaffWorkshiftRegistration>> _mockStaffWorkshiftRepository;
        private readonly Mock<IRepository<Store>> _mockStoreRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly WorkshiftScheduleService _service;

        public WorkshiftUnitTest()
        {
            _mockWorkshiftScheduleRepository = new Mock<IRepository<WorkshiftSchedule>>();
            _mockWorkshiftRepository = new Mock<IRepository<Workshift>>();
            _mockStaffWorkshiftRepository = new Mock<IRepository<StaffWorkshiftRegistration>>();
            _mockStoreRepository = new Mock<IRepository<Store>>();
            _mockMapper = new Mock<IMapper>();

            _service = new WorkshiftScheduleService(
                _mockWorkshiftScheduleRepository.Object,
                _mockWorkshiftRepository.Object,
                _mockStaffWorkshiftRepository.Object,
                _mockStoreRepository.Object,
                _mockMapper.Object);
        }

        #region CM-56 Create Workshift Tests

        [Theory]
        [InlineData("cd9a18ab-67e6-4e4c-b5a9-17c25b1245ea", "960b2be0-7667-4938-9ed1-a36617ab0e45", "2025-08-10T06:44:45.878Z", "77356722-4157-44f0-9821-03c984bfa190", "HuyND", "07:00:00", "12:00:00")]
        [InlineData(null, "0746240b-e4dd-4616-b669-60794b6c3c39", "2025-08-10T06:44:45.878Z", "77356722-4157-44f0-9821-03c984bfa190", "HuyND", "07:00:00", "12:00:00")]
        [InlineData("631b2586-df8b-4021-8365-11335bfb5043", null, "2025-08-10T06:44:45.878Z", "77356722-4157-44f0-9821-03c984bfa190", "HuyND", "07:00:00", "12:00:00")]
        [InlineData("631b2586-df8b-4021-8365-11335bfb5043", "960b2be0-7667-4938-9ed1-a36617ab0e45", "", "77356722-4157-44f0-9821-03c984bfa190", "HuyND", "07:00:00", "12:00:00")]
        [InlineData("631b2586-df8b-4021-8365-11335bfb5043", "960b2be0-7667-4938-9ed1-a36617ab0e45", null, "77356722-4157-44f0-9821-03c984bfa190", "HuyND", "07:00:00", "12:00:00")]
        [InlineData("631b2586-df8b-4021-8365-11335bfb5043", "960b2be0-7667-4938-9ed1-a36617ab0e45", "2025-08-10T06:44:45.878Z", null, "HuyND", "07:00:00", "12:00:00")]
        [InlineData("631b2586-df8b-4021-8365-11335bfb5043", "960b2be0-7667-4938-9ed1-a36617ab0e45", "2025-08-10T06:44:45.878Z", "77356722-4157-44f0-9821-03c984bfa190", null, "07:00:00", "12:00:00")]
        [InlineData("631b2586-df8b-4021-8365-11335bfb5043", "960b2be0-7667-4938-9ed1-a36617ab0e45", "2025-08-10T06:44:45.878Z", "77356722-4157-44f0-9821-03c984bfa190", "HuyND", null, "12:00:00")]
        [InlineData("631b2586-df8b-4021-8365-11335bfb5043", "960b2be0-7667-4938-9ed1-a36617ab0e45", "2025-08-10T06:44:45.878Z", "77356722-4157-44f0-9821-03c984bfa190", "HuyND", "07:00:00", null)]
        [InlineData("631b2586-df8b-4021-8365-11335bfb5043", "960b2be0-7667-4938-9ed1-a36617ab0e45", "2025-08-10T06:44:45.878Z", null, null, null, null)]
        [InlineData(null, null, null, null, null, null, null)]
        public async Task CreateAsync_WithInvalidInput_ShouldReturnFalse(
            string storeId,
            string workshiftId,
            string workDate,
            string staffId,
            string staffName,
            string startTime,
            string endTime)
        {
            // Arrange
            var workshiftRequest = new WorkshiftResponse
            {
                Id = workshiftId != null ? Guid.Parse(workshiftId) : Guid.Empty,
                WorkDate = !string.IsNullOrEmpty(workDate) ? DateTime.Parse(workDate) : DateTime.MinValue,
                Shift = new List<StaffWorkshiftResponse>()
            };

            workshiftRequest.Shift.Add(new StaffWorkshiftResponse
            {
                StaffId = Guid.Parse(staffId),
                StaffName = staffName,
                StartTime = TimeSpan.Parse(startTime),
                EndTime = TimeSpan.Parse(endTime)
            });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(workshiftRequest, storeId));
        }

        [Theory]
        [InlineData("631b2586-df8b-4021-8365-11335bfb5043", "960b2be0-7667-4938-9ed1-a36617ab0e45", "2025-08-10T06:44:45.878Z", "77356722-4157-44f0-9821-03c984bfa190", "HuyND", "07:00:00", "12:00:00")]
        [InlineData("631b2586-df8b-4021-8365-11335bfb5043", "0746240b-e4dd-4616-b669-60794b6c3c39", "2025-08-10T06:44:45.878Z", "77356722-4157-44f0-9821-03c984bfa190", "HuyND", "07:00:00", "12:00:00")]
        [InlineData("631b2586-df8b-4021-8365-11335bfb5043", null, "2025-08-10T06:44:45.878Z", "77356722-4157-44f0-9821-03c984bfa190", "HuyND", "07:00:00", "12:00:00")]
        public async Task CreateAsync_WithValidInput_ShouldReturnTrue(
            string storeId,
            string workshiftId,
            string workDate,
            string staffId,
            string staffName,
            string startTime,
            string endTime)
        {
            // Arrange
            var workshiftRequest = new WorkshiftResponse
            {
                Id = workshiftId != null ? Guid.Parse(workshiftId) : Guid.Empty,
                WorkDate = string.IsNullOrEmpty(workDate) ? DateTime.Parse(workDate) : DateTime.MinValue,
                Shift = new List<StaffWorkshiftResponse>()
            };

            workshiftRequest.Shift.Add(new StaffWorkshiftResponse
            {
                StaffId = Guid.Parse(staffId),
                StaffName = staffName,
                StartTime = TimeSpan.Parse(startTime),
                EndTime = TimeSpan.Parse(endTime)
            });

            workshiftRequest.Id = Guid.NewGuid();


            var workshift = new List<Workshift>();

            workshift.Add(new Workshift
            {
                Id = Guid.NewGuid(),
                StoreId = Guid.Parse(storeId),
                WorkDate = workshiftRequest.WorkDate
            });
            _mockWorkshiftRepository.Setup(r => r.AsQueryable())
            .Returns(workshift.AsQueryable().BuildMockDbSet().Object);

            _mockWorkshiftRepository.Setup(r => r.AddAsync(It.IsAny<Workshift>()))
                .Returns(Task.CompletedTask);
            _mockWorkshiftScheduleRepository.Setup(r => r.AddAsync(It.IsAny<WorkshiftSchedule>()))
                .Returns(Task.CompletedTask);
            _mockStaffWorkshiftRepository.Setup(r => r.AddAsync(It.IsAny<StaffWorkshiftRegistration>()))
                .Returns(Task.CompletedTask);
            _mockWorkshiftScheduleRepository.Setup(r => r.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            _mockStoreRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Store { Id = Guid.Parse(storeId) });

            // Act
            var result = await _service.CreateAsync(workshiftRequest, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(Guid.TryParse(result, out _));
        }

        #endregion

        #region CM-57 Get All Workshift Tests

        [Theory]
        [InlineData(null, 10, "name", "Thai", "created_date", "desc")]
        [InlineData(1, null, "name", "Thai", "created_date", "desc")]
        [InlineData(null, null, null, null, null, null)]
        public async Task ListAllWorkshiftsAsync_WithInvalidPagination_ShouldValidateFalse(
            int page,
            int pageSize,
            string searchBy,
            string searchValue,
            string sortBy,
            string sortOrder)
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = page,
                PageSize = pageSize,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Act
            var validationContext = new ValidationContext(query);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(query, validationContext, validationResults, true);

            //Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData(1, 10, "name", "Thai", "created_date", "desc", true)]
        [InlineData(5, 10, "name", "Thai", "created_date", "desc", true)]
        [InlineData(1, 20, "name", "Thai", "created_date", "desc", true)]
        [InlineData(1, 10, null, "Thai", "created_date", "desc", true)]
        [InlineData(1, 10, "name", null, "created_date", "desc", true)]
        [InlineData(1, 10, "name", "Thai", null, "desc", true)]
        [InlineData(1, 10, "name", "Thai", "created_date", null, true)]
        [InlineData(1, 10, "name", "Thai", "created_date", "desc", false)]
        [InlineData(1, 10, "name", "Thai", "created_date", "desc", null)]
        public async Task ListAll_VariousParameters_ReturnsExpectedResult(
            int page,
            int pageSize,
            string searchBy,
            string searchValue,
            string sortBy,
            string sortOrder,
            bool hasFilters)
        {
            // Arrange
            var storeId = "631b2586-df8b-4021-8365-11335bfb5043";
            var urlQueryParameters = new UrlQueryParameters
            {
                Page = page,
                PageSize = pageSize,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Filters = hasFilters ? new Dictionary<string, string>
                {
                    { "storeId", storeId }
                } : null
            };

            var workshifts = new List<Workshift>
            {
                new Workshift
                {
                    Id = Guid.NewGuid(),
                    StoreId = Guid.Parse(storeId),
                    WorkDate = DateTime.Now,
                    CreatedAt = DateTime.UtcNow,
                    WorkshiftSchedules = new List<WorkshiftSchedule>
                    {
                        new WorkshiftSchedule
                        {
                            Name = "Morning Shift",
                            StartTime = TimeSpan.FromHours(8),
                            EndTime = TimeSpan.FromHours(16),
                            StaffWorkshiftRegistrations = new List<StaffWorkshiftRegistration>
                            {
                                new StaffWorkshiftRegistration { StaffName = "Thai" }
                            }
                        }
                    }
                }
            }.AsQueryable().BuildMockDbSet();

            _mockWorkshiftRepository.Setup(r => r.AsQueryable())
                .Returns(workshifts.Object);

            // Act
            var result = await _service.ListAll(urlQueryParameters, storeId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResult<WorkshiftResponse>>(result);
        }

        #endregion

        #region CM-58 Delete Workshift Tests

        [Theory]
        [InlineData("65a18f22-dd31-4ea6-b456-a775dbcfd62e")]
        [InlineData(null)]
        public async Task DeleteScheduleAsync_WithInvalidInput_ShouldReturnFalse(string scheduleId)
        {
            // Arrange
            Guid scheduleIdGuid = Guid.TryParse(scheduleId, out var parsedScheduleId) ? parsedScheduleId : Guid.Empty;

            _mockWorkshiftRepository.Setup(r => r.AsQueryable())
                .Returns(new List<Workshift>().AsQueryable().BuildMockDbSet().Object);

            // Act & Assert
            var result = await _service.DeleteScheduleAsync(scheduleIdGuid);
            Assert.False(result);
        }

        [Theory]
        [InlineData("5e45861b-ac1d-4433-8bdc-ac48a18d8012")]
        public async Task DeleteScheduleAsync_WithValidInput_ShouldReturnTrue(string scheduleId)
        {
            // Arrange
            Guid scheduleIdGuid = Guid.Parse(scheduleId);

            var existingWorkshift = new Workshift
            {
                Id = scheduleIdGuid,
                StoreId = Guid.NewGuid(),
                WorkDate = DateTime.Now
            };

            var workshifts = new List<Workshift> { existingWorkshift }.AsQueryable().BuildMockDbSet();

            _mockWorkshiftRepository.Setup(r => r.AsQueryable())
                .Returns(workshifts.Object);
            _mockWorkshiftScheduleRepository.Setup(r => r.AsQueryable())
                .Returns(new List<WorkshiftSchedule>().AsQueryable().BuildMockDbSet().Object);
            _mockWorkshiftRepository.Setup(r => r.Remove(It.IsAny<Workshift>()));
            _mockWorkshiftRepository.Setup(r => r.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // Act 
            var result = await _service.DeleteScheduleAsync(scheduleIdGuid);

            // Assert
            Assert.True(result);
        }

        #endregion
    }
}
