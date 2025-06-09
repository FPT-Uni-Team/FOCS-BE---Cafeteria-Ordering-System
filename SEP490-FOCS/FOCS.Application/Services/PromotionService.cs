using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace FOCS.Application.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<Coupon> _couponRepository;

        private readonly IRepository<CouponUsage> _couponUsageRepository;
        private readonly IRepository<PromotionItemCondition> _promotionItemConditionRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly IMapper _mapper;
        public PromotionService(
            IRepository<Promotion> promotionRepository,
            IRepository<PromotionItemCondition> promotionItemConditionRepository,
            IRepository<Store> storeRepository,
            IRepository<MenuItem> menuItemRepository,
            IRepository<Coupon> couponRepository,
            IRepository<CouponUsage> couponUsageRepository,
            IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _promotionItemConditionRepository = promotionItemConditionRepository;
            _storeRepository = storeRepository;
            _menuItemRepository = menuItemRepository;
            _couponRepository = couponRepository;
            _couponUsageRepository = couponUsageRepository;
            _mapper = mapper;
        }

        public async Task<PromotionDTO> CreatePromotionAsync(PromotionDTO dto, string userId)
        {
            await ValidatePromotionDto(dto);
            await ValidatePromotionUniqueness(dto);
            await ValidateStoreExists(dto.StoreId);

            var newPromotion = CreatePromotionEntity(dto, userId);

            await _promotionRepository.AddAsync(newPromotion);
            await _promotionRepository.SaveChangesAsync();

            if (newPromotion.PromotionType == PromotionType.BuyXGetY)
            {
                await CreatePromotionItemCondition(dto, newPromotion.Id);
            }

            return _mapper.Map<PromotionDTO>(newPromotion);
        }

        public async Task<PagedResult<PromotionDTO>> GetPromotionsByStoreAsync(UrlQueryParameters query, Guid storeId)
        {
            var promotionQuery = _promotionRepository.AsQueryable()
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

        public async Task<PromotionDTO> GetPromotionAsync(Guid promotionId)
        {
            var promotion = await _promotionRepository.AsQueryable()
                .FirstOrDefaultAsync(p => p.Id == promotionId && !p.IsDeleted);

            ConditionCheck.CheckCondition(promotion != null, Errors.Common.NotFound);
            return _mapper.Map<PromotionDTO>(promotion);
        }

        public async Task<bool> UpdatePromotionAsync(Guid id, PromotionDTO dto, string userId)
        {
            var promotion = await GetActivePromotionById(id);
            if (promotion == null) return false;

            _mapper.Map(dto, promotion);
            UpdateAuditFields(promotion, userId);

            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivatePromotionAsync(Guid id, string userId)
        {
            var promotion = await GetActivePromotionById(id);
            if (promotion == null) return false;

            ConditionCheck.CheckCondition(!promotion.IsActive, Errors.PromotionError.PromotionActive);

            promotion.IsActive = true;
            UpdateAuditFields(promotion, userId);

            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivatePromotionAsync(Guid id, string userId)
        {
            var promotion = await GetActivePromotionById(id);
            if (promotion == null) return false;

            ConditionCheck.CheckCondition(promotion.IsActive, Errors.PromotionError.PromotionInactive);

            promotion.IsActive = false;
            UpdateAuditFields(promotion, userId);

            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePromotionAsync(Guid id, string userId)
        {
            var promotion = await GetActivePromotionById(id);
            if (promotion == null) return false;

            promotion.IsActive = false;
            promotion.IsDeleted = true;
            UpdateAuditFields(promotion, userId);

            await _promotionRepository.SaveChangesAsync();
            return true;
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
            //var couponUsageTime = await _couponUsageRepository.FindAsync(x => x.UserId == Guid.Parse(userId) && x.CouponId == Guid.Parse(couponCode));
            //ConditionCheck.CheckCondition(couponUsageTime.Count() <= coupon.MaxUsagePerUser, Errors.PromotionError.CouponMaxUsed);

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


        #region Private Helper Methods

        private async Task ValidatePromotionDto(PromotionDTO dto)
        {
            var context = new ValidationContext(dto);
            var validationResults = dto.Validate(context);
            ConditionCheck.CheckCondition(!validationResults.Any(),
                string.Join("; ", validationResults.Select(r => r.ErrorMessage)));
        }

        private async Task ValidatePromotionUniqueness(PromotionDTO dto)
        {
            var existingPromotionTitle = await _promotionRepository
                .AsQueryable()
                .FirstOrDefaultAsync(p => p.Title == dto.Title && !p.IsDeleted);

            ConditionCheck.CheckCondition(existingPromotionTitle == null, Errors.PromotionError.PromotionTitleExist);

            var overlappingPromotion = await _promotionRepository
                .AsQueryable()
                .FirstOrDefaultAsync(p => p.PromotionType == dto.PromotionType
                                        && p.StoreId == dto.StoreId
                                        && ((dto.StartDate >= p.StartDate && dto.StartDate <= p.EndDate)
                                            || (dto.EndDate >= p.StartDate && dto.EndDate <= p.EndDate)
                                            || (dto.StartDate <= p.StartDate && dto.EndDate >= p.EndDate))
                                        && !p.IsDeleted);

            ConditionCheck.CheckCondition(overlappingPromotion == null, Errors.PromotionError.PromotionOverLapping);
        }

        private async Task ValidateStoreExists(Guid storeId)
        {
            var store = await _storeRepository.GetByIdAsync(storeId);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound);
        }

        private Promotion CreatePromotionEntity(PromotionDTO dto, string userId)
        {
            var promotion = _mapper.Map<Promotion>(dto);
            promotion.Id = Guid.NewGuid();
            promotion.IsDeleted = false;
            promotion.CreatedAt = DateTime.UtcNow;
            promotion.CreatedBy = userId;
            return promotion;
        }

        private async Task CreatePromotionItemCondition(PromotionDTO dto, Guid promotionId)
        {
            var buyItem = await _menuItemRepository.GetByIdAsync(dto.PromotionItemConditionDTO.BuyItemId);
            ConditionCheck.CheckCondition(buyItem != null, Errors.OrderError.MenuItemNotFound);

            var getItem = await _menuItemRepository.GetByIdAsync(dto.PromotionItemConditionDTO.GetItemId);
            ConditionCheck.CheckCondition(getItem != null, Errors.OrderError.MenuItemNotFound);

            var condition = _mapper.Map<PromotionItemCondition>(dto.PromotionItemConditionDTO);
            condition.Id = Guid.NewGuid();
            condition.PromotionId = promotionId;

            await _promotionItemConditionRepository.AddAsync(condition);
            await _promotionItemConditionRepository.SaveChangesAsync();
        }

        private async Task<Promotion> GetActivePromotionById(Guid id)
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
            if (string.IsNullOrWhiteSpace(parameters.SortBy)) return query;

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
                _ => query
            };
        }

        #endregion
    }
}