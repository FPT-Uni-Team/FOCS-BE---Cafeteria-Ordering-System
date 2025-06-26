using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        public AdminMenuItemService(IRepository<MenuItem> menuRepository,
            IRepository<Store> storeRepository,
            UserManager<User> userManager,
            IRepository<UserStore> userStoreRepository,
            IMapper mapper, 
            IRepository<Category> menuCategory)
        {
            _menuRepository = menuRepository;
            _storeRepository = storeRepository;
            _userManager = userManager;
            _userStoreRepository = userStoreRepository;
            _mapper = mapper;
            _menuCategory = menuCategory;
        }

        public async Task<MenuItemAdminDTO> CreateMenuAsync(MenuItemAdminDTO dto, string storeId)
        {
            var isExist = await _menuRepository.AsQueryable().AnyAsync(x => x.Name == dto.Name);

            ConditionCheck.CheckCondition(!isExist, Errors.Common.IsExist, "name");

            var newItem = _mapper.Map<MenuItem>(dto);

            // Ensure the new item has required properties set
            newItem.Id = Guid.NewGuid();
            newItem.IsDeleted = false;
            newItem.CreatedAt = DateTime.UtcNow;
            newItem.CreatedBy = storeId;

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
            var menuItems = await _menuRepository.AsQueryable().FirstOrDefaultAsync(m => m.StoreId == Guid.Parse(storeId) && m.Id == menuItemId);


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
