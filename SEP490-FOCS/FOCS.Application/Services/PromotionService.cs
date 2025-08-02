using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.Formats.Asn1;
namespace FOCS.Application.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<Coupon> _couponRepository;

        private readonly IRepository<UserStore> _userStoreRepository;

        private readonly IRepository<CouponUsage> _couponUsageRepository;
        private readonly IRepository<PromotionItemCondition> _promotionItemConditionRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<StoreSetting> _storeSettingRepository;
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        private readonly IPricingService _pricingService;
        public PromotionService(
            IRepository<Promotion> promotionRepository,
            IRepository<PromotionItemCondition> promotionItemConditionRepository,
            IRepository<Store> storeRepository,
            IRepository<MenuItem> menuItemRepository,
            IRepository<Coupon> couponRepository,
            IRepository<CouponUsage> couponUsageRepository,
            UserManager<User> userManager,
            IMapper mapper,
            IRepository<StoreSetting> storeSettingRepository,
            IRepository<UserStore> userStoreRepository,
            IPricingService pricingService)
        {
            _promotionRepository = promotionRepository;
            _promotionItemConditionRepository = promotionItemConditionRepository;
            _storeRepository = storeRepository;
            _menuItemRepository = menuItemRepository;
            _couponRepository = couponRepository;
            _storeSettingRepository = storeSettingRepository;
            _couponUsageRepository = couponUsageRepository;
            _userManager = userManager;
            _mapper = mapper;
            _userStoreRepository = userStoreRepository;
            _pricingService = pricingService;
        }

        public async Task<PromotionDTO> CreatePromotionAsync(PromotionDTO dto, Guid storeId, string userId)
        {
            await ValidateStoreExists(storeId);
            await ValidateUser(userId, storeId);
            await ValidatePromotionDto(dto);
            await ValidatePromotionUniqueness(dto, storeId);

            var coupons = await ValidateCoupons(dto.CouponIds, dto.StartDate, dto.EndDate, storeId);

            var newPromotion = CreatePromotionEntity(dto, userId, storeId, coupons);

            await _promotionRepository.AddAsync(newPromotion);

            if (newPromotion.PromotionType == PromotionType.BuyXGetY)
            {
                await CreateOrUpdatePromotionItemCondition(dto, newPromotion.Id);
            }

            await _promotionRepository.SaveChangesAsync();

            return _mapper.Map<PromotionDTO>(newPromotion);
        }

        public async Task<PagedResult<PromotionDTO>> GetPromotionsByStoreAsync(UrlQueryParameters query, Guid storeId, string userId)
        {
            await ValidateUser(userId, storeId);
            var promotionQuery = _promotionRepository.AsQueryable().Include(p => p.PromotionItemConditions)
                .Include(p => p.Coupons)
                .Where(p => p.StoreId == storeId && !p.IsDeleted);

            promotionQuery = ApplyFilters(promotionQuery, query);
            promotionQuery = ApplySearch(promotionQuery, query);
            promotionQuery = ApplySort(promotionQuery, query);

            var total = await promotionQuery.CountAsync();
            var items = await promotionQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<PromotionDTO>>(items);
            return new PagedResult<PromotionDTO>(mapped, total, query.Page, query.PageSize);
        }

        public async Task<PromotionDTO> GetPromotionAsync(Guid promotionId, string userId)
        {
            var promotion = await _promotionRepository.AsQueryable()
                .Include(p => p.PromotionItemConditions).Include(p => p.Coupons)
                .FirstOrDefaultAsync(p => p.Id == promotionId && !p.IsDeleted);

            ConditionCheck.CheckCondition(promotion != null, Errors.Common.NotFound, Errors.FieldName.Id);
            await ValidateUser(userId, promotion.StoreId);

            return _mapper.Map<PromotionDTO>(promotion);
        }

        public async Task<bool> UpdatePromotionAsync(Guid promotionId, PromotionDTO dto, Guid storeId, string userId)
        {
            ConditionCheck.CheckCondition(promotionId == dto.Id, Errors.Common.NotFound, Errors.FieldName.Id);
            var promotion = await GetAvailablePromotionById(promotionId);
            if (promotion == null) return false;
            await ValidateStoreExists(storeId);
            await ValidateUser(userId, promotion.StoreId);
            await ValidatePromotionUniqueness(dto, storeId);

            if (promotion.IsActive &&
                    promotion.StartDate <= DateTime.UtcNow && promotion.EndDate >= DateTime.UtcNow)
            {
                await ValidatePromotionDto(dto, updateOngoingPromotion: true);
                promotion.EndDate = dto.EndDate;
            }
            else
            {
                await ValidatePromotionDto(dto);
                var coupons = await ValidateCoupons(dto.CouponIds, dto.StartDate, dto.EndDate, storeId, promotionId);
                promotion.Coupons = coupons;
                _mapper.Map(dto, promotion);

                if (promotion.PromotionType == PromotionType.BuyXGetY)
                {
                    await CreateOrUpdatePromotionItemCondition(dto, promotion.Id);
                }
            }

            UpdateAuditFields(promotion, userId);
            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivatePromotionAsync(Guid promotionId, string userId)
        {
            var promotion = await GetAvailablePromotionById(promotionId);
            if (promotion == null) return false;

            await ValidateUser(userId, promotion.StoreId);
            ConditionCheck.CheckCondition(!promotion.IsActive, Errors.PromotionError.PromotionActive, Errors.FieldName.IsActive);

            promotion.IsActive = true;
            UpdateAuditFields(promotion, userId);

            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivatePromotionAsync(Guid promotionId, string userId)
        {
            var promotion = await GetAvailablePromotionById(promotionId);
            if (promotion == null) return false;

            await ValidateUser(userId, promotion.StoreId);
            ConditionCheck.CheckCondition(promotion.IsActive, Errors.PromotionError.PromotionInactive, Errors.FieldName.IsActive);

            promotion.IsActive = false;
            UpdateAuditFields(promotion, userId);

            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePromotionAsync(Guid promotionId, string userId)
        {
            var promotion = await GetAvailablePromotionById(promotionId);
            if (promotion == null) return false;

            await ValidateUser(userId, promotion.StoreId);
            ConditionCheck.CheckCondition(!promotion.IsActive, Errors.PromotionError.PromotionActive, Errors.FieldName.IsActive);
            promotion.IsDeleted = true;
            UpdateAuditFields(promotion, userId);
            await _promotionRepository.SaveChangesAsync();

            var couponList = await _couponRepository.FindAsync(x => x.PromotionId == promotionId);
            if (!couponList.Any()) return true;

            foreach (var coupon in couponList)
            {
                coupon.PromotionId = null;
                coupon.UpdatedAt = DateTime.UtcNow;
                coupon.UpdatedBy = userId;
            }
            await _couponRepository.SaveChangesAsync();
            return true;
        }

        public async Task IsValidPromotionCouponAsync(string couponCode, string userId, Guid storeId)
        {
            //Check coupon eligible
            ConditionCheck.CheckCondition(!string.IsNullOrWhiteSpace(couponCode), Errors.Common.Empty, Errors.FieldName.CouponCode);

            var coupon = await _couponRepository.AsQueryable().FirstOrDefaultAsync(x => x.Code == couponCode && x.StoreId == storeId);
            ConditionCheck.CheckCondition(coupon != null, Errors.PromotionError.CouponNotFound, Errors.FieldName.CouponCode);

            ConditionCheck.CheckCondition(coupon.CountUsed < coupon.MaxUsage, Errors.PromotionError.CouponMaxUsed, Errors.FieldName.CouponMaxUsed);

            //Validate max use per user
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var couponUsageTime = await _couponUsageRepository.FindAsync(x => x.UserId == Guid.Parse(userId) && x.CouponId == coupon.Id);
                ConditionCheck.CheckCondition(couponUsageTime.Count() <= coupon.MaxUsagePerUser, Errors.PromotionError.CouponMaxUsed, Errors.FieldName.CouponMaxUsed);
            }

            var currentDate = DateTime.UtcNow;
            ConditionCheck.CheckCondition(currentDate >= coupon.StartDate && currentDate <= coupon.EndDate, Errors.PromotionError.InvalidPeriodDatetime, Errors.FieldName.EndDate);

            //Check promotion eligible
            if (coupon.PromotionId.HasValue)
            {
                var promotion = await _promotionRepository.FindAsync(x => x.Id == coupon.PromotionId && x.StoreId == storeId);
                ConditionCheck.CheckCondition(promotion != null, Errors.PromotionError.PromotionNotFound, Errors.FieldName.Id);
                var currentPromotion = promotion?.FirstOrDefault();
                ConditionCheck.CheckCondition(currentDate >= currentPromotion?.StartDate && currentDate <= currentPromotion.EndDate, Errors.PromotionError.InvalidPeriodDatetime, Errors.FieldName.EndDate);
            }
        }

        public async Task<DiscountResultDTO> ApplyEligiblePromotions(DiscountResultDTO discountResult)
        {
            var now = DateTime.UtcNow;

            var promotionsQuery = _promotionRepository.AsQueryable()
                .Include(x => x.Coupons)
                .Where(x => x.StartDate <= now && x.EndDate >= now && !x.IsDeleted && x.IsActive && (x.Coupons == null || !x.Coupons.Any()) && x.CanApplyCombine == true);

            var promotionFixed = await promotionsQuery
                .Where(x => x.PromotionType == PromotionType.FixedAmount)
                .OrderByDescending(x => x.DiscountValue)
                .FirstOrDefaultAsync();

            var promotionPercent = await promotionsQuery
                .Where(x => x.PromotionType == PromotionType.Percentage)
                .OrderByDescending(x => x.DiscountValue)
                .FirstOrDefaultAsync();

            decimal totalBeforeDiscount = discountResult.TotalPrice;
            decimal fixedDiscountAmount = (decimal)promotionFixed?.DiscountValue!;
            decimal percentDiscountAmount = promotionPercent != null
                ? totalBeforeDiscount * ((decimal)promotionPercent.DiscountValue! / 100)
                : 0;

            if (fixedDiscountAmount == 0 && percentDiscountAmount == 0)
                return discountResult;

            if (percentDiscountAmount > fixedDiscountAmount)
            {
                discountResult.TotalPrice -= await ApplyPromotion(promotionPercent, percentDiscountAmount);
                discountResult.AppliedPromotions.Add(promotionPercent!.Title);
            }
            else
            {
                discountResult.TotalPrice -= await ApplyPromotion(promotionFixed, fixedDiscountAmount, discountResult);
                discountResult.AppliedPromotions.Add(promotionFixed!.Title);
            }

            return discountResult;
        }


        #region Private Helper Methods

        private async Task<decimal> ApplyPromotion(Promotion? promotion, decimal decreaseAmount, DiscountResultDTO? discountResultDTO = null)
        {
            return promotion.PromotionScope switch
            {
                PromotionScope.Order => decreaseAmount,
                PromotionScope.Item => await CalculateDiscountForEachItem(promotion, decreaseAmount, discountResultDTO)
            };
        }

        private async Task<decimal> CalculateDiscountForEachItem(Promotion promotion, decimal maxDecreaseAmount, DiscountResultDTO? discountResultDTO)
        {
            if (discountResultDTO?.ItemDiscountDetails == null || !discountResultDTO.ItemDiscountDetails.Any())
                return 0;

            decimal totalDiscount = 0;

            foreach (var item in discountResultDTO.ItemDiscountDetails)
            {
                bool isEligible = promotion.AcceptForItems == null ||
                                  !promotion.AcceptForItems.Any() ||
                                  promotion.AcceptForItems.Contains(Guid.Parse(item.ItemCode));

                if (!isEligible)
                    continue;

                var parts = item.ItemCode.Split('_');
                var menuItemId = Guid.Parse(parts[0]);
                Guid? variantId = parts.Length > 1 ? Guid.Parse(parts[1]) : null;

                var price = await _pricingService.GetPriceByProduct(menuItemId, variantId, promotion.StoreId);

                double itemUnitPrice = price.ProductPrice + (price.VariantPrice ?? 0);
                double itemTotalPrice = itemUnitPrice * item.Quantity;

                decimal itemDiscount = 0;

                switch (promotion.PromotionType)
                {
                    case PromotionType.FixedAmount:
                        itemDiscount = (decimal)(promotion.DiscountValue ?? 0) * item.Quantity;
                        break;

                    case PromotionType.Percentage:
                        itemDiscount = (decimal)itemTotalPrice * ((decimal)promotion.DiscountValue!.Value / 100);
                        break;
                }

                totalDiscount += itemDiscount;
            }

            return promotion.MaxDiscountValue == null
                ? Math.Min((decimal)totalDiscount, (decimal)promotion.MaxDiscountValue)
                : totalDiscount;
        }

        private async Task ValidateUser(string userId, Guid storeId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound, Errors.FieldName.UserId);

            var storesOfUser = (await _userStoreRepository.FindAsync(x => x.UserId == Guid.Parse(userId))).Distinct().ToList();
            ConditionCheck.CheckCondition(storesOfUser.Select(x => x.StoreId).Contains(storeId), Errors.AuthError.UserUnauthor, Errors.FieldName.UserId);
        }

        private async Task ValidatePromotionDto(PromotionDTO dto, bool updateOngoingPromotion = false)
        {
            var context = new ValidationContext(dto);
            dto.Validate(context, updateOngoingPromotion);
        }

        private async Task ValidatePromotionUniqueness(PromotionDTO dto, Guid storeId)
        {
            var existingPromotionTitle = await _promotionRepository
                .AsQueryable()
                .FirstOrDefaultAsync(p => p.Title == dto.Title && p.Id != dto.Id && !p.IsDeleted);

            ConditionCheck.CheckCondition(existingPromotionTitle == null, Errors.PromotionError.PromotionTitleExist, Errors.FieldName.Title);

            var overlappingPromotion = await _promotionRepository
                .AsQueryable()
                .FirstOrDefaultAsync(p => p.PromotionType == dto.PromotionType
                                        && p.StoreId == storeId
                                        && p.Id != dto.Id
                                        && ((dto.StartDate >= p.StartDate && dto.StartDate <= p.EndDate)
                                            || (dto.EndDate >= p.StartDate && dto.EndDate <= p.EndDate)
                                            || (dto.StartDate <= p.StartDate && dto.EndDate >= p.EndDate))
                                        && !p.IsDeleted);

            ConditionCheck.CheckCondition(overlappingPromotion == null, Errors.PromotionError.PromotionOverLapping, Errors.FieldName.EndDate);

            if (dto.AcceptForItems?.Count > 0)
            {
                foreach (var item in dto.AcceptForItems)
                {
                    await ValidateMenuItem(item);
                }
            }
        }

        private async Task ValidateStoreExists(Guid storeId)
        {
            var store = await _storeRepository.GetByIdAsync(storeId);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound, Errors.FieldName.StoreId);
        }

        private async Task<ICollection<Coupon>> ValidateCoupons(List<Guid> couponIds, DateTime startDate, DateTime endDate, Guid storeId, Guid? promotionId = null)
        {
            var coupons = await _couponRepository.AsQueryable()
                            .Where(c => c.StoreId == storeId &&
                                        couponIds.Contains(c.Id))
                            .ToListAsync();

            ConditionCheck.CheckCondition(!coupons.Any(c => c.StartDate < startDate || c.EndDate > endDate), Errors.PromotionError.InvalidPeriodDatetime, Errors.FieldName.CouponIds);
            ConditionCheck.CheckCondition(coupons.All(c => c.PromotionId == null || c.PromotionId == promotionId), Errors.PromotionError.CouponAssigned, Errors.FieldName.CouponIds);

            return coupons;
        }

        private Promotion CreatePromotionEntity(PromotionDTO dto, string userId, Guid storeId, ICollection<Coupon>? coupons = null)
        {
            var promotion = _mapper.Map<Promotion>(dto);
            promotion.CountUsed = 0;
            promotion.Id = Guid.NewGuid();
            promotion.StoreId = storeId;
            promotion.IsDeleted = false;
            promotion.CreatedAt = DateTime.UtcNow;
            promotion.CreatedBy = userId;
            promotion.Coupons = coupons;
            return promotion;
        }

        private async Task ValidateMenuItem(Guid id)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(id);
            ConditionCheck.CheckCondition(menuItem != null, Errors.OrderError.MenuItemNotFound, Errors.FieldName.MenuItemId);
        }

        private async Task CreateOrUpdatePromotionItemCondition(PromotionDTO dto, Guid promotionId)
        {
            await ValidateMenuItem(dto.PromotionItemConditionDTO.BuyItemId);
            await ValidateMenuItem(dto.PromotionItemConditionDTO.GetItemId);

            var existingCondition = await _promotionItemConditionRepository.AsQueryable().Where(c => c.PromotionId == promotionId).FirstOrDefaultAsync();

            if (existingCondition != null)
            {
                _mapper.Map(dto.PromotionItemConditionDTO, existingCondition);
                await _promotionItemConditionRepository.SaveChangesAsync();
            }
            else
            {
                var condition = _mapper.Map<PromotionItemCondition>(dto.PromotionItemConditionDTO);
                condition.Id = Guid.NewGuid();
                condition.PromotionId = promotionId;
                await _promotionItemConditionRepository.AddAsync(condition);
                await _promotionItemConditionRepository.SaveChangesAsync();
            }
        }

        private async Task<Promotion> GetAvailablePromotionById(Guid id)
        {
            var promotion = await _promotionRepository.GetByIdAsync(id);
            return (promotion?.IsDeleted == false) ? promotion : null;
        }

        private static void UpdateAuditFields(Promotion promotion, string userId)
        {
            promotion.UpdatedAt = DateTime.UtcNow;
            promotion.UpdatedBy = userId;
        }

        private static IQueryable<Promotion> ApplyFilters(IQueryable<Promotion> query, UrlQueryParameters parameters)
        {
            if (parameters.Filters?.Any() != true) return query;

            foreach (var (key, value) in parameters.Filters)
            {
                query = key.ToLowerInvariant() switch
                {
                    "promotion_type" when Enum.TryParse<PromotionType>(value, true, out var promotionType) =>
                        query.Where(p => p.PromotionType == promotionType),
                    "start_date" => query.Where(p => p.StartDate >= DateTime.Parse(value)),
                    "end_date" => query.Where(p => p.EndDate <= DateTime.Parse(value)),
                    "status" when Enum.TryParse<PromotionStatus>(value, true, out var status) =>
                        status switch
                        {
                            PromotionStatus.Incomming => query.Where(p => p.IsActive && p.StartDate > DateTime.UtcNow),
                            PromotionStatus.OnGoing => query.Where(p => p.IsActive && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow),
                            PromotionStatus.Expired => query.Where(p => p.IsActive && p.EndDate < DateTime.UtcNow),
                            PromotionStatus.UnAvailable => query.Where(p => !p.IsActive),
                            _ => query
                        },
                    _ => query
                };
            }

            return query;
        }

        private static IQueryable<Promotion> ApplySearch(IQueryable<Promotion> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SearchBy) || string.IsNullOrWhiteSpace(parameters.SearchValue))
                return query;

            var searchValue = parameters.SearchValue.ToLowerInvariant();

            return parameters.SearchBy.ToLowerInvariant() switch
            {
                "title" => query.Where(p => p.Title.ToLower().Contains(searchValue)),
                _ => query
            };
        }

        private static IQueryable<Promotion> ApplySort(IQueryable<Promotion> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy)) return query.OrderBy(p => p.StartDate);

            var isDescending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            return parameters.SortBy.ToLowerInvariant() switch
            {
                "title" => isDescending
                    ? query.OrderByDescending(p => p.Title)
                    : query.OrderBy(p => p.Title),
                "end_date" => isDescending
                    ? query.OrderByDescending(p => p.EndDate)
                    : query.OrderBy(p => p.EndDate),
                "start_date" => isDescending
                    ? query.OrderByDescending(p => p.StartDate)
                    : query.OrderBy(p => p.StartDate),
                "promotion_type" => isDescending
                    ? query.OrderByDescending(p => p.PromotionType)
                    : query.OrderBy(p => p.PromotionType),
                "discount_value" => isDescending
                    ? query.OrderByDescending(p => p.DiscountValue)
                    : query.OrderBy(p => p.DiscountValue),
                _ => query.OrderBy(p => p.StartDate)
            };
        }

        #endregion
    }
}