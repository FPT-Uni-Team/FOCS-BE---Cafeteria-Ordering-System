using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class MenuItemsVariantGroupItemService : IMenuItemsVariantGroupItemService
    {
        private readonly IRepository<MenuItemVariantGroupItem> _menuItemVariantGroupItemRepo;

        public MenuItemsVariantGroupItemService(IRepository<MenuItemVariantGroupItem> menuItemVariantGroupItemRepo)
        {
            _menuItemVariantGroupItemRepo = menuItemVariantGroupItemRepo;
        }

        public async Task<bool> AssignMenuItemVariantGroupToMenuItemVariantItemGroup(CreateMenuItemVariantGroupItemRequest request)
        {
            try
            {
                var newMenuItemVariantGroupItems = request.MenuItemVariantGroupItemRequests
                    .Where(x => x.VariantIds != null && x.VariantIds.Any())
                    .SelectMany(group => group.VariantIds.Select(variantId => new MenuItemVariantGroupItem
                    {
                        Id = Guid.NewGuid(),
                        MenuItemVariantGroupId = group.MenuItemVariantGroupId,
                        MenuItemVariantId = variantId,
                        IsActive = true
                    }))
                    .ToList();

                await _menuItemVariantGroupItemRepo.AddRangeAsync(newMenuItemVariantGroupItems);
                await _menuItemVariantGroupItemRepo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
