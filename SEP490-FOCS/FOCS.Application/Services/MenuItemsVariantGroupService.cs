using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
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
    public class MenuItemsVariantGroupService : IMenuItemsVariantGroupService
    {
        private readonly IRepository<MenuItemVariantGroup> _menuItemVariantGroupRepository;

        private readonly IMapper _mapper;

        public MenuItemsVariantGroupService(IMapper mapper, IRepository<MenuItemVariantGroup> menuItemVariantGroupRepo)
        {
            _menuItemVariantGroupRepository = menuItemVariantGroupRepo;
            _mapper = mapper;
        }

        public async Task<List<MenuItemVariantGroup>> AssignMenuItemToVariantGroup(CreateMenuItemVariantGroupRequest request)
        {
            try
            {
                var newMenuItemVariantGroups = request.VariantGroupIds
                                           .Select(variantGroupId => new MenuItemVariantGroup
                                           {
                                               Id = Guid.NewGuid(),
                                               MenuItemId = request.MenuItemId.Value,
                                               VariantGroupId = variantGroupId,
                                               MinSelect = request.MinSelect,
                                               MaxSelect = request.MaxSelect,
                                               IsRequired = request.IsRequired
                                           })
                                           .ToList();

                await _menuItemVariantGroupRepository.AddRangeAsync(newMenuItemVariantGroups);
                await _menuItemVariantGroupRepository.SaveChangesAsync();

                return newMenuItemVariantGroups;
            } catch (Exception ex)
            {
                return new List<MenuItemVariantGroup>();
            }
        }
    }
}
