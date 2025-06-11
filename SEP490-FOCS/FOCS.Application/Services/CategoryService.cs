using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FOCS.Common.Constants;
using FOCS.Common.Exceptions;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using FOCS.Common.Enums;
using System.Formats.Asn1;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<MenuCategory> _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(IRepository<MenuCategory> repository, IMapper mapper)
        {
            _categoryRepository = repository;
            _mapper = mapper;
        }

        public async Task<MenuCategoryDTO> CreateCategoryAsync(CreateCategoryRequest request, string? storeId)
        {
            var isExistName = await _categoryRepository.FindAsync(x => x.Name == request.Name && x.StoreId == Guid.Parse(storeId));
            ConditionCheck.CheckCondition(isExistName.Count() == 0, Errors.Category.CategoryIsExist);

            var cate = _mapper.Map<MenuCategory>(request);
            cate.Id = Guid.NewGuid();
            cate.StoreId = Guid.Parse(storeId);

            await _categoryRepository.AddAsync(cate);
            await _categoryRepository.SaveChangesAsync();

            return _mapper.Map<MenuCategoryDTO>(cate);
        }

        public async Task<PagedResult<MenuCategoryDTO>> ListCategoriesAsync(UrlQueryParameters query, string? storeId)
        {
            var categoryQuery = _categoryRepository.AsQueryable()
               .Where(p => p.StoreId == Guid.Parse(storeId) && !p.IsDeleted);

            categoryQuery = ApplyFilters(categoryQuery, query);
            categoryQuery = ApplySearch(categoryQuery, query);
            categoryQuery = ApplySort(categoryQuery, query);
            var total = await categoryQuery.CountAsync();
            var items = await categoryQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
            .ToListAsync();

            var mapped = _mapper.Map<List<MenuCategoryDTO>>(items);
            return new PagedResult<MenuCategoryDTO>(mapped, total, query.Page, query.PageSize);
        }

        public async Task<bool> DisableCategory(Guid categoryId, string? storeId)
        {
            try
            {
                var cate = (await _categoryRepository.FindAsync(x => x.Id == categoryId))?.FirstOrDefault();
                ConditionCheck.CheckCondition(cate != null, Errors.Common.NotFound);

                cate.IsActive = false;

                _categoryRepository.Update(cate);
                await _categoryRepository.SaveChangesAsync();
            } catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> EnableCategory(Guid categoryId, string? storeId)
        {
            try
            {
                var cate = (await _categoryRepository.FindAsync(x => x.Id == categoryId))?.FirstOrDefault();
                ConditionCheck.CheckCondition(cate != null, Errors.Common.NotFound);

                cate.IsActive = true;

                _categoryRepository.Update(cate);
                await _categoryRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<MenuCategoryDTO> UpdateCategoryAsync(UpdateCategoryRequest request, Guid categoryId, string? storeId)
        {
            var cate = (await _categoryRepository.FindAsync(x => x.Id == categoryId))?.FirstOrDefault();
            ConditionCheck.CheckCondition(cate != null, Errors.Common.NotFound);

            _mapper.Map(request, cate);

            _categoryRepository.Update(cate);
            await _categoryRepository.SaveChangesAsync();

            return _mapper.Map<MenuCategoryDTO>(cate);
        }

        #region private method
        private static IQueryable<MenuCategory> ApplyFilters(IQueryable<MenuCategory> query, UrlQueryParameters parameters)
        {
            if (parameters.Filters?.Any() != true) return query;

            foreach (var (key, value) in parameters.Filters)
            {
                //query = key.ToLowerInvariant() switch
                //{
                //    "promotion_type" when Enum.TryParse<PromotionType>(value, true, out var promotionType) =>
                //        query.Where(p => p.PromotionType == promotionType),
                //    _ => query
                //};
            }

            return query;
        }

        private static IQueryable<MenuCategory> ApplySearch(IQueryable<MenuCategory> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SearchBy) || string.IsNullOrWhiteSpace(parameters.SearchValue))
                return query;

            var searchValue = parameters.SearchValue.ToLowerInvariant();

            return parameters.SearchBy.ToLowerInvariant() switch
            {
                "name" => query.Where(p => p.Name.ToLower().Contains(searchValue)),
                "description" => query.Where(x => x.Description.ToLower().Contains(searchValue)),
                _ => query
            };
        }

        private static IQueryable<MenuCategory> ApplySort(IQueryable<MenuCategory> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy)) return query;

            var isDescending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            return parameters.SortBy.ToLowerInvariant() switch
            {
                "name" => isDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "sort_order" => isDescending
                    ? query.OrderByDescending(p => p.SortOrder)
                    : query.OrderBy(p => p.SortOrder),
                _ => query
            };
        }
        #endregion
    }
}
