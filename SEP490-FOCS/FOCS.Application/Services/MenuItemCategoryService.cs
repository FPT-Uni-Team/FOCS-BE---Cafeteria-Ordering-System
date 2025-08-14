using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Order.Infrastucture.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FOCS.Application.Services
{
    public class MenuItemCategoryService : IMenuItemCategoryService
    {
        private readonly IRepository<MenuItemCategories> _menuItemCategoryRepository;

        private readonly ICategoryService _categoryService;

        private IMapper _mapper;
        public MenuItemCategoryService(IRepository<MenuItemCategories> menuItemCategoryRepository, IMapper mapper, ICategoryService categoryService)
        {
            _menuItemCategoryRepository = menuItemCategoryRepository;
            _mapper = mapper;
            _categoryService = categoryService;
        }

        public async Task AssignCategoriesToMenuItem(List<Guid> categoryIds, Guid menuItemId, string userId)
        {
            try
            {
                var isExist = await _menuItemCategoryRepository.AsQueryable().AnyAsync(x => categoryIds.Contains(x.CategoryId) && x.MenuItemId == menuItemId);

                ConditionCheck.CheckCondition(!isExist, Errors.Common.IsExist);


                var menuItemCategories = categoryIds.Select(x => new MenuItemCategories
                {
                    Id = Guid.NewGuid(),
                    CategoryId = x,
                    MenuItemId = menuItemId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });

                await _menuItemCategoryRepository.AddRangeAsync(menuItemCategories);
                await _menuItemCategoryRepository.SaveChangesAsync();
            }catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> DeleteCategory(Guid cateId, Guid storeId)
        {
            try
            {
                var cate = await _categoryService.GetById(cateId, storeId);

                var associateWithProduct = await _menuItemCategoryRepository.AsQueryable().Where(x => x.CategoryId == cateId).ToListAsync();

                if(associateWithProduct != null)
                {
                    _menuItemCategoryRepository.RemoveRange(associateWithProduct);
                }

                await _categoryService.RemoveCategory(cate.Id, storeId.ToString());

                return true;

            } catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<List<MenuCategoryDTO>> ListCategoriesWithMenuItem(Guid menuItemId, string storeId)
        {
            var categories = await _menuItemCategoryRepository.AsQueryable().Include(x => x.Category).Include(x => x.MenuItem).Where(x => x.MenuItemId == menuItemId && x.CreatedBy == storeId).ToListAsync();

            return categories.Select(x => new MenuCategoryDTO
            {
                Id = x.CategoryId,
                Name = x.Category.Name,
                Description = x.Category.Description,
                IsActive = x.Category.IsActive
            }).ToList();
        }

        public async Task<bool> AssignMenuItemsToCategory(Guid cateId, List<Guid> menuItemIds, Guid storeId)
        {
            try
            {
                var isExist = await _menuItemCategoryRepository.AsQueryable().AnyAsync(x => menuItemIds.Contains(x.MenuItemId) && x.CategoryId == cateId);

                ConditionCheck.CheckCondition(!isExist, Errors.Common.IsExist);

                var menuItemCategories = menuItemIds.Select(x => new MenuItemCategories
                {
                    Id = Guid.NewGuid(),
                    CategoryId = cateId,
                    MenuItemId = x,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = storeId.ToString()
                });

                await _menuItemCategoryRepository.AddRangeAsync(menuItemCategories);
                await _menuItemCategoryRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<CategoryMenuItemDetailResponse> GetCategoryWithMenuItemDetail(Guid cateId, Guid StoreId)
        {
            var cateWithMenuItem = await _menuItemCategoryRepository.AsQueryable().Include(x => x.Category).Include(x => x.MenuItem).Where(x => x.CategoryId == cateId).ToListAsync();

            if(cateWithMenuItem == null) return new CategoryMenuItemDetailResponse();

            return new CategoryMenuItemDetailResponse
            {
                Id = cateId,
                Description = cateWithMenuItem.FirstOrDefault()?.Category?.Description,
                Name = cateWithMenuItem.FirstOrDefault().Category.Name,
                IsActive = cateWithMenuItem.FirstOrDefault().Category.IsActive,
                MenuItems = cateWithMenuItem.Select(x => new MenuItemWithCategory
                {
                    Id = x.MenuItem.Id,
                    Description = x.MenuItem.Description,
                    Name = x.MenuItem.Name,
                    IsAvailable = x.MenuItem.IsAvailable
                }).ToList()
            };
        }

        public async Task<PagedResult<CategoryMenuItemDetailResponse>> ListCategoriesWithMenuItems(UrlQueryParameters urlQueryParameters, Guid storeId)
        {
            var menuItemCategoryQuery = _menuItemCategoryRepository.AsQueryable().Include(p => p.MenuItem).Include(x => x.Category)
                .Where(p => p.CreatedBy == storeId.ToString());

            menuItemCategoryQuery = ApplyFilters(menuItemCategoryQuery, urlQueryParameters);
            menuItemCategoryQuery = ApplySearch(menuItemCategoryQuery, urlQueryParameters);
            menuItemCategoryQuery = ApplySort(menuItemCategoryQuery, urlQueryParameters);

            var total = await menuItemCategoryQuery.CountAsync();
            var items = await menuItemCategoryQuery
                .Skip((urlQueryParameters.Page - 1) * urlQueryParameters.PageSize)
                .Take(urlQueryParameters.PageSize)
            .ToListAsync();

            var mapped = items.GroupBy(x => x.Category)
                 .Select(group => new CategoryMenuItemDetailResponse
                 {
                     Id = group.Key.Id,
                     Name = group.Key.Name,
                     Description = group.Key.Description,
                     IsActive = group.Key.IsActive,
                     MenuItems = group.Select(x => new MenuItemWithCategory
                     {
                         Id = x.MenuItem.Id,
                         Name = x.MenuItem.Name,
                         Description = x.MenuItem.Description,
                         IsAvailable = x.MenuItem.IsAvailable
                     }).ToList()
                 }).ToList();

            return new PagedResult<CategoryMenuItemDetailResponse>(mapped, total, urlQueryParameters.Page, urlQueryParameters.PageSize);
        }

        public async Task<bool> RemoveMenuItemFromCategory(Guid cateId, List<Guid> menuItemIds)
        {
            try
            {
                var found = await _menuItemCategoryRepository.AsQueryable().Where(x => menuItemIds.Contains(x.MenuItemId) && x.CategoryId == cateId).ToListAsync();

                ConditionCheck.CheckCondition(found != null, Errors.Common.NotFound);

                _menuItemCategoryRepository.RemoveRange(found!);
                await _menuItemCategoryRepository.SaveChangesAsync();

                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> RemoveCategoriesFromProduct(RemoveCategoriesFromProductRequest request, string storeId)
        {
            try
            {
                var found = await _menuItemCategoryRepository.AsQueryable().Where(x => request.CateIds.Contains(x.CategoryId) && x.MenuItemId == request.MenuItemId).ToListAsync();

                ConditionCheck.CheckCondition(found != null, Errors.Common.NotFound);

                _menuItemCategoryRepository.RemoveRange(found!);
                await _menuItemCategoryRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static IQueryable<MenuItemCategories> ApplyFilters(IQueryable<MenuItemCategories> query, UrlQueryParameters parameters)
        {
            if (parameters.Filters?.Any() != true) return query;

            foreach (var (key, value) in parameters.Filters)
            {
                
            }

            return query;
        }

        private static IQueryable<MenuItemCategories> ApplySearch(IQueryable<MenuItemCategories> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SearchBy) || string.IsNullOrWhiteSpace(parameters.SearchValue))
                return query;

            var searchValue = parameters.SearchValue.ToLowerInvariant();

            return parameters.SearchBy.ToLowerInvariant() switch
            {
               
            };
        }

        private static IQueryable<MenuItemCategories> ApplySort(IQueryable<MenuItemCategories> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy)) return query;

            var isDescending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            return parameters.SortBy.ToLowerInvariant() switch
            {
               
            };
        }
    }
}
