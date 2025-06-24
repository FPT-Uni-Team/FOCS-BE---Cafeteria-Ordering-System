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
                _logger.LogInformation("Fetching menu for store {StoreId}", storeId);
                var menuItems = await _menuItemRepository.IncludeAsync(source => source
                                                                .Where(m => m.StoreId == storeId)
                                                                .Include(m => m.VariantGroups));
                                                                //.ThenInclude(v => v.Variants));

                if (!string.IsNullOrWhiteSpace(urlQueryParameters.SearchValue))
                {
                    var search = urlQueryParameters.SearchValue.Trim();
                    menuItems = menuItems.Where(s => s.Name.ToLower().Contains(search.ToLower()));
                }

                if (urlQueryParameters.Filters?.Any() == true)
                {
                    foreach (var (key, value) in urlQueryParameters.Filters)
                    {
                        menuItems = key switch
                        {
                            "Price" when double.TryParse(value, out var price) =>
                                menuItems.Where(m => m.BasePrice > price),
                            _ => menuItems
                        };
                    }
                }

                if (urlQueryParameters.SortBy != null)
                {
                    Expression<Func<MenuItem, object>> sortSelector = urlQueryParameters.SortBy switch
                    {
                        "Name" => m => m.Name,
                        "Price" => m => m.BasePrice,
                        _ => m => m.Id // Default sort
                    };

                    menuItems = urlQueryParameters.SortOrder.Equals("ASC", StringComparison.OrdinalIgnoreCase)
                        ? menuItems.OrderBy(sortSelector)
                        : menuItems.OrderByDescending(sortSelector);
                }

                var totalItems = menuItems.Count();

                List<MenuItemDTO> data;
                var pageIndex = urlQueryParameters.Page;
                var pageSize = urlQueryParameters.PageSize;
                if (pageIndex == 0 && pageSize == 0)
                {
                    data = _mapper.Map<List<MenuItemDTO>>(menuItems);
                }
                else
                {
                    var pageStart = pageIndex - 1;
                    data = _mapper.Map<List<MenuItemDTO>>(
                        menuItems
                        .Skip(pageStart * pageSize)
                        .Take(pageSize));
                }
                data = data.ToList();
                var pagedResponse = new PagedResult<MenuItemDTO>(data, totalItems, pageIndex, pageSize);
                return pagedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching menu for store {StoreId}", storeId);
                return new PagedResult<MenuItemDTO>([], 0, 0, 0);
            }
        }

        public async Task<MenuItemDTO> GetItemVariant(Guid itemId)
        {
            try
            {
                _logger.LogInformation("Fetching detail info for item {ItemId}", itemId);
                var menuItems = await _menuItemRepository.IncludeAsync(source => source
                                                                .Where(m => m.Id == itemId)
                                                                .Include(m => m.VariantGroups));
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