using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Application.Services
{
    public class StoreSettingService : IStoreSettingService
    {
        private readonly IRepository<StoreSetting> _storeSettingRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public StoreSettingService(IRepository<StoreSetting> storeSettingRepository, UserManager<User> userManager, IMapper mapper)
        {
            _storeSettingRepository = storeSettingRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<StoreSettingDTO> GetStoreSettingAsync(Guid storeId, string userId)
        {
            await ValidateUser(userId, storeId);
            var storeSetting = await _storeSettingRepository.AsQueryable()
                .Where(s => s.StoreId.Equals(storeId) && s.IsDeleted == false).FirstOrDefaultAsync();
            ConditionCheck.CheckCondition(storeSetting != null, Errors.StoreSetting.StoreSettingNotFound);

            return _mapper.Map<StoreSettingDTO>(storeSetting);
        }

        public async Task<StoreSettingDTO> CreateStoreSettingAsync(Guid storeId, StoreSettingDTO dto, string userId)
        {
            await ValidateUser(userId, storeId);
            var storeSetting = await _storeSettingRepository.AsQueryable()
                .Where(s => s.StoreId.Equals(storeId) && s.IsDeleted == false).FirstOrDefaultAsync();
            ConditionCheck.CheckCondition(storeSetting == null, Errors.StoreSetting.SettingExist);

            var newStoreSetting = _mapper.Map<StoreSetting>(dto);
            newStoreSetting.Id = Guid.NewGuid();
            newStoreSetting.CreatedAt = DateTime.UtcNow;
            newStoreSetting.CreatedBy = userId;
            newStoreSetting.StoreId = storeId;

            await _storeSettingRepository.AddAsync(newStoreSetting);
            await _storeSettingRepository.SaveChangesAsync();

            return _mapper.Map<StoreSettingDTO>(newStoreSetting);
        }

        public async Task<bool> UpdateStoreSettingAsync(Guid storeId, StoreSettingDTO dto, string userId)
        {
            await ValidateUser(userId, storeId);
            var storeSetting = await _storeSettingRepository.AsQueryable()
                .Where(s => s.StoreId.Equals(storeId) && s.IsDeleted == false).FirstOrDefaultAsync();
            if (storeSetting == null || storeSetting.IsDeleted)
                return false;

            _mapper.Map(dto, storeSetting);
            storeSetting.UpdatedAt = DateTime.UtcNow;
            storeSetting.UpdatedBy = userId;

            await _storeSettingRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetStoreSettingAsync(Guid storeId, string userId)
        {
            var storeSetting = await _storeSettingRepository.AsQueryable()
                .Where(s => s.StoreId.Equals(storeId) && s.IsDeleted == false).FirstOrDefaultAsync();
            if (storeSetting == null || storeSetting.IsDeleted)
                return false;

            storeSetting.OpenTime = new TimeSpan(0, 0, 0);
            storeSetting.CloseTime = new TimeSpan(23, 59, 59);
            storeSetting.Currency = "VND";
            storeSetting.PaymentConfig = PaymentConfig.Momo;
            storeSetting.LogoUrl = "";
            storeSetting.IsSelfService = true;
            storeSetting.discountStrategy = DiscountStrategy.CouponThenPromotion;
            storeSetting.UpdatedAt = DateTime.UtcNow;
            storeSetting.UpdatedBy = userId;

            await _storeSettingRepository.SaveChangesAsync();
            return true;
        }

        #region Private Helper Methods

        private async Task ValidateUser(string userId, Guid storeId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound);
            ConditionCheck.CheckCondition(user.StoreId == storeId, Errors.AuthError.UserUnauthor);
        }

        #endregion
    }
}
