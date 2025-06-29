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
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FOCS.Application.Services
{
    public class AdminCouponService : IAdminCouponService
    {
        private readonly IRepository<Coupon> _couponRepository;
        private readonly IRepository<CouponUsage> _couponUsageRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<UserStore> _userStoreRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        private readonly ILogger<Coupon> _logger;

        public AdminCouponService(IRepository<Coupon> couponRepository,
                                  IRepository<CouponUsage> couponUsageRepository,
                                  IRepository<Store> storeRepository,
                                  IRepository<UserStore> userStoreRepository,
                                  UserManager<User> userManager,
                                  ILogger<Coupon> logger,
                                  IRepository<Promotion> promotionRepository,
                                  IMapper mapper)
        {
            _couponRepository = couponRepository;
            _storeRepository = storeRepository;
            _userManager = userManager;
            _userStoreRepository = userStoreRepository;
            _couponUsageRepository = couponUsageRepository;
            _logger = logger;
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<CouponAdminDTO> CreateCouponAsync(CouponAdminDTO dto, string userId, string storeId)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(storeId, out Guid storeIdGuid), Errors.Common.InvalidGuidFormat, AdminCouponConstants.FieldStoreId);
            await ValidateUser(userId, storeIdGuid);
            await ValidateStoreExists(storeIdGuid);

            // Check coupon code type
            ConditionCheck.CheckCondition(dto.CouponType == CouponType.Manual
                                                     || dto.CouponType == CouponType.AutoGenerate,
                                                        AdminCouponConstants.CheckCouponCodeType, AdminCouponConstants.FieldCouponType);

            // Auto && Has Code
            ConditionCheck.CheckCondition(dto.CouponType != CouponType.AutoGenerate || string.IsNullOrWhiteSpace(dto.Code),
                                                        AdminCouponConstants.CheckCouponCodeForAuto, AdminCouponConstants.FieldCode);

            // Type 'auto' => Generate unique code
            string couponCode = dto.CouponType == CouponType.AutoGenerate ? await GenerateUniqueCouponCodeAsync()
                                                                         : dto.Code?.Trim() ?? "";

            // Check manual code empty
            ConditionCheck.CheckCondition(dto.CouponType != CouponType.Manual
                                                     || !string.IsNullOrWhiteSpace(couponCode),
                                                        AdminCouponConstants.CheckCouponCodeForManual, AdminCouponConstants.FieldCode);

            // Check unique code
            var existing = await _couponRepository.AsQueryable()
                                                  .AnyAsync(c => c.Code == couponCode && !c.IsDeleted);
            ConditionCheck.CheckCondition(!existing, AdminCouponConstants.CheckCreateUniqueCode, AdminCouponConstants.FieldCode);

            // Check dates
            ConditionCheck.CheckCondition(dto.StartDate < dto.EndDate, AdminCouponConstants.CheckCreateDate, AdminCouponConstants.FieldDate);

            // Check promotion Id
            if (dto.PromotionId.HasValue)
            {
                var existingPromotion = await _promotionRepository.AsQueryable()
                                                  .Where(c => c.Id == dto.PromotionId && !c.IsDeleted).FirstOrDefaultAsync();

                ConditionCheck.CheckCondition(existingPromotion != null, AdminCouponConstants.CheckPromotion, AdminCouponConstants.FieldPromotionId);
                ConditionCheck.CheckCondition(dto.StartDate > existingPromotion.StartDate || dto.EndDate < existingPromotion.EndDate, AdminCouponConstants.PromotionOutOfDate, AdminCouponConstants.FieldPromotionId);
            }

            // Map DTO to entity
            var newCoupon = _mapper.Map<Coupon>(dto);
            newCoupon.Id = Guid.NewGuid();
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
            ConditionCheck.CheckCondition(coupon != null, FOCS.Common.Exceptions.Errors.Common.NotFound, Errors.FieldName.CouponId);

            try
            {
                switch (setCouponConditionRequest.ConditionType)
                {
                    case CouponConditionType.MinOrderAmount:
                        coupon.MinimumOrderAmount = setCouponConditionRequest.Value;
                        break;

                    case CouponConditionType.MinItemsQuantity:
                        if (int.TryParse(setCouponConditionRequest.Value.ToString(), out int minItemQuantity))
                        {
                            coupon.MinimumItemQuantity = minItemQuantity;
                        }
                        else
                        {
                            throw new ArgumentException("Invalid MinimumItemQuantity value.");
                        }
                        break;
                }

                _couponRepository.Update(coupon);
                await _couponRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public async Task<PagedResult<CouponAdminDTO>> GetAllCouponsAsync(UrlQueryParameters query, Guid storeId, string userId)
        {
            await ValidateUser(userId, storeId);
            await ValidateStoreExists(storeId);

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

        public async Task<PagedResult<CouponAdminDTO>> GetAvailableCouponsAsync(UrlQueryParameters query, Guid promotionId, Guid storeId, string userId)
        {
            await ValidateUser(userId, storeId);
            await ValidateStoreExists(storeId);

            var couponQuery = _couponRepository.AsQueryable().Include(c => c.Promotion)
                                                                .Where(c => !c.IsDeleted &&
                                                                c.StoreId == storeId &&
                                                                (c.PromotionId == null || c.PromotionId == promotionId) &&
                                                                c.CountUsed < c.MaxUsage &&
                                                                c.IsActive && c.EndDate > DateTime.UtcNow);

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

        public async Task<List<CouponAdminDTO>> GetCouponsByListIdAsync(List<Guid> couponIds, string storeId, string userId)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(storeId, out Guid storeIdGuid), Errors.Common.InvalidGuidFormat, AdminCouponConstants.FieldStoreId);
            await ValidateUser(userId, storeIdGuid);
            await ValidateStoreExists(storeIdGuid);

            if (couponIds == null || couponIds.Count == 0)
                return new List<CouponAdminDTO>();

            var coupons = await _couponRepository.FindAsync(c => couponIds.Contains(c.Id) && !c.IsDeleted);

            ConditionCheck.CheckCondition(coupons.Any(), AdminCouponConstants.GetCouponsByListIdNotFound, AdminCouponConstants.FieldListCouponId);

            return _mapper.Map<List<CouponAdminDTO>>(coupons.ToList());
        }

        public async Task<bool> UpdateCouponAsync(Guid id, CouponAdminDTO dto, string userId, string storeId)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null || coupon.IsDeleted)
                return false;

            // Check coupon code type
            ConditionCheck.CheckCondition(dto.CouponType == CouponType.Manual
                                                     || dto.CouponType == CouponType.AutoGenerate,
                                                        AdminCouponConstants.CheckCouponCodeType, AdminCouponConstants.FieldCouponType);

            // Auto && Has Code
            ConditionCheck.CheckCondition(dto.CouponType != CouponType.AutoGenerate || string.IsNullOrWhiteSpace(dto.Code),
                                                        AdminCouponConstants.CheckCouponCodeForAuto, AdminCouponConstants.FieldCode);

            // Type 'auto' => Generate unique code
            string couponCode = dto.CouponType == CouponType.AutoGenerate ? await GenerateUniqueCouponCodeAsync()
                                                                         : dto.Code?.Trim() ?? "";

            // Check manual code empty
            ConditionCheck.CheckCondition(dto.CouponType != CouponType.Manual
                                                     || !string.IsNullOrWhiteSpace(couponCode),
                                                        AdminCouponConstants.CheckCouponCodeForManual, AdminCouponConstants.FieldCode);

            // Check unique code (exclude current coupon)
            var existing = await _couponRepository.AsQueryable()
                                                  .AnyAsync(c => c.Id != id && c.Code == dto.Code && !c.IsDeleted);
            ConditionCheck.CheckCondition(!existing, AdminCouponConstants.CheckUpdateUniqueCode, AdminCouponConstants.FieldCode);

            // Check dates
            ConditionCheck.CheckCondition(dto.StartDate <= dto.EndDate, AdminCouponConstants.CheckUpdateDate, AdminCouponConstants.FieldDate);

            if (dto.PromotionId.HasValue && dto.PromotionId != coupon.PromotionId)
            {
                var existingPromotion = await _promotionRepository.AsQueryable()
                                                  .Where(c => c.Id == dto.PromotionId && !c.IsDeleted).FirstOrDefaultAsync();

                ConditionCheck.CheckCondition(existingPromotion != null, AdminCouponConstants.CheckPromotion, AdminCouponConstants.FieldPromotionId);
                ConditionCheck.CheckCondition(dto.StartDate > existingPromotion.StartDate && dto.EndDate < existingPromotion.EndDate, AdminCouponConstants.PromotionOutOfDate, AdminCouponConstants.FieldPromotionId);
            }

            _mapper.Map(dto, coupon);
            coupon.StoreId = Guid.Parse(storeId);
            coupon.Code = couponCode;
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

        public async Task<TrackCouponUsageDTO?> TrackCouponUsageAsync(Guid couponId)
        {
            var coupon = await _couponRepository.GetByIdAsync(couponId);
            // If coupon unavailable
            ConditionCheck.CheckCondition(coupon != null, AdminCouponConstants.CouponStatusNotFound, AdminCouponConstants.FieldCouponId);
            ConditionCheck.CheckCondition(!coupon.IsDeleted, AdminCouponConstants.CouponStatusNotFound, AdminCouponConstants.FieldCouponId);
            ConditionCheck.CheckCondition(coupon.IsActive, AdminCouponConstants.TrackNotFound, AdminCouponConstants.FieldCouponId);

            var now = DateTime.UtcNow;
            // If coupon not in date range
            ConditionCheck.CheckCondition(coupon.StartDate <= now && now <= coupon.EndDate, $"Coupon {coupon.Code} must be within the promotion period.", AdminCouponConstants.FieldDate);

            var usages = await _couponUsageRepository.AsQueryable()
                                                     .Where(u => u.CouponId == couponId)
                                                     .Select(u => new TrackCouponUsageDTO.UsageInfo
                                                     {
                                                         OrderId = u.OrderId,
                                                         UsedAt = u.UsedAt
                                                     })
                                                     .ToListAsync();

            var usageCount = usages.Count;

            ConditionCheck.CheckCondition(usageCount < coupon.MaxUsage, AdminCouponConstants.CouponUsageLimitExceed, AdminCouponConstants.FieldCouponId);

            return new TrackCouponUsageDTO
            {
                TotalLeft = coupon.MaxUsage - usageCount,
                Usages = usages
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
            ConditionCheck.CheckCondition(promotion != null && !promotion.IsDeleted, AdminCouponConstants.CheckPromotion, AdminCouponConstants.FieldPromotionId);

            // Get coupons
            var coupons = await _couponRepository.FindAsync(c => couponIds.Contains(c.Id) && !c.IsDeleted);

            foreach (var coupon in coupons)
            {
                // Check exists store
                ConditionCheck.CheckCondition(coupon.StoreId == storeId, $"Coupon {coupon.Code} does not belong to this store.", AdminCouponConstants.FieldStoreId);

                // Check coupon dates within promotion dates
                ConditionCheck.CheckCondition(coupon.StartDate >= promotion.StartDate
                                           && coupon.EndDate <= promotion.EndDate,
                                              $"Coupon {coupon.Code} must be within the promotion period.", AdminCouponConstants.FieldDate);

                // Apply PromotionId
                coupon.PromotionId = promotionId;
                coupon.UpdatedAt = DateTime.UtcNow;
                coupon.UpdatedBy = userId.ToString();
            }

            await _couponRepository.SaveChangesAsync();
            return true;
        }

        #region Private Helper Methods
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

        private async Task ValidateUser(string userId, Guid storeId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var storesOfUser = (await _userStoreRepository.FindAsync(x => x.UserId == Guid.Parse(userId))).Distinct().ToList();

            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound, AdminCouponConstants.FieldUserId);
            ConditionCheck.CheckCondition(storesOfUser.Select(x => x.StoreId).Contains(storeId), Errors.AuthError.UserUnauthor, AdminCouponConstants.FieldStoreId);
        }

        private async Task ValidateStoreExists(Guid storeId)
        {
            var store = await _storeRepository.GetByIdAsync(storeId);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound, AdminCouponConstants.FieldStoreId);
        }

        #endregion
    }
}
