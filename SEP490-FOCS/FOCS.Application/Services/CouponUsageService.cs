using FOCS.Common.Interfaces;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CouponEntity = FOCS.Order.Infrastucture.Entities.Coupon;

namespace FOCS.Application.Services
{
    public class CouponUsageService : ICouponUsageService
    {
        private readonly IRepository<CouponUsage> _couponUsageService;

        private readonly IRepository<CouponEntity> _couponRepo;

        public CouponUsageService(IRepository<CouponUsage> couponUsageService, IRepository<CouponEntity> couponRepo)
        {
            _couponUsageService= couponUsageService;
            _couponRepo = couponRepo;
        }
        public async Task<bool> SaveCouponUsage(string couponCode, Guid userId, Guid orderId)
        {
            try
            {
                var currentCouponCode = (await _couponRepo.AsQueryable().FirstOrDefaultAsync(x => x.Code == couponCode))!.Id;

                var couponUsage = new CouponUsage
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    OrderId = orderId,
                    CouponId = currentCouponCode,
                    UsedAt = DateTime.UtcNow
                };

                await _couponUsageService.AddAsync(couponUsage);
                await _couponUsageService.SaveChangesAsync();

                return true;
            } catch(Exception ex)
            {
                return false;
            }
        }
    }
}
