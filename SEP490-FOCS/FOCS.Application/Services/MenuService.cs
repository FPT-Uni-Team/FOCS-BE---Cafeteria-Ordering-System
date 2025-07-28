using AutoMapper;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Order.Infrastucture.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace FOCS.Application.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MenuService> _logger;
        private readonly IRepository<MenuItem> _menuItemRepository;

        public MenuService(IRepository<MenuItem> menuRepository, IMapper mapper, ILogger<MenuService> logger)
        {
            _menuItemRepository = menuRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<MenuItemDTO>> GetMenuItemByStore(UrlQueryParameters urlQueryParameters, Guid storeId)
        {
            try
            {
                var menuItemsQuery = _menuItemRepository.AsQueryable()
                                                        .Where(x => x.StoreId == storeId)
                                                        .Include(mi => mi.Images)
                                                        .Include(mi => mi.MenuItemCategories).ThenInclude(mic => mic.Category)
                                                        .Include(mi => mi.MenuItemVariantGroups).ThenInclude(mivg => mivg.VariantGroup)
                                                        .Include(mi => mi.MenuItemVariantGroups)
                                                            .ThenInclude(mivg => mivg.MenuItemVariantGroupItems)
                                                            .ThenInclude(mivgi => mivgi.MenuItemVariant);

                var totalItems = await menuItemsQuery.CountAsync();

                int pageIndex = urlQueryParameters.Page;
                int pageSize = urlQueryParameters.PageSize;

                IQueryable<MenuItem> pagedQuery;

                if (pageIndex > 0 && pageSize > 0)
                {
                    int skip = (pageIndex - 1) * pageSize;
                    pagedQuery = menuItemsQuery.Skip(skip).Take(pageSize);
                }
                else
                {
                    pagedQuery = menuItemsQuery;
                }

                var data = await pagedQuery.Select(x => new MenuItemDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    BasePrice = x.BasePrice,
                    Description = x.Description,
                    Images = x.Images.FirstOrDefault().Url,
                    IsAvailable = x.IsAvailable,
                    MenuCategories = x.MenuItemCategories.Select(y => new MenuCategoryDTO
                    {
                        Id = y.Category.Id,
                        Description = y.Category.Description,
                        IsActive = y.Category.IsActive,
                        Name = y.Category.Name
                    }).ToList(),
                    VariantGroups = x.MenuItemVariantGroups.Select(z => new VariantGroupDTO
                    {
                        Id = z.Id,
                        name = z.VariantGroup.Name,
                        MinSelect = z.MinSelect,
                        MaxSelect = z.MaxSelect,
                        IsRequired = z.IsRequired,
                        Variants = z.MenuItemVariantGroupItems.Select(c => new MenuItemVariantDTO
                        {
                            Id = c.MenuItemVariant.Id,
                            Name = c.MenuItemVariant.Name,
                            IsAvailable = c.MenuItemVariant.IsAvailable,
                            Price = c.MenuItemVariant.Price,
                        }).ToList()
                    }).ToList()
                }).ToListAsync();

                return new PagedResult<MenuItemDTO>(data, totalItems, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching menu for store {StoreId}", storeId);
                return new PagedResult<MenuItemDTO>([], 0, 0, 0);
            }
        }

        public async Task<List<MenuItemDTO>> GetMenuItemByIds(List<Guid> ids, Guid storeId)
        {
            try
            {
                var menuItemsQuery = _menuItemRepository.AsQueryable()
                                                        .Where(x => x.StoreId == storeId && ids.Contains(x.Id))
                                                        .Include(mi => mi.Images)
                                                        .Include(mi => mi.MenuItemCategories).ThenInclude(mic => mic.Category)
                                                        .Include(mi => mi.MenuItemVariantGroups).ThenInclude(mivg => mivg.VariantGroup)
                                                        .Include(mi => mi.MenuItemVariantGroups)
                                                            .ThenInclude(mivg => mivg.MenuItemVariantGroupItems)
                                                            .ThenInclude(mivgi => mivgi.MenuItemVariant);

                var data = await menuItemsQuery.Select(x => new MenuItemDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    BasePrice = x.BasePrice,
                    Description = x.Description,
                    Images = x.Images.FirstOrDefault().Url,
                    IsAvailable = x.IsAvailable,
                    MenuCategories = x.MenuItemCategories.Select(y => new MenuCategoryDTO
                    {
                        Id = y.Category.Id,
                        Description = y.Category.Description,
                        IsActive = y.Category.IsActive,
                        Name = y.Category.Name
                    }).ToList(),
                    VariantGroups = x.MenuItemVariantGroups.Select(z => new VariantGroupDTO
                    {
                        Id = z.Id,
                        name = z.VariantGroup.Name,
                        MinSelect = z.MinSelect,
                        MaxSelect = z.MaxSelect,
                        IsRequired = z.IsRequired,
                        Variants = z.MenuItemVariantGroupItems.Select(c => new MenuItemVariantDTO
                        {
                            Id = c.MenuItemVariant.Id,
                            Name = c.MenuItemVariant.Name,
                            IsAvailable = c.MenuItemVariant.IsAvailable,
                            Price = c.MenuItemVariant.Price,
                        }).ToList()
                    }).ToList()
                }).ToListAsync();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching menu for store {StoreId}", storeId);
                return new List<MenuItemDTO>();
            }
        }

        public async Task<MenuItemDTO> GetItemVariant(Guid itemId)
        {
            try
            {
                _logger.LogInformation("Fetching detail info for item {ItemId}", itemId);
                var menuItems = await _menuItemRepository.IncludeAsync(source => source
                                                                .Where(m => m.Id == itemId));
                                                                //.Include(m => m.VariantGroups));
                                                              //  .ThenInclude(v => v.Variants));
                var data = _mapper.Map<MenuItemDTO>(menuItems.FirstOrDefault());
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetching detail info for item {ItemId}", itemId);
                return null;
            }
        }
    }
}