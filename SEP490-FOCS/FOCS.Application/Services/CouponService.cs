using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Application.Services
{
    public class CouponService : ICouponService
    {
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<Coupon> _couponRepository;
        private readonly IMapper _mapper;

        public CouponService(IRepository<Promotion> proRepo, IRepository<Coupon> couponRepository, IMapper mapper)
        {
            _couponRepository = couponRepository;
            _promotionRepository = proRepo;
            _mapper = mapper;
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

        public async Task<PagedResult<CouponAdminDTO>> GetOngoingCouponsAsync(UrlQueryParameters query, string storeId)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(storeId, out Guid storeIdGuid),
                                                    Errors.Common.InvalidGuidFormat,
                                                    Errors.FieldName.StoreId);

            var couponQuery = _couponRepository.AsQueryable().Include(c => c.Promotion)
                                                                .Where(c => !c.IsDeleted && c.IsActive &&
                                                                c.StoreId == storeIdGuid && c.CountUsed < c.MaxUsage &&
                                                                c.StartDate < DateTime.UtcNow && c.EndDate > DateTime.UtcNow);

            // Search
            if (!string.IsNullOrEmpty(query.SearchBy) && !string.IsNullOrEmpty(query.SearchValue))
            {
                var value = query.SearchValue.ToLower();
                switch (query.SearchBy.ToLower())
                {
                    case "code":
                        couponQuery = couponQuery.Where(c => c.Code.ToLower().Contains(value));
                        break;
                    case "description":
                        couponQuery = couponQuery.Where(c => c.Description.ToLower().Contains(value));
                        break;
                    case "discounttype":
                        if (Enum.TryParse<DiscountType>(query.SearchValue, true, out var type))
                            couponQuery = couponQuery.Where(c => c.DiscountType == type);
                        break;
                }
            }

            // Filters
            if (query.Filters != null)
            {
                foreach (var filter in query.Filters)
                {
                    var key = filter.Key.ToLowerInvariant();
                    var value = filter.Value;
                    switch (key)
                    {
                        case "discount_type":
                            if (Enum.TryParse<DiscountType>(value, true, out var discountType))
                                couponQuery = couponQuery.Where(c => c.DiscountType == discountType);
                            break;
                        case "is_active":
                            if (bool.TryParse(value, out var isActive))
                                couponQuery = couponQuery.Where(c => c.IsActive == isActive);
                            break;
                        case "start_date":
                            if (DateTime.TryParse(value, out var startDate))
                                couponQuery = couponQuery.Where(c => c.StartDate >= startDate);
                            break;
                        case "end_date":
                            if (DateTime.TryParse(value, out var endDate))
                                couponQuery = couponQuery.Where(c => c.EndDate <= endDate);
                            break;
                        case "status":
                            if (Enum.TryParse<CouponStatus>(value, true, out var couponStatus))
                            {
                                var now = DateTime.UtcNow;
                                couponQuery = couponStatus switch
                                {
                                    CouponStatus.UnAvailable => couponQuery.Where(c => !c.IsActive || c.CountUsed >= c.MaxUsage),
                                    CouponStatus.Incomming => couponQuery.Where(c => c.IsActive && c.CountUsed < c.MaxUsage && c.StartDate > now),
                                    CouponStatus.On_going => couponQuery.Where(c => c.IsActive && c.CountUsed < c.MaxUsage && c.StartDate <= now && c.EndDate >= now),
                                    CouponStatus.Expired => couponQuery.Where(c => c.IsActive && c.EndDate < now),
                                    _ => couponQuery
                                };
                            }
                            break;
                        case "promotion_id":
                            if (Guid.TryParse(value, out var promoId))
                                couponQuery = couponQuery.Where(c => c.PromotionId == promoId);
                            break;
                        case "promotion_status":
                            if (Enum.TryParse<CouponByPromotionStatus>(value, true, out var promoStatus))
                            {
                                couponQuery = promoStatus switch
                                {
                                    CouponByPromotionStatus.UnAvailable => couponQuery
                                        .Where(c => c.Promotion != null && (!c.Promotion.IsActive || c.Promotion.IsDeleted)),
                                    CouponByPromotionStatus.InPromotionDuration => couponQuery
                                        .Where(c => c.Promotion != null && c.Promotion.IsActive && !c.Promotion.IsDeleted &&
                                                    c.Promotion.StartDate <= c.StartDate &&
                                                    c.EndDate <= c.Promotion.EndDate),
                                    _ => couponQuery
                                };
                            }
                            break;
                    }
                }
            }

            // Sort
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                bool desc = query.SortOrder?.ToLower() == "desc";
                couponQuery = query.SortBy.ToLower() switch
                {
                    "code" => desc ? couponQuery.OrderByDescending(c => c.Code) : couponQuery.OrderBy(c => c.Code),
                    "value" => desc ? couponQuery.OrderByDescending(c => c.Value) : couponQuery.OrderBy(c => c.Value),
                    "start_date" => desc ? couponQuery.OrderByDescending(c => c.StartDate) : couponQuery.OrderBy(c => c.StartDate),
                    "end_date" => desc ? couponQuery.OrderByDescending(c => c.EndDate) : couponQuery.OrderBy(c => c.EndDate),
                    "is_active" => desc ? couponQuery.OrderByDescending(c => c.IsActive) : couponQuery.OrderBy(c => c.IsActive),
                    _ => couponQuery.OrderBy(c => c.StartDate)
                };
            }

            // Pagination
            var total = await couponQuery.CountAsync();
            var items = await couponQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<CouponAdminDTO>>(items);
            return new PagedResult<CouponAdminDTO>(mapped, total, query.Page, query.PageSize);
        }

    }
}
