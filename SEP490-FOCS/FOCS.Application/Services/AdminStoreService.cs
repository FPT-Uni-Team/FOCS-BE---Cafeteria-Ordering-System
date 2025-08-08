using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Common.Models.Payment;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using System.Runtime.CompilerServices;

namespace FOCS.Application.Services
{
    public class AdminStoreService : IAdminStoreService
    {
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<StoreSetting> _storeSettingRepository;
        private readonly IRepository<Brand> _brandRepository;
        private readonly IRepository<PaymentAccount> _paymentAccountRepository;
        private readonly IMapper _mapper;

        private readonly IDataProtector _dataProtector;

        public AdminStoreService(IRepository<Store> storeRepository, IRepository<PaymentAccount> paymentAccountRepository, IRepository<StoreSetting> storeSettingRepository, IMapper mapper, IDataProtectionProvider dataProtector, IRepository<Brand> brandRepository)
        {
            _paymentAccountRepository = paymentAccountRepository;
            _storeRepository = storeRepository;
            _storeSettingRepository = storeSettingRepository;
            _mapper = mapper;
            _dataProtector = dataProtector.CreateProtector("PayOS.Protection");
            _brandRepository = brandRepository;
        }

        public async Task<StoreAdminDTO> GetById(Guid id)
        {
            var store = await _storeRepository.GetByIdAsync(id);

            ConditionCheck.CheckCondition(store != null, Errors.Common.NotFound);

            return _mapper.Map<StoreAdminDTO>(store);
        }
        public async Task<StoreAdminDTO> CreateStoreAsync(StoreAdminDTO dto, string userId)
        {
            await CheckValidInput(userId, dto.BrandId);

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

            return _mapper.Map<StoreAdminDTO>(newStore);
        }

        public async Task<bool> CreatePaymentAsync(CreatePaymentRequest request, string storeId)
        {
            try
            {
                var exist = await _paymentAccountRepository.AsQueryable().AnyAsync(x => x.BankName == request.BankName && x.AccountNumber == request.AccountNumber);

                ConditionCheck.CheckCondition(!exist, Errors.Common.IsExist);

                var newPayment = new PaymentAccount
                {
                    Id = Guid.NewGuid(),
                    BankCode = request.BankCode,
                    AccountName = request.AccountName,
                    AccountNumber = request.AccountNumber,
                    BankName = request.BankName,
                    StoreId = Guid.Parse(storeId),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _paymentAccountRepository.AddAsync(newPayment);
                await _paymentAccountRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<StoreAdminResponse> GetStoreSetting(Guid id)
        {
            var store = await _storeSettingRepository.AsQueryable().Include(x => x.Store).FirstOrDefaultAsync(x => x.StoreId == id);

            var dataStoreSetting = _mapper.Map<StoreAdminResponse>(store);

            var protector = new SecretProtector(_dataProtector);

            dataStoreSetting.PayOSClientId = protector.Decrypt(dataStoreSetting.PayOSClientId);
            dataStoreSetting.PayOSApiKey = protector.Decrypt(dataStoreSetting.PayOSApiKey);
            dataStoreSetting.PayOSChecksumKey = protector.Decrypt(dataStoreSetting.PayOSChecksumKey);

            return dataStoreSetting;
        }

        public async Task<bool> UpdateConfigPayment(UpdateConfigPaymentRequest request, string storeId)
        {
            try
            {
                var storeSetting = await _storeSettingRepository
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.StoreId == Guid.Parse(storeId));

                if (storeSetting == null)
                {
                    return false;
                }

                var secretProtector = new SecretProtector(_dataProtector);

                storeSetting.PayOSClientId = secretProtector.Encrypt(request.PayOSClientId);
                storeSetting.PayOSApiKey = secretProtector.Encrypt(request.PayOSApiKey);
                storeSetting.PayOSChecksumKey = secretProtector.Encrypt(request.PayOSChecksumKey);

                _storeSettingRepository.Update(storeSetting);
                await _storeSettingRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<PagedResult<StoreAdminDTO>> GetAllStoresAsync(UrlQueryParameters query, string userId)
        {
            await CheckValidInput(userId);
            var storeQuery = _storeRepository.AsQueryable().Include(i => i.Brand).Where(s => !s.IsDeleted && s.Brand.CreatedBy.Equals(userId));

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

            if (query.Filters?.Any() == true)
            {
                foreach (var (key, value) in query.Filters)
                {
                    var filterValue = value.ToLower();
                    storeQuery = key.ToLowerInvariant() switch
                    {
                        "brand_id" => storeQuery.Where(p => p.Brand.Id.ToString().Equals(filterValue)),
                        _ => storeQuery
                    };
                }
            }

            // Pagination
            var total = await storeQuery.CountAsync();
            var items = await storeQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<StoreAdminDTO>>(items);
            return new PagedResult<StoreAdminDTO>(mapped, total, query.Page, query.PageSize);
        }

        public async Task<bool> UpdateStoreAsync(Guid id, StoreAdminDTO dto, string userId)
        {
            await CheckValidInput(userId, dto.BrandId);
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
            await CheckValidInput(userId);
            var store = await _storeRepository.GetByIdAsync(id);
            if (store == null || store.IsDeleted)
                return false;

            store.IsDeleted = true;
            store.UpdatedAt = DateTime.UtcNow;
            store.UpdatedBy = userId;

            await _storeRepository.SaveChangesAsync();
            return true;
        }

        public async Task CheckValidInput(string userId, Guid? brandId = null)
        {
            //check userId is not null or empty
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId is required(Please login).");
            }

            if (brandId.HasValue)
            {
                var brand = await _brandRepository.GetByIdAsync(brandId);
                ConditionCheck.CheckCondition(brand != null, Errors.Common.BrandNotFound, Errors.FieldName.BrandId);
            }
        }
    }
}
