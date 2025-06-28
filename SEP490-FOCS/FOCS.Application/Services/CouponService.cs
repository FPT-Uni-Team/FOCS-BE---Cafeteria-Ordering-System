using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Utilities.IO;
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

        public async Task IsValidApplyCouponAsync(string couponCode, Guid storeId)
        {
            ConditionCheck.CheckCondition(!string.IsNullOrWhiteSpace(couponCode), Errors.Common.Empty);

            var couponList = await _couponRepository.FindAsync(x => x.Code == couponCode && x.StoreId == storeId);
            ConditionCheck.CheckCondition(couponList.Any(), Errors.PromotionError.CouponNotFound);

            var coupon = couponList.First();

            ConditionCheck.CheckCondition(coupon.CountUsed < coupon.MaxUsage, Errors.PromotionError.CouponMaxUsed);

            var currentDate = DateTime.UtcNow;
            ConditionCheck.CheckCondition(currentDate >= coupon.StartDate && currentDate <= coupon.EndDate, Errors.PromotionError.InvalidPeriodDatetime);
        }

    }
}
