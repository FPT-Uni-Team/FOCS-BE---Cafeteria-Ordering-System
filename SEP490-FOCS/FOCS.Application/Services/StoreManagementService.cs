using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Application.Services
{
    public class StoreManagementService : IStoreManagementService
    {
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<StoreSetting> _storeSettingRepository;
        private readonly IMapper _mapper;

        public StoreManagementService(IRepository<Store> storeRepository, IRepository<StoreSetting> storeSettingRepository, IMapper mapper)
        {
            _storeRepository = storeRepository;
            _storeSettingRepository = storeSettingRepository;
            _mapper = mapper;
        }

        public async Task<StoreAdminServiceDTO> CreateStoreAsync(StoreAdminServiceDTO dto, string userId)
        {
            var newStore = _mapper.Map<Store>(dto);
            newStore.Id = Guid.NewGuid();
            newStore.IsDeleted = false;
            newStore.CreatedAt = DateTime.UtcNow;
            newStore.CreatedBy = userId;

            await _storeRepository.AddAsync(newStore);
            await _storeRepository.SaveChangesAsync();

            var defaultSetting = new StoreSetting
            {
                StoreId = newStore.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = userId,
            };

            await _storeSettingRepository.AddAsync(defaultSetting);
            await _storeSettingRepository.SaveChangesAsync();

            return _mapper.Map<StoreAdminServiceDTO>(newStore);
        }

        public async Task<PagedResult<StoreAdminServiceDTO>> GetAllStoresAsync(UrlQueryParameters query)
        {
            var storeQuery = _storeRepository.AsQueryable().Where(s => !s.IsDeleted);

            // Search
            if (!string.IsNullOrEmpty(query.SearchBy) && !string.IsNullOrEmpty(query.SearchValue))
            {
                var searchValue = query.SearchValue.ToLower();

                storeQuery = query.SearchBy.ToLower() switch
                {
                    "name" => storeQuery.Where(s => s.Name.ToLower().Contains(searchValue)),
                    "address" => storeQuery.Where(s => s.Address.ToLower().Contains(searchValue)),
                    "phonenumber" => storeQuery.Where(s => s.PhoneNumber.ToLower().Contains(searchValue)),
                    _ => storeQuery
                };
            }

            // Sort
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                bool desc = query.SortOrder?.ToLower() == "desc";

                storeQuery = query.SortBy.ToLower() switch
                {
                    "name" => desc ? storeQuery.OrderByDescending(s => s.Name) : storeQuery.OrderBy(s => s.Name),
                    "address" => desc ? storeQuery.OrderByDescending(s => s.Address) : storeQuery.OrderBy(s => s.Address),
                    "phonenumber" => desc ? storeQuery.OrderByDescending(s => s.PhoneNumber) : storeQuery.OrderBy(s => s.PhoneNumber),
                    "customtaxrate" => desc ? storeQuery.OrderByDescending(s => s.CustomTaxRate) : storeQuery.OrderBy(s => s.CustomTaxRate),
                    _ => storeQuery
                };
            }

            // Pagination
            var total = await storeQuery.CountAsync();
            var items = await storeQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<StoreAdminServiceDTO>>(items);
            return new PagedResult<StoreAdminServiceDTO>(mapped, total, query.Page, query.PageSize);
        }

        public async Task<bool> UpdateStoreAsync(Guid id, StoreAdminServiceDTO dto, string userId)
        {
            var store = await _storeRepository.GetByIdAsync(id);
            if (store == null || store.IsDeleted)
                return false;

            _mapper.Map(dto, store);
            store.UpdatedAt = DateTime.UtcNow;
            store.UpdatedBy = userId;

            await _storeRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStoreAsync(Guid id, string userId)
        {
            var store = await _storeRepository.GetByIdAsync(id);
            if (store == null || store.IsDeleted)
                return false;

            store.IsDeleted = true;
            store.UpdatedAt = DateTime.UtcNow;
            store.UpdatedBy = userId;

            await _storeRepository.SaveChangesAsync();
            return true;
        }
    }
}
