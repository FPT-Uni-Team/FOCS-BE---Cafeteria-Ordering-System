using AutoMapper;
using FOCS.Application.DTOs.AdminDTO;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FOCS.Application.Services
{
    public class AdminCouponService : IAdminCouponService
    {
        private readonly IRepository<Coupon> _couponRepository;
        private readonly IRepository<CouponUsage> _couponUsageRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IMapper _mapper;

        private readonly ILogger<Coupon> _logger;

        public AdminCouponService(IRepository<Coupon> couponRepository, IRepository<CouponUsage> couponUsageRepository, ILogger<Coupon> logger, IRepository<Promotion> promotionRepository, IMapper mapper)
        {
            _couponRepository = couponRepository;
            _couponUsageRepository = couponUsageRepository;
            _logger = logger;
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<CouponAdminDTO> CreateCouponAsync(CouponAdminDTO dto, string userId, string storeId)
        {
            // Check userId empty
            ConditionCheck.CheckCondition(!string.IsNullOrEmpty(userId), AdminCouponConstants.UserIdEmpty);

            // Check coupon code type
            ConditionCheck.CheckCondition(dto.CouponType == CouponType.Manual
                                                     || dto.CouponType == CouponType.AutoGenerate,
                                                        AdminCouponConstants.CheckCouponCodeType);

            // Type 'auto' => Generate unique code
            string couponCode = dto.CouponType == CouponType.AutoGenerate ? await GenerateUniqueCouponCodeAsync()
                                                                         : dto.Code?.Trim() ?? "";

            // Check manual code empty
            ConditionCheck.CheckCondition(dto.CouponType != CouponType.AutoGenerate
                                                     || !string.IsNullOrWhiteSpace(couponCode),
                                                        AdminCouponConstants.CheckCouponCodeForManual);

            // Check unique code
            var existing = await _couponRepository.AsQueryable()
                                                  .AnyAsync(c => c.Code == couponCode && !c.IsDeleted);
            ConditionCheck.CheckCondition(!existing, AdminCouponConstants.CheckCreateUniqueCode);

            // Check dates
            ConditionCheck.CheckCondition(dto.StartDate < dto.EndDate, AdminCouponConstants.CheckCreateDate);

            // Check promotion Id
            if (dto.PromotionId.HasValue)
            {
                var existingPromotion = await _promotionRepository.AsQueryable()
                                                  .AnyAsync(c => c.Id == dto.PromotionId && !c.IsDeleted);
                ConditionCheck.CheckCondition(existingPromotion, AdminCouponConstants.CheckPromotion);
            }

            // Map DTO to entity
            var newCoupon = _mapper.Map<Coupon>(dto);
            newCoupon.Id = Guid.NewGuid();

            newCoupon.MinimumItemQuantity = dto.SetCouponConditionRequest.ConditionType switch
            {
                CouponConditionType.MinItemsQuantity => dto.SetCouponConditionRequest.Value,
                _ => null
            };

            newCoupon.MinimumOrderAmount = dto.SetCouponConditionRequest.ConditionType switch
            {
                CouponConditionType.MinOrderAmount => dto.SetCouponConditionRequest.Value,
                _ => null
            };

            newCoupon.Code = couponCode;
            newCoupon.StoreId = Guid.Parse(storeId);
            newCoupon.IsDeleted = false;
            newCoupon.CreatedAt = DateTime.UtcNow;
            newCoupon.CreatedBy = userId;

            await _couponRepository.AddAsync(newCoupon);
            await _couponRepository.SaveChangesAsync();

            return _mapper.Map<CouponAdminDTO>(newCoupon);
        }

        public async Task SetCouponConditionAsync(Guid couponId, SetCouponConditionRequest setCouponConditionRequest)
        {
            var coupon = await _couponRepository.GetByIdAsync(couponId);
            ConditionCheck.CheckCondition(coupon != null, FOCS.Common.Exceptions.Errors.Common.NotFound);

            try
            {
                coupon.MinimumItemQuantity = setCouponConditionRequest.ConditionType switch
                {
                    CouponConditionType.MinItemsQuantity => setCouponConditionRequest.Value,
                    _ => null
                };

                coupon.MinimumOrderAmount = setCouponConditionRequest.ConditionType switch
                {
                    CouponConditionType.MinOrderAmount => setCouponConditionRequest.Value,
                    _ => null
                };

                _couponRepository.Update(coupon);
                await _couponRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task<string> GenerateUniqueCouponCodeAsync()
        {
            const string chars = AdminCouponConstants.GenerateUniqueCouponCode;
            var random = new Random();

            string newCode;
            bool exists;

            do
            {
                newCode = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                exists = await _couponRepository.AsQueryable()
                    .AnyAsync(c => c.Code == newCode && !c.IsDeleted);

            } while (exists);

            return newCode;
        }

        public async Task<PagedResult<CouponAdminDTO>> GetAllCouponsAsync(UrlQueryParameters query, Guid storeId, string userId)
        {
            // Check userId is valid
            ConditionCheck.CheckCondition(!string.IsNullOrEmpty(userId), AdminCouponConstants.UserIdEmpty);
            ConditionCheck.CheckCondition(storeId != null, Errors.Common.StoreNotFound);

            var couponQuery = _couponRepository.AsQueryable().Include(c => c.Promotion).Where(c => !c.IsDeleted && c.StoreId == storeId);

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
                    "startdate" => desc ? couponQuery.OrderByDescending(c => c.StartDate) : couponQuery.OrderBy(c => c.StartDate),
                    "enddate" => desc ? couponQuery.OrderByDescending(c => c.EndDate) : couponQuery.OrderBy(c => c.EndDate),
                    "isactive" => desc ? couponQuery.OrderByDescending(c => c.IsActive) : couponQuery.OrderBy(c => c.IsActive),
                    _ => couponQuery
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

        public async Task<PagedResult<CouponAdminDTO>> GetAvailableCouponsAsync(UrlQueryParameters query, Guid storeId, string userId)
        {
            // Check userId is valid
            ConditionCheck.CheckCondition(!string.IsNullOrEmpty(userId), AdminCouponConstants.UserIdEmpty);
            ConditionCheck.CheckCondition(storeId != null, Errors.Common.StoreNotFound);

            var couponQuery = _couponRepository.AsQueryable().Include(c => c.Promotion)
                                                                .Where(c => !c.IsDeleted &&
                                                                c.StoreId == storeId &&
                                                                c.PromotionId == null &&
                                                                c.CountUsed < c.MaxUsage &&
                                                                c.IsActive && c.StartDate > DateTime.UtcNow);

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
                    "startdate" => desc ? couponQuery.OrderByDescending(c => c.StartDate) : couponQuery.OrderBy(c => c.StartDate),
                    "enddate" => desc ? couponQuery.OrderByDescending(c => c.EndDate) : couponQuery.OrderBy(c => c.EndDate),
                    "isactive" => desc ? couponQuery.OrderByDescending(c => c.IsActive) : couponQuery.OrderBy(c => c.IsActive),
                    _ => couponQuery
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

        public async Task<CouponAdminDTO> GetCouponByIdAsync(Guid couponId, string userId)
        {
            var coupon = await _couponRepository
                                .AsQueryable()
                                .FirstOrDefaultAsync(c => c.Id == couponId && !c.IsDeleted);

            if (coupon == null)
                return null;

            return _mapper.Map<CouponAdminDTO>(coupon);
        }

        public async Task<bool> UpdateCouponAsync(Guid id, CouponAdminDTO dto, string userId, string storeId)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null || coupon.IsDeleted)
                return false;

            // Check unique code (exclude current coupon)
            var existing = await _couponRepository.AsQueryable()
                                                  .AnyAsync(c => c.Id != id && c.Code == dto.Code && !c.IsDeleted);
            ConditionCheck.CheckCondition(!existing, AdminCouponConstants.CheckUpdateUniqueCode);

            // Check dates
            ConditionCheck.CheckCondition(dto.StartDate <= dto.EndDate, AdminCouponConstants.CheckUpdateDate);

            _mapper.Map(dto, coupon);
            coupon.StoreId = Guid.Parse(storeId);
            coupon.UpdatedAt = DateTime.UtcNow;
            coupon.UpdatedBy = userId;

            await _couponRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCouponAsync(Guid id, string userId)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null || coupon.IsDeleted)
                return false;

            coupon.IsDeleted = true;
            coupon.UpdatedAt = DateTime.UtcNow;
            coupon.UpdatedBy = userId;

            await _couponRepository.SaveChangesAsync();
            return true;
        }

        public async Task<TrackCouponUsageDTO?> TrackCouponUsageAsync(Guid couponId, Guid? userId)
        {
            var coupon = await _couponRepository.GetByIdAsync(couponId);
            // If coupon unavailable
            if (coupon == null || coupon.IsDeleted || !coupon.IsActive) return null;

            var now = DateTime.UtcNow;
            // If coupon not in date range
            if (now < coupon.StartDate || now > coupon.EndDate) return null;

            var usageCount = await _couponUsageRepository.AsQueryable().CountAsync(u => u.CouponId == couponId);
            // If coupon usage exceeded
            if (usageCount >= coupon.MaxUsage) return null;

            int? leftPerUser = null;
            if (userId.HasValue && coupon.MaxUsagePerUser.HasValue)
            {
                var userUsage = await _couponUsageRepository.AsQueryable()
                            .CountAsync(u => u.CouponId == couponId && u.UserId == userId.Value);

                if (userUsage >= coupon.MaxUsagePerUser.Value)
                    leftPerUser = 0;
                else
                    leftPerUser = coupon.MaxUsagePerUser.Value - userUsage;
            }

            return new TrackCouponUsageDTO
            {
                TotalLeft = coupon.MaxUsage - usageCount,
                LeftPerUser = leftPerUser
            };
        }

        public async Task<bool> SetCouponStatusAsync(Guid couponId, bool isActive, string userId)
        {
            var coupon = await _couponRepository.GetByIdAsync(couponId);
            if (coupon == null || coupon.IsDeleted)
                return false;

            coupon.IsActive = isActive;
            coupon.UpdatedAt = DateTime.UtcNow;
            coupon.UpdatedBy = userId;

            await _couponRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignCouponsToPromotionAsync(List<Guid> couponIds, Guid promotionId, string userId, Guid storeId)
        {
            // Get promotion
            var promotion = await _promotionRepository.GetByIdAsync(promotionId);
            ConditionCheck.CheckCondition(promotion != null && !promotion.IsDeleted, AdminCouponConstants.CheckPromotion);

            // Get coupons
            var coupons = await _couponRepository.FindAsync(c => couponIds.Contains(c.Id) && !c.IsDeleted);

            foreach (var coupon in coupons)
            {
                // Check exists store
                ConditionCheck.CheckCondition(coupon.StoreId == storeId, $"Coupon {coupon.Code} does not belong to this store.");

                // Check coupon dates within promotion dates
                ConditionCheck.CheckCondition(coupon.StartDate >= promotion.StartDate
                                           && coupon.EndDate <= promotion.EndDate,
                                              $"Coupon {coupon.Code} must be within the promotion period.");

                // Apply PromotionId
                coupon.PromotionId = promotionId;
                coupon.UpdatedAt = DateTime.UtcNow;
                coupon.UpdatedBy = userId.ToString();
            }

            await _couponRepository.SaveChangesAsync();
            return true;
        }


    }
}
