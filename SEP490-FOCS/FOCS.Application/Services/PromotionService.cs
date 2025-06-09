using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<Coupon> _couponRepository;

        private readonly IRepository<CouponUsage> _couponUsageRepository;

        public PromotionService(IRepository<Promotion> proRepo, IRepository<CouponUsage> couponUsageRepository, IRepository<Coupon> couponRepository)
        {
            _couponRepository = couponRepository;
            _promotionRepository = proRepo;
            _couponRepository = couponRepository;
        }

        public async Task IsValidPromotionCouponAsync(string couponCode, string userId, Guid storeId)
        {
            //Check coupon eligible
            ConditionCheck.CheckCondition(!string.IsNullOrWhiteSpace(couponCode), Errors.Common.Empty);

            var couponList = await _couponRepository.FindAsync(x => x.Code == couponCode && x.StoreId == storeId);
            ConditionCheck.CheckCondition(couponList.Any(), Errors.PromotionError.CouponNotFound);

            var coupon = couponList.First();

            ConditionCheck.CheckCondition(coupon.CountUsed < coupon.MaxUsage, Errors.PromotionError.CouponMaxUsed);

            //Validate max use per user
            var couponUsageTime = await _couponUsageRepository.FindAsync(x => x.UserId == Guid.Parse(userId) && x.CouponId == Guid.Parse(couponCode));
            ConditionCheck.CheckCondition(couponUsageTime.Count() <= coupon.MaxUsagePerUser, Errors.PromotionError.CouponMaxUsed);

            var currentDate = DateTime.Now;
            ConditionCheck.CheckCondition(currentDate >= coupon.StartDate && currentDate <= coupon.EndDate, Errors.PromotionError.InvalidPeriodDatetime);

            //Check promotion eligible
            if (coupon.PromotionId.HasValue)
            {
                var promotion = await _promotionRepository.FindAsync(x => x.Id == coupon.PromotionId && x.StoreId == storeId);
                ConditionCheck.CheckCondition(promotion != null, Errors.PromotionError.PromotionNotFound);
                var currentPromotion = promotion?.FirstOrDefault();
                ConditionCheck.CheckCondition(currentDate >= currentPromotion?.StartDate && currentDate <= currentPromotion.EndDate, Errors.PromotionError.InvalidPeriodDatetime);
            }
        }
    }
}
