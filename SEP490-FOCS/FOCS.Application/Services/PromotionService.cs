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
using System.ComponentModel.DataAnnotations;
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
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        public PromotionService(
            IRepository<Promotion> promotionRepository,
            IRepository<PromotionItemCondition> promotionItemConditionRepository,
            IRepository<Store> storeRepository,
            IRepository<MenuItem> menuItemRepository,
            IRepository<Coupon> couponRepository,
            IRepository<CouponUsage> couponUsageRepository,
            UserManager<User> userManager,
            IMapper mapper,
            IRepository<UserStore> userStoreRepository)
        {
            _promotionRepository = promotionRepository;
            _promotionItemConditionRepository = promotionItemConditionRepository;
            _storeRepository = storeRepository;
            _menuItemRepository = menuItemRepository;
            _couponRepository = couponRepository;
            _couponUsageRepository = couponUsageRepository;
            _userManager = userManager;
            _mapper = mapper;
            _userStoreRepository = userStoreRepository;
        }

        public async Task<PromotionDTO> CreatePromotionAsync(PromotionDTO dto, string userId)
        {
            await ValidateUser(userId, dto.StoreId);
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

        public async Task<PagedResult<PromotionDTO>> GetPromotionsByStoreAsync(UrlQueryParameters query, Guid storeId, string userId)
        {
            await ValidateUser(userId, storeId);
            var promotionQuery = _promotionRepository.AsQueryable().Include(x => x.PromotionItemConditions)
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
                .FirstOrDefaultAsync(p => p.Id == promotionId && !p.IsDeleted);

            ConditionCheck.CheckCondition(promotion != null, Errors.Common.NotFound);
            await ValidateUser(userId, promotion.StoreId);

            return _mapper.Map<PromotionDTO>(promotion);
        }

        public async Task<bool> UpdatePromotionAsync(Guid promotionId, PromotionDTO dto, string userId)
        {
            var promotion = await GetAvailablePromotionById(promotionId);
            if (promotion == null) return false;

            await ValidateUser(userId, promotion.StoreId);
            _mapper.Map(dto, promotion);
            UpdateAuditFields(promotion, userId);

            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivatePromotionAsync(Guid promotionId, string userId)
        {
            var promotion = await GetAvailablePromotionById(promotionId);
            if (promotion == null) return false;

            await ValidateUser(userId, promotion.StoreId);
            ConditionCheck.CheckCondition(!promotion.IsActive, Errors.PromotionError.PromotionActive);
            ConditionCheck.CheckCondition(promotion.StartDate.Date <= DateTime.Now.Date
                                                        && promotion.EndDate.Date >= DateTime.Now.Date,
                                                        Errors.PromotionError.PromotionInvalidDateToActive);

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
            ConditionCheck.CheckCondition(promotion.IsActive, Errors.PromotionError.PromotionInactive);

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
            ConditionCheck.CheckCondition(!promotion.IsActive, Errors.PromotionError.PromotionActive);
            promotion.IsActive = false;
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
            ConditionCheck.CheckCondition(!string.IsNullOrWhiteSpace(couponCode), Errors.Common.Empty);

            var coupon = await _couponRepository.AsQueryable().FirstOrDefaultAsync(x => x.Code == couponCode && x.StoreId == storeId);
            ConditionCheck.CheckCondition(coupon != null, Errors.PromotionError.CouponNotFound);

            ConditionCheck.CheckCondition(coupon.CountUsed < coupon.MaxUsage, Errors.PromotionError.CouponMaxUsed);

            //Validate max use per user
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var couponUsageTime = await _couponUsageRepository.FindAsync(x => x.UserId == Guid.Parse(userId) && x.CouponId == coupon.Id);
                ConditionCheck.CheckCondition(couponUsageTime.Count() <= coupon.MaxUsagePerUser, Errors.PromotionError.CouponMaxUsed);
            }

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



        private async Task ValidateUser(string userId, Guid storeId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var storesOfUser = (await _userStoreRepository.FindAsync(x => x.UserId == Guid.Parse(userId))).Distinct().ToList();

            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound);
            ConditionCheck.CheckCondition(storesOfUser.Select(x => x.StoreId).Contains(storeId), Errors.AuthError.UserUnauthor);
        }

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

        private async Task ValidateMenuItem(Guid id)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(id);
            ConditionCheck.CheckCondition(menuItem != null, Errors.OrderError.MenuItemNotFound);
        }

        private async Task CreatePromotionItemCondition(PromotionDTO dto, Guid promotionId)
        {
            await ValidateMenuItem(dto.PromotionItemConditionDTO.BuyItemId);
            await ValidateMenuItem(dto.PromotionItemConditionDTO.GetItemId);

            var condition = _mapper.Map<PromotionItemCondition>(dto.PromotionItemConditionDTO);
            condition.Id = Guid.NewGuid();
            condition.PromotionId = promotionId;

            await _promotionItemConditionRepository.AddAsync(condition);
            await _promotionItemConditionRepository.SaveChangesAsync();
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