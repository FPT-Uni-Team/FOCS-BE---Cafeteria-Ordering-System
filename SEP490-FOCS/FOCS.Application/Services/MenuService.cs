using AutoMapper;
using FOCS.Application.DTOs;
using FOCS.Order.Infrastucture.Common.Repositories;
using FOCS.Order.Infrastucture.Common.UnitOfWorks;
using FOCS.Order.Infrastucture.Context;
using FOCS.Order.Infrastucture.Entities;
using FOCS.Order.Infrastucture.Interfaces;
using Microsoft.Extensions.Logging;

namespace FOCS.Application.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MenuService> _logger;
        private readonly MenuRepository _menuRepository;

        public MenuService(MenuRepository menuRepository, IMapper mapper, ILogger<MenuService> logger)
        {
            _menuRepository = menuRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public Task<List<MenuItemVariant>> GetMenuByStore(Guid storeId)
        {
            try
            {
                _logger.LogInformation("Fetching menu for store {StoreId}", storeId);
                var menuItems = _menuRepository.GetMenuByStore(storeId);
                _logger.LogInformation("Successfully fetched {Count} menu items for store {StoreId}",
                                    menuItems.Result.Count, storeId);
                return menuItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching menu for store {StoreId}", storeId);
                throw;
            }
        }
    }
}
