using AutoMapper;
using CloudinaryDotNet;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class WorkshiftScheduleService : IWorkshiftScheduleService
    {
        private readonly IRepository<WorkshiftSchedule> _workshiftScheduleRepository;
        private readonly IRepository<Workshift> _workshiftRepository;
        private readonly IRepository<StaffWorkshiftRegistration> _staffWorkshiftRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IMapper _mapper;

        public WorkshiftScheduleService(IRepository<WorkshiftSchedule> workshiftScheduleRepository, 
            IRepository<Workshift> workshiftRepository, 
            IRepository<StaffWorkshiftRegistration> staffWorkshiftRepository, 
            IRepository<Store> storeRepository,
            IMapper mapper)
        {
            _workshiftRepository = workshiftRepository;
            _workshiftScheduleRepository = workshiftScheduleRepository;
            _staffWorkshiftRepository = staffWorkshiftRepository;
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<string> CreateAsync(WorkshiftResponse request, string storeId)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(storeId, out Guid storeIdGuid),
                                                    Errors.Common.InvalidGuidFormat,
                                                    Errors.FieldName.StoreId);
            var store = await _storeRepository.GetByIdAsync(storeIdGuid);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound, Errors.FieldName.StoreId);

            var createWorkshift = await CreateScheduleAsync(storeIdGuid, request.WorkDate);

            ConditionCheck.CheckCondition(createWorkshift != null, Errors.Common.NotFound);

            foreach(var item in request.Shift)
            {
                var createCurrent = await AddWorkShiftToScheduleAsync(createWorkshift.Id, new CreateWorkShiftDto
                {
                    StaffId = item.StaffId,   
                    Name = item.StaffName,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime
                }, storeId);

                ConditionCheck.CheckCondition(createCurrent != null, Errors.Common.IsExist);
            }

            return createWorkshift.Id.ToString();
        }

        // ========== Workshift Schedule ==========
        public async Task<PagedResult<WorkshiftResponse>> ListAll(UrlQueryParameters urlQueryParameters, string storeId)
        {
            var workshiftsQuery = _workshiftRepository.AsQueryable()
                .Include(x => x.WorkshiftSchedules)
                    .ThenInclude(y => y.StaffWorkshiftRegistrations)
                .Where(x => x.StoreId == Guid.Parse(storeId));

            if (!string.IsNullOrWhiteSpace(urlQueryParameters.SearchBy) &&
                !string.IsNullOrWhiteSpace(urlQueryParameters.SearchValue))
            {
                var searchBy = urlQueryParameters.SearchBy.ToLower();
                var searchValue = urlQueryParameters.SearchValue.ToLower();

                workshiftsQuery = searchBy switch
                {
                    "name" => workshiftsQuery.Where(w =>
                        w.WorkshiftSchedules.Any(s =>
                            s.Name != null && s.Name.ToLower().Contains(searchValue))),

                    "staff" => workshiftsQuery.Where(w =>
                        w.WorkshiftSchedules.Any(s =>
                            s.StaffWorkshiftRegistrations.Any(r =>
                                r.StaffName != null && r.StaffName.ToLower().Contains(searchValue)))),

                    _ => workshiftsQuery
                };
            }

            int page = urlQueryParameters.Page <= 0 ? 1 : urlQueryParameters.Page;
            int pageSize = urlQueryParameters.PageSize <= 0 ? 10 : urlQueryParameters.PageSize;

            int totalItems = await workshiftsQuery.CountAsync();

            var workshifts = await workshiftsQuery
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapWorkshifts = workshifts.Select(x => new WorkshiftResponse
            {
                Id = x.Id,
                WorkDate = x.WorkDate,
                Shift = x.WorkshiftSchedules.Select(y => new StaffWorkshiftResponse
                {
                    StartTime = y.StartTime,
                    EndTime = y.EndTime,
                    StaffName = y.StaffWorkshiftRegistrations
                                    .Select(r => r.StaffName)
                                    .Where(name => !string.IsNullOrEmpty(name))
                                    .FirstOrDefault()
                }).ToList()
            }).ToList();

            return new PagedResult<WorkshiftResponse>(mapWorkshifts, totalItems, page, pageSize);

        }


        public async Task<WorkshiftScheduleDto> CreateScheduleAsync(Guid storeId, DateTime workDate)
        {
            var workshiftExist = await _workshiftRepository.AsQueryable().FirstOrDefaultAsync(x => x.StoreId == storeId && x.WorkDate == workDate);

            if(workshiftExist != null)
            {
                return new WorkshiftScheduleDto
                {
                    StoreId = storeId,
                    WorkDate = workDate,
                    Id = workshiftExist.Id
                };
            }

            var newWorkshift = new Workshift
            {
                Id = Guid.NewGuid(),
                WorkDate = workDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = storeId.ToString(),
                StoreId = storeId,
            };

            await _workshiftRepository.AddAsync(newWorkshift);

            return new WorkshiftScheduleDto
            {
                StoreId = storeId,
                WorkDate = workDate,
                Id = newWorkshift.Id
            };
        }

        public async Task<WorkshiftScheduleDto?> GetScheduleByIdAsync(Guid scheduleId)
        {
            var workshiftExist = await _workshiftRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == scheduleId);

            ConditionCheck.CheckCondition(workshiftExist != null, Errors.Common.IsExist, "workdate");

            return new WorkshiftScheduleDto
            {
                StoreId = workshiftExist.StoreId,
                WorkDate = workshiftExist.WorkDate,
                Id = workshiftExist.Id
            };
        }

        public async Task<List<WorkshiftScheduleResponse>> GetSchedulesByStoreAsync(Guid storeId, DateTime? fromDate, DateTime? toDate)
        {
            var workshiftExist = await _workshiftRepository.AsQueryable().Include(x => x.WorkshiftSchedules).Where(x => x.StoreId == storeId && x.WorkDate > fromDate && x.WorkDate < toDate).ToListAsync();

            ConditionCheck.CheckCondition(workshiftExist.Any(), Errors.Common.NotFound);

            return workshiftExist.Select(x => new WorkshiftScheduleResponse
            {
                Id = x.Id,
                StoreId = x.StoreId,
                WorkDate = x.WorkDate,
                WorkShifts = x.WorkshiftSchedules.Select(y => new CreateWorkShiftDto
                {
                    Name = y.Name,
                    StartTime = y.StartTime,
                    EndTime = y.EndTime,
                }).ToList()
            }).ToList();
            
        }

        public async Task<bool> DeleteScheduleAsync(Guid scheduleId)
        {
            try
            {
                var workshiftExist = await _workshiftRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == scheduleId);

                ConditionCheck.CheckCondition(workshiftExist != null, Errors.Common.IsExist, "workdate");

                var allWorkshiftSchedule = await _workshiftScheduleRepository.AsQueryable().Where(x => x.WorkshiftId == scheduleId).ToListAsync();
                if (allWorkshiftSchedule != null)
                {
                    _workshiftScheduleRepository.RemoveRange(allWorkshiftSchedule);
                }

                _workshiftRepository.Remove(workshiftExist);
                await _workshiftRepository.SaveChangesAsync();

                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }

        // ========== Workshift (Shift) ==========
        public async Task<bool> AddWorkShiftToScheduleAsync(Guid workshiftId, CreateWorkShiftDto workShiftDto,string storeId)
        {
            try
            {
                var existingShifts = await _workshiftScheduleRepository.AsQueryable()
                    .Where(x => x.WorkshiftId == workshiftId)
                    .ToListAsync();

                ConditionCheck.CheckCondition(existingShifts != null, Errors.Common.NotFound);

                bool isOverlapping = existingShifts.Any(x =>
                    (workShiftDto.StartTime < x.EndTime && workShiftDto.EndTime > x.StartTime)
                );

                if (isOverlapping)
                    return false;

                var newWorkshiftSchedule = new WorkshiftSchedule
                {
                    Id = Guid.NewGuid(),
                    Name = workShiftDto.Name,
                    StartTime = workShiftDto.StartTime,
                    EndTime = workShiftDto.EndTime,
                    WorkshiftId = workshiftId,
                    StoreId = Guid.Parse(storeId),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = storeId,
                    IsActive = true
                };

                await _workshiftScheduleRepository.AddAsync(newWorkshiftSchedule);

                var newStaffRegister = new StaffWorkshiftRegistration
                {
                    Id = Guid.NewGuid(),
                    StaffId = (Guid)workShiftDto.StaffId,
                    StaffName = workShiftDto.Name,
                    Status = Common.Enums.WorkshiftStatus.Pending,
                    WorkshiftScheduleId = newWorkshiftSchedule.Id
                };

                await _staffWorkshiftRepository.AddAsync(newStaffRegister);
                await _workshiftScheduleRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task<List<WorkShiftDto>> GetWorkShiftsByScheduleAsync(Guid scheduleId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteWorkShiftAsync(Guid workShiftId)
        {
            try
            {
                var workshiftExist = await _workshiftScheduleRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == workShiftId);

                ConditionCheck.CheckCondition(workshiftExist != null, Errors.Common.IsExist, "id");

                _workshiftScheduleRepository.Remove(workshiftExist);
                await _workshiftScheduleRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<WorkshiftResponse?> GetWorkShiftByIdAsync(Guid workShiftId, string storeId)
        {
            var workshiftsQuery = await _workshiftRepository.AsQueryable()
                                .Include(x => x.WorkshiftSchedules)
                                    .ThenInclude(y => y.StaffWorkshiftRegistrations)
                                .FirstOrDefaultAsync(x => x.StoreId == Guid.Parse(storeId) && x.Id == workShiftId);

            ConditionCheck.CheckCondition(workshiftsQuery != null, Errors.Common.NotFound);

            var rs = new WorkshiftResponse
            {
                WorkDate = workshiftsQuery.WorkDate,
                Shift = workshiftsQuery.WorkshiftSchedules.Select(y => new StaffWorkshiftResponse
                {
                    StartTime = y.StartTime,
                    EndTime = y.EndTime,
                    StaffName = y.StaffWorkshiftRegistrations
                                   .Select(r => r.StaffName)
                                   .Where(name => !string.IsNullOrEmpty(name))
                                   .FirstOrDefault()
                }).ToList()
            };

            return rs;

        }


        // ========== Staff Registration ==========

        public async Task<List<StaffWorkshiftRegistrationResponse>> GetRegistrationsByStaffAsync(Guid staffId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _staffWorkshiftRepository.AsQueryable()
                .Include(x => x.WorkshiftSchedule)
                .ThenInclude(s => s.Workshift)
                .Where(x => x.StaffId == staffId);

            if (fromDate.HasValue)
                query = query.Where(x => x.WorkshiftSchedule.Workshift.WorkDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.WorkshiftSchedule.Workshift.WorkDate <= toDate.Value);

            var registrations = await query.ToListAsync();

            ConditionCheck.CheckCondition(registrations.Any(), Errors.Common.NotFound);

            var grouped = registrations
                .GroupBy(x => x.WorkshiftSchedule.Workshift.WorkDate)
                .Select(group => new StaffWorkshiftRegistrationResponse
                {
                    WorkDate = group.Key,
                    WorkShifts = group.Select(x => new CreateWorkShiftDto
                    {
                        Name = x.WorkshiftSchedule.Name,
                        StartTime = x.WorkshiftSchedule.StartTime,
                        EndTime = x.WorkshiftSchedule.EndTime
                    }).ToList()
                }).OrderBy(x => x.WorkDate)
                .ToList();

            return grouped;
        }

        public async Task<List<StaffWorkshiftRegistrationDto>> GetRegistrationsByWorkShiftAsync(Guid workShiftId)
        {
            var registrations = await _staffWorkshiftRepository.AsQueryable()
                .Where(x => x.WorkshiftSchedule.WorkshiftId == workShiftId)
                .ToListAsync();

            return registrations.Select(x => new StaffWorkshiftRegistrationDto
            {
                Id = x.Id,
                StaffId = x.StaffId,
                RegisteredAt = x.CreatedAt ?? DateTime.UtcNow,
                Status = x.Status
            }).ToList();
        }


        public async Task<StaffWorkshiftRegistrationDto> RegisterStaffToWorkShiftAsync(Guid staffId, Guid workShiftScheduleId)
        {
            var exists = await _staffWorkshiftRepository.AsQueryable()
                .AnyAsync(x => x.StaffId == staffId && x.WorkshiftScheduleId == workShiftScheduleId);

            ConditionCheck.CheckCondition(!exists, Errors.Common.IsExist, "Already registered");

            var registration = new StaffWorkshiftRegistration
            {
                Id = Guid.NewGuid(),
                StaffId = staffId,
                Status = Common.Enums.WorkshiftStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                WorkshiftScheduleId = workShiftScheduleId
            };

            await _staffWorkshiftRepository.AddAsync(registration);
            await _staffWorkshiftRepository.SaveChangesAsync();

            return new StaffWorkshiftRegistrationDto
            {
                Id = registration.Id,
                StaffId = registration.StaffId,
                Status = Common.Enums.WorkshiftStatus.Pending,
                RegisteredAt = registration.CreatedAt ?? DateTime.UtcNow
            };
        }


        public async Task<bool> ApproveRegistrationAsync(Guid registrationId)
        {
            var registration = await _staffWorkshiftRepository.GetByIdAsync(registrationId);
            ConditionCheck.CheckCondition(registration != null, Errors.Common.NotFound);

            registration!.Status = Common.Enums.WorkshiftStatus.Approved;
            await _staffWorkshiftRepository.SaveChangesAsync();
            return true;
        }

        public async Task BulkRegisterStaffToShiftsAsync(BulkRegisterRequest request)
        {
            //var currentDate = request.FromDate.Date;
            //var endDate = request.ToDate.Date;

            //var createdRegistrations = new List<StaffWorkshiftRegistration>();

            //while (currentDate <= endDate)
            //{
            //    var schedule = await _workshiftScheduleRepository.AsQueryable()
            //        .FirstOrDefaultAsync(x => x.StoreId == request.StoreId && x.Workshift.WorkDate == currentDate);

            //    if (schedule == null)
            //    {
            //        schedule = new WorkshiftSchedule
            //        {
            //            Id = Guid.NewGuid(),
            //            StoreId = request.StoreId,
            //            WorkDate = currentDate,
            //            CreatedAt = DateTime.UtcNow
            //        };
            //        await _workshiftScheduleRepository.AddAsync(schedule);
            //        await _workshiftScheduleRepository.SaveChangesAsync();
            //    }

            //    var workShift = await _workShiftRepository.AsQueryable()
            //        .FirstOrDefaultAsync(x => x.WorkshiftScheduleId == schedule.Id && x.Name == request.ShiftName);

            //    if (workShift == null)
            //        continue;

            //    var isRegistered = await _staffWorkshiftRepository.AsQueryable()
            //        .AnyAsync(x => x.StaffId == request.StaffId && x.WorkshiftId == workShift.Id);

            //    if (!isRegistered)
            //    {
            //        createdRegistrations.Add(new StaffWorkshiftRegistration
            //        {
            //            Id = Guid.NewGuid(),
            //            StaffId = request.StaffId,
            //            WorkshiftId = workShift.Id,
            //            RegisteredAt = DateTime.UtcNow,
            //            Status = "Pending"
            //        });
            //    }

            //    currentDate = currentDate.AddDays(1);
            //}

            //if (createdRegistrations.Any())
            //{
            //    await _staffWorkshiftRepository.AddRangeAsync(createdRegistrations);
            //    await _staffWorkshiftRepository.SaveChangesAsync();
            //}
        }


        public async Task<bool> CancelRegistrationAsync(Guid registrationId)
        {
            var registration = await _staffWorkshiftRepository.GetByIdAsync(registrationId);
            ConditionCheck.CheckCondition(registration != null, Errors.Common.NotFound);

            registration!.Status = Common.Enums.WorkshiftStatus.Reject;
            await _staffWorkshiftRepository.SaveChangesAsync();
            return true;
        }

    }
}
