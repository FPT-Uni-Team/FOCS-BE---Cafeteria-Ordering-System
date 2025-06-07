using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Order.Infrastucture.Migrations;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Application.Services
{
    public class StoreSettingService : IStoreSettingService
    {
        private readonly IRepository<StoreSetting> _storeSettingRepository;
        private readonly IMapper _mapper;

        public StoreSettingService(IRepository<StoreSetting> storeSettingRepository, IMapper mapper)
        {
            _storeSettingRepository = storeSettingRepository;
            _mapper = mapper;
        }

        public async Task<StoreSettingDTO> GetStoreSettingAsync(Guid storeId)
        {
            var storeSetting = await _storeSettingRepository.AsQueryable()
                .Where(s => s.StoreId.Equals(storeId) && s.IsDeleted == false).FirstOrDefaultAsync();
            return _mapper.Map<StoreSettingDTO>(storeSetting);
        }

        public async Task<bool> UpdateStoreSettingAsync(Guid storeId, StoreSettingDTO dto, string userId)
        {
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
            storeSetting.logoUrl = "";
            storeSetting.IsSelfService = true;
            storeSetting.discountStrategy = DiscountStrategy.CouponThenPromotion;
            storeSetting.UpdatedAt = DateTime.UtcNow;
            storeSetting.UpdatedBy = userId;

            await _storeSettingRepository.SaveChangesAsync();
            return true;
        }
    }
}
