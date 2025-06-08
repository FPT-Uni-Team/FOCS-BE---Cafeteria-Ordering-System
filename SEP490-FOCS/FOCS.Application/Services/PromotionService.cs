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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace FOCS.Application.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<PromotionItemCondition> _promotionItemConditionRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly IMapper _mapper;

        public PromotionService(IRepository<Promotion> promotionRepository,
            IRepository<PromotionItemCondition> promotionItemConditionRepository,
            IRepository<Store> storeRepository, IRepository<MenuItem> menuItemRepository, IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _promotionItemConditionRepository = promotionItemConditionRepository;
            _storeRepository = storeRepository;
            _menuItemRepository = menuItemRepository;
            _mapper = mapper;
        }

        public async Task<PromotionDTO> CreatePromotionAsync(PromotionDTO dto, string userId)
        {
            var context = new ValidationContext(dto);
            var validationResults = dto.Validate(context);
            ConditionCheck.CheckCondition(!validationResults.Any(), string.Join("; ", validationResults.Select(r => r.ErrorMessage)));

            var existingPromotionTitle = await _promotionRepository
                .AsQueryable()
                .FirstOrDefaultAsync(p => p.Title.Equals(dto.Title) && !p.IsDeleted);
            ConditionCheck.CheckCondition(existingPromotionTitle == null, "Promotion with this title already exists.");

            var existingPromotionTypeAndRange = await _promotionRepository
                .AsQueryable()
                .FirstOrDefaultAsync(p => p.PromotionType.Equals(dto.PromotionType)
                                        && ((dto.StartDate > p.StartDate && dto.StartDate < p.EndDate)
                                                    || (dto.EndDate > p.StartDate && dto.EndDate < p.EndDate))
                                        && !p.IsDeleted);

            var store = await _storeRepository.GetByIdAsync(dto.StoreId);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound);

            var newPromotion = _mapper.Map<Promotion>(dto);
            newPromotion.Id = Guid.NewGuid();
            newPromotion.IsDeleted = false;
            newPromotion.CreatedAt = DateTime.UtcNow;
            newPromotion.CreatedBy = userId;

            if (newPromotion.PromotionType == PromotionType.BuyXGetY)
            {
                var buyItem = _menuItemRepository.GetByIdAsync(dto.PromotionItemConditionDTO.BuyItemId);
                ConditionCheck.CheckCondition(buyItem != null, Errors.OrderError.MenuItemNotFound);
                var getItem = _menuItemRepository.GetByIdAsync(dto.PromotionItemConditionDTO.GetItemId);
                ConditionCheck.CheckCondition(getItem != null, Errors.OrderError.MenuItemNotFound);
                var newPromotionItemCondition = _mapper.Map<PromotionItemCondition>(dto.PromotionItemConditionDTO);
                newPromotionItemCondition.Id = Guid.NewGuid();
                newPromotionItemCondition.PromotionId = newPromotion.Id;

                await _promotionItemConditionRepository.AddAsync(newPromotionItemCondition);
                await _promotionItemConditionRepository.SaveChangesAsync();
            }

            await _promotionRepository.AddAsync(newPromotion);
            await _promotionRepository.SaveChangesAsync();

            return _mapper.Map<PromotionDTO>(newPromotion);
        }

        public async Task<PagedResult<PromotionDTO>> GetPromotionsByStoreAsync(UrlQueryParameters query, Guid storeId)
        {
            var promotionQuery = _promotionRepository.AsQueryable().Where(p => p.StoreId.Equals(storeId) && !p.IsDeleted);

            if (query.Filters?.Any() == true)
            {
                foreach (var (key, value) in query.Filters)
                {
                    promotionQuery = key switch
                    {
                        "promotion_type" when double.TryParse(value, out var price) =>
                            promotionQuery.Where(m => m.PromotionType.Equals(value)),
                        _ => promotionQuery
                    };
                }
            }

            // Search
            if (!string.IsNullOrEmpty(query.SearchBy) && !string.IsNullOrEmpty(query.SearchValue))
            {
                var searchValue = query.SearchValue.ToLower();

                promotionQuery = query.SearchBy.ToLower() switch
                {
                    "title" => promotionQuery.Where(s => s.Title.ToLower().Contains(searchValue)),
                    _ => promotionQuery
                };
            }

            // Sort
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                bool desc = query.SortOrder?.ToLower() == "desc";

                promotionQuery = query.SortBy.ToLower() switch
                {
                    "title" => desc ? promotionQuery.OrderByDescending(s => s.Title) : promotionQuery.OrderBy(s => s.Title),
                    "end_date" => desc ? promotionQuery.OrderByDescending(s => s.EndDate) : promotionQuery.OrderBy(s => s.EndDate),
                    "start_date" => desc ? promotionQuery.OrderByDescending(s => s.StartDate) : promotionQuery.OrderBy(s => s.StartDate),
                    "promotion_type" => desc ? promotionQuery.OrderByDescending(s => s.PromotionType) : promotionQuery.OrderBy(s => s.PromotionType),
                    "discount_value" => desc ? promotionQuery.OrderByDescending(s => s.DiscountValue) : promotionQuery.OrderBy(s => s.DiscountValue),
                    _ => promotionQuery
                };
            }

            // Pagination
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
                                .Where(p => p.Id.Equals(promotionId) && !p.IsDeleted).FirstOrDefaultAsync();
            ConditionCheck.CheckCondition(promotion != null, Errors.Common.NotFound);
            return _mapper.Map<PromotionDTO>(promotion);
        }

        public async Task<bool> UpdatePromotionAsync(Guid id, PromotionDTO dto, string userId)
        {
            var promotion = await _promotionRepository.GetByIdAsync(id);
            if (promotion == null || promotion.IsDeleted)
                return false;

            _mapper.Map(dto, promotion);
            promotion.UpdatedAt = DateTime.UtcNow;
            promotion.UpdatedBy = userId;

            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivePromotionAsync(Guid id, string userId)
        {
            var promotion = await _promotionRepository.GetByIdAsync(id);
            if (promotion == null || promotion.IsDeleted)
                return false;

            ConditionCheck.CheckCondition(!promotion.IsActive, Errors.PromotionError.PromotionActive);
            promotion.IsActive = true;
            promotion.UpdatedAt = DateTime.UtcNow;
            promotion.UpdatedBy = userId;

            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> InactivePromotionAsync(Guid id, string userId)
        {
            var promotion = await _promotionRepository.GetByIdAsync(id);
            if (promotion == null || promotion.IsDeleted)
                return false;

            ConditionCheck.CheckCondition(promotion.IsActive, Errors.PromotionError.PromotionInactive);
            promotion.IsActive = false;
            promotion.UpdatedAt = DateTime.UtcNow;
            promotion.UpdatedBy = userId;

            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePromotionAsync(Guid id, string userId)
        {
            var promotion = await _promotionRepository.GetByIdAsync(id);
            if (promotion == null || promotion.IsDeleted)
                return false;

            promotion.IsActive = false;
            promotion.IsDeleted = true;
            promotion.UpdatedAt = DateTime.UtcNow;
            promotion.UpdatedBy = userId;

            await _promotionRepository.SaveChangesAsync();
            return true;
        }

        public Task IsValidPromotionCouponAsync(string couponCode, string storeId)
        {
            throw new NotImplementedException();
        }
    }
}
