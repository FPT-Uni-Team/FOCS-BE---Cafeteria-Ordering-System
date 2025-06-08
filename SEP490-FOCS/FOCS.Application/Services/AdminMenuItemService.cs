using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Application.Services
{
    public class AdminMenuItemService : IAdminMenuItemService
    {
        private readonly IRepository<MenuItem> _menuRepository;
        private readonly IMapper _mapper;

        public AdminMenuItemService(IRepository<MenuItem> menuRepository, IMapper mapper)
        {
            _menuRepository = menuRepository;
            _mapper = mapper;
        }

        public async Task<MenuItemAdminServiceDTO> CreateMenuAsync(MenuItemAdminServiceDTO dto, string userId)
        {
            var newItem = _mapper.Map<MenuItem>(dto);

            // Ensure the new item has required properties set
            newItem.Id = Guid.NewGuid();
            newItem.IsDeleted = false;
            newItem.CreatedAt = DateTime.UtcNow;
            newItem.CreatedBy = userId;

            await _menuRepository.AddAsync(newItem);
            await _menuRepository.SaveChangesAsync();

            return _mapper.Map<MenuItemAdminServiceDTO>(newItem);
        }

        public async Task<PagedResult<MenuItemAdminServiceDTO>> GetAllMenuItemAsync(UrlQueryParameters query, Guid storeId)
        {
            var menuQuery = _menuRepository.AsQueryable()
                .Include(m => m.MenuCategory)
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
                    else if (filter.Key.Equals("category", StringComparison.OrdinalIgnoreCase))
                    {
                        menuQuery = menuQuery.Where(m => m.MenuCategory.Name == filter.Value);
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
                    "category" => descending ? menuQuery.OrderByDescending(m => m.MenuCategory.Name) : menuQuery.OrderBy(m => m.MenuCategory.Name),
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

            var mappedItems = _mapper.Map<List<MenuItemAdminServiceDTO>>(items);

            return new PagedResult<MenuItemAdminServiceDTO>(mappedItems, totalCount, query.Page, query.PageSize);
        }


        public async Task<MenuItemAdminServiceDTO?> GetMenuDetailAsync(Guid id)
        {
            var items = await _menuRepository.FindAsync(m => m.Id == id && !m.IsDeleted);
            var item = items.FirstOrDefault();

            return item != null ? _mapper.Map<MenuItemAdminServiceDTO>(item) : null;
        }

        public async Task<bool> UpdateMenuAsync(Guid id, MenuItemAdminServiceDTO dto, string userId)
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

        public async Task<MenuItemDetailAdminServiceDTO> GetMenuItemDetail(Guid menuItemId, string storeId)
        {
            var menuItems = await _menuRepository.IncludeAsync(source => source
                                                               .Where(m => m.StoreId == Guid.Parse(storeId) && m.Id == menuItemId)
                                                               .Include(m => m.MenuCategory)
                                                               .Include(m => m.VariantGroups)
                                                               .ThenInclude(v => v.Variants));


            return _mapper.Map<MenuItemDetailAdminServiceDTO>(menuItems.FirstOrDefault());
        }
    }
}
