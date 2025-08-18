using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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

        public async Task<PagedResult<CouponUsageResponse>> GetList(UrlQueryParameters urlQuery, string storeId)
        {
            var couponUsageInStore = _couponUsageService.AsQueryable().Include(x => x.Order).Include(x => x.Coupon).Where(x => x.Order.StoreId == Guid.Parse(storeId));

            if(!string.IsNullOrEmpty(urlQuery.SearchBy) && urlQuery.SearchBy != null)
            {
                couponUsageInStore = urlQuery.SearchBy switch
                {
                    "order_code" => couponUsageInStore.Where(x => x.Order.OrderCode.ToString() == urlQuery.SearchValue),
                    "coupon_code" => couponUsageInStore.Where(x => x.Coupon.Code.ToString() == urlQuery.SearchValue),
                    _ => couponUsageInStore
                };
            }
            var rs = await couponUsageInStore.Skip((urlQuery.Page - 1) * urlQuery.PageSize)
                                        .Take(urlQuery.PageSize)
                                        .ToListAsync();

            return new PagedResult<CouponUsageResponse>(rs.Select(x => new CouponUsageResponse
            {
                CouponCode = x.Coupon.Code,
                OrderCode = x.Order.OrderCode.ToString(),
                ActorId = x.UserId.ToString(),
                UsedAt = x.UsedAt
            }).ToList(), couponUsageInStore.Count(), urlQuery.Page, urlQuery.PageSize);
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
