using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class CouponService : ICouponService
    {
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<Coupon> _couponRepository;

        public CouponService(IRepository<Promotion> proRepo, IRepository<Coupon> couponRepository)
        {
            _couponRepository = couponRepository;
            _promotionRepository = proRepo;
        }

        public async Task IsValidApplyCouponAsync(string couponCode)
        {
            ConditionCheck.CheckCondition(couponCode != null, Errors.Common.Empty);
            
            var coupon = await _couponRepository.FindAsync(x => x.Code == couponCode);
            ConditionCheck.CheckCondition(coupon != null && coupon.Count() > 1, Errors.PromotionError.CouponNotFound);
            var couponExist = coupon.FirstOrDefault();
            var currentDate = DateTime.Now;
            ConditionCheck.CheckCondition(couponExist?.CountUsed >= couponExist?.MaxUsage, Errors.PromotionError.CouponMaxUsed);

            //Validate max usage per user by store setting

            //Check period time
            ConditionCheck.CheckCondition(couponExist.StartDate < currentDate && couponExist.EndDate > currentDate, Errors.PromotionError.InvalidPeriodDatetime);
            ConditionCheck.CheckCondition(couponExist.Promotion.StartDate < currentDate && couponExist.Promotion.EndDate > currentDate, Errors.PromotionError.InvalidPeriodDatetime);
        }
    }
}
