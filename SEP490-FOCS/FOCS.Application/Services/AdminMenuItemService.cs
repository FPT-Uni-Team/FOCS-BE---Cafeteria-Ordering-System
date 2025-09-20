using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;

namespace FOCS.Application.Services
{
    public class AdminMenuItemService : IAdminMenuItemService
    {
        private readonly IRepository<MenuItem> _menuRepository;
        private readonly IRepository<Category> _menuCategory;

        private readonly IRepository<UserStore> _userStoreRepository;

        private readonly IRepository<Store> _storeRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IRepository<MenuItemImage> _menuItemImageRepository;
        public AdminMenuItemService(IRepository<MenuItem> menuRepository,
            IRepository<Store> storeRepository,
            UserManager<User> userManager,
            IRepository<UserStore> userStoreRepository,
            IMapper mapper,
            IRepository<Category> menuCategory,
            IRepository<MenuItemImage> menuItemImageRepository)
        {
            _menuRepository = menuRepository;
            _storeRepository = storeRepository;
            _userManager = userManager;
            _userStoreRepository = userStoreRepository;
            _mapper = mapper;
            _menuCategory = menuCategory;
            _menuItemImageRepository = menuItemImageRepository;
        }

        public async Task<MenuItemAdminDTO> CreateMenuAsync(MenuItemAdminDTO dto, string storeId)
        {
            var isExist = await _menuRepository.AsQueryable().AnyAsync(x => x.Name == dto.Name && !x.IsDeleted);

            ConditionCheck.CheckCondition(!isExist, Errors.Common.IsExist, "name");

            var newItem = _mapper.Map<MenuItem>(dto);

            // Ensure the new item has required properties set
            newItem.Id = Guid.NewGuid();
            newItem.IsDeleted = false;
            newItem.CreatedAt = DateTime.UtcNow;
            newItem.CreatedBy = storeId;
            newItem.IsActive = true;

            await _menuRepository.AddAsync(newItem);

            await _menuRepository.SaveChangesAsync();

            return _mapper.Map<MenuItemAdminDTO>(newItem);
        }

        public async Task<PagedResult<MenuItemAdminDTO>> GetAllMenuItemAsync(UrlQueryParameters query, Guid storeId)
        {
            var menuQuery = _menuRepository.AsQueryable()
                .Where(m => !m.IsDeleted && m.StoreId == storeId);

            // search
            if (!string.IsNullOrEmpty(query.SearchBy) && !string.IsNullOrEmpty(query.SearchValue))
            {
                if (query.SearchBy.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    menuQuery = menuQuery.Where(m => m.Name.Contains(query.SearchValue));
                }
                else if (query.SearchBy.Equals("description", StringComparison.OrdinalIgnoreCase))
                {
                    menuQuery = menuQuery.Where(m => m.Description.Contains(query.SearchValue));
                }
            }

            // filters
            if (query.Filters != null)
            {
                foreach (var filter in query.Filters)
                {
                    if (filter.Key.Equals("price", StringComparison.OrdinalIgnoreCase) && double.TryParse(filter.Value, out var price))
                    {
                        menuQuery = menuQuery.Where(m => m.BasePrice > price);
                    }
                }
            }

            // sort
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                bool descending = query.SortOrder?.ToLower() == "desc";
                menuQuery = query.SortBy.ToLower() switch
                {
                    "name" => descending ? menuQuery.OrderByDescending(m => m.Name) : menuQuery.OrderBy(m => m.Name),
                    "base_price" => descending ? menuQuery.OrderByDescending(m => m.BasePrice) : menuQuery.OrderBy(m => m.BasePrice),
                    _ => menuQuery
                };
            } else
            {
                menuQuery.OrderBy(x => x.IsAvailable);
            }

            // total count
            var totalCount = await menuQuery.CountAsync();

            // pagination
            var items = await menuQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var mappedItems = _mapper.Map<List<MenuItemAdminDTO>>(items);

            return new PagedResult<MenuItemAdminDTO>(mappedItems, totalCount, query.Page, query.PageSize);
        }


        public async Task<MenuItemAdminDTO?> GetMenuDetailAsync(Guid id)
        {
            var items = await _menuRepository.FindAsync(m => m.Id == id && !m.IsDeleted);
            var item = items.FirstOrDefault();

            return item != null ? _mapper.Map<MenuItemAdminDTO>(item) : null;
        }

        public async Task<bool> UpdateMenuAsync(Guid id, MenuItemAdminDTO dto, string userId)
        {
            var item = await _menuRepository.GetByIdAsync(id);
            if (item == null || item.IsDeleted) return false;

            _mapper.Map(dto, item);

            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = userId;

            await _menuRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMenuAsync(Guid id, string userId)
        {
            var item = await _menuRepository.GetByIdAsync(id);
            if (item == null || item.IsDeleted) return false;

            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = userId;

            await _menuRepository.SaveChangesAsync();
            return true;
        }

        public async Task<MenuItemDetailAdminDTO> GetMenuItemDetail(Guid menuItemId, string storeId)
        {
            var menuItems = await _menuRepository.AsQueryable().Include(x => x.Images).FirstOrDefaultAsync(m => m.StoreId == Guid.Parse(storeId) && m.Id == menuItemId);


            return _mapper.Map<MenuItemDetailAdminDTO>(menuItems);
        }

        public async Task<List<MenuItemDetailAdminDTO>> GetListMenuItemDetail(List<Guid> menuItemIds, string storeId, string userId)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(storeId, out Guid storeIdGuid), Errors.Common.InvalidGuidFormat);
            await ValidateUser(userId, storeIdGuid);
            await ValidateStoreExists(storeIdGuid);
            var menuItems = await _menuRepository.AsQueryable()
                                .Where(m => m.StoreId == storeIdGuid && menuItemIds.Contains(m.Id)).ToListAsync();

            return _mapper.Map<List<MenuItemDetailAdminDTO>>(menuItems);
        }

        public async Task<bool> ActivateMenuItemAsync(Guid menuItemId, string userId)
        {
            return await UpdateMenuItemStatusAsync(menuItemId, userId, isActive: true,
                condition: item => !item.IsActive,
                error: Errors.MenuItemError.MenuItemActive,
                fieldName: Errors.FieldName.IsActive);
        }

        public async Task<bool> DeactivateMenuItemAsync(Guid menuItemId, string userId)
        {
            return await UpdateMenuItemStatusAsync(menuItemId, userId, isActive: false,
                condition: item => item.IsActive,
                error: Errors.MenuItemError.MenuItemInactive,
                fieldName: Errors.FieldName.IsActive);
        }

        public async Task<bool> EnableMenuItemAsync(Guid menuItemId, string userId)
        {
            return await UpdateMenuItemStatusAsync(menuItemId, userId, isAvailable: true,
                condition: item => !item.IsAvailable,
                error: Errors.MenuItemError.MenuItemAvailable,
                fieldName: Errors.FieldName.IsAvailable);
        }

        public async Task<bool> DisableMenuItemAsync(Guid menuItemId, string userId)
        {
            return await UpdateMenuItemStatusAsync(menuItemId, userId, isAvailable: false,
                condition: item => item.IsAvailable,
                error: Errors.MenuItemError.MenuItemUnavailable,
                fieldName: Errors.FieldName.IsAvailable);
        }

        private async Task<bool> UpdateMenuItemStatusAsync(Guid menuItemId, string userId,
            bool? isActive = null, bool? isAvailable = null,
            Func<MenuItem, bool> condition = null, string error = null, string fieldName = null)
        {
            var menuItem = await GetMenuItemAsync(menuItemId, userId);
            if (menuItem == null) return false;

            ConditionCheck.CheckCondition(condition(menuItem), error, fieldName);

            if (isActive.HasValue)
                menuItem.IsActive = isActive.Value;
            if (isAvailable.HasValue)
                menuItem.IsAvailable = isAvailable.Value;

            menuItem.UpdatedAt = DateTime.UtcNow;
            menuItem.UpdatedBy = userId;

            await _menuRepository.SaveChangesAsync();
            return true;
        }

        private async Task<MenuItem?> GetMenuItemAsync(Guid menuItemId, string userId)
        {
            var menuItem = await _menuRepository.AsQueryable()
                                            .Where(x => x.Id == menuItemId && !x.IsDeleted).FirstOrDefaultAsync();
            if (menuItem == null) return null;

            var user = await _userManager.FindByIdAsync(userId);

            var storesOfUser = (await _userStoreRepository.FindAsync(x => x.UserId == Guid.Parse(userId))).Distinct().ToList();

            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound, Errors.FieldName.UserId);
            ConditionCheck.CheckCondition(storesOfUser.Select(x => x.StoreId).Contains(menuItem.StoreId), Errors.AuthError.UserUnauthor, Errors.FieldName.UserId);

            return menuItem;
        }

        #region Private Helper Methods

        private async Task ValidateUser(string userId, Guid storeId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var storesOfUser = (await _userStoreRepository.FindAsync(x => x.UserId == Guid.Parse(userId))).Distinct().ToList();

            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound);
            ConditionCheck.CheckCondition(storesOfUser.Select(x => x.StoreId).Contains(storeId), Errors.AuthError.UserUnauthor);
        }

        private async Task ValidateStoreExists(Guid storeId)
        {
            var store = await _storeRepository.GetByIdAsync(storeId);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound);
        }

        #endregion
    }
}
