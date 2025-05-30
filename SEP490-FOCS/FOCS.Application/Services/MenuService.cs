using AutoMapper;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Context;
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
        private readonly IRepository<MenuItem> _menuRepository;

        public MenuService(IRepository<MenuItem> menuRepository, IMapper mapper, ILogger<MenuService> logger)
        {
            _menuRepository = menuRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<MenuItemDTO>> GetMenuItemByStore(UrlQueryParameters urlQueryParameters, Guid storeId)
        {
            try
            {
                _logger.LogInformation("Fetching menu for store {StoreId}", storeId);
                var menuItems = await _menuRepository.IncludeAsync(
                                        source => source
                                                    .Include(m => m.MenuCategory)
                                                    .Include(m => m.VariantGroups).ThenInclude(v => v.Variants));

                var search = urlQueryParameters.SearchValue;
                if (!string.IsNullOrEmpty(search))
                {
                    menuItems = menuItems.Where(s => s.Name.Contains(search));
                }

                if (urlQueryParameters.Filters != null)
                {
                    foreach (var item in urlQueryParameters.Filters)
                    {
                        if (item.Key.Equals("Price"))
                        {
                            menuItems = menuItems.Where(m => m.BasePrice > Double.Parse(item.Value));
                        }
                        if (item.Key.Equals("Category"))
                        {
                            menuItems = menuItems.Where(m => m.MenuCategory.Name.Equals(item.Value));
                        }
                    }
                }

                if (urlQueryParameters.SortBy != null)
                {
                    Expression<Func<MenuItem, object>> sortSelector = urlQueryParameters.SortBy switch
                    {
                        "Name" => m => m.Name,
                        "Price" => m => m.BasePrice,
                        "Category" => m => m.MenuCategory.Name,
                        _ => m => m.Id // Default sort
                    };

                    menuItems = urlQueryParameters.SortOrder.Equals("ASC", StringComparison.OrdinalIgnoreCase)
                        ? menuItems.OrderBy(sortSelector)
                        : menuItems.OrderByDescending(sortSelector);
                }

                var totalItems = menuItems.Count();

                List<MenuItemDTO> data;
                int totalPages = 0;
                var pageIndex = urlQueryParameters.Page;
                var pageSize = urlQueryParameters.PageSize;
                if (pageIndex == 0 && pageSize == 0)
                {
                    data = _mapper.Map<List<MenuItemDTO>>(menuItems);
                }
                else
                {
                    var pageStart = pageIndex - 1;
                    totalPages = (int)Math.Ceiling(totalItems / (float)pageSize);
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
                return null;
            }
        }
    }
}
