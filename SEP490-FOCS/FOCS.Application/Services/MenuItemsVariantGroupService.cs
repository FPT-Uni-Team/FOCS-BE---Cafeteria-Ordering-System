using AutoMapper;
using FOCS.Application.DTOs;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace FOCS.Application.Services
{
    public class MenuItemsVariantGroupService : IMenuItemsVariantGroupService
    {
        private readonly IRepository<MenuItemVariantGroup> _menuItemVariantGroupRepository;
        private readonly IRepository<MenuItemVariantGroupItem> _menuItemVariantGroupItemRepository;

        private readonly IMapper _mapper;

        public MenuItemsVariantGroupService(IMapper mapper, IRepository<MenuItemVariantGroup> menuItemVariantGroupRepo, IRepository<MenuItemVariantGroupItem> menuItemVariantGroupItemRepository)
        {
            _menuItemVariantGroupRepository = menuItemVariantGroupRepo;
            _mapper = mapper;
            _menuItemVariantGroupItemRepository = menuItemVariantGroupItemRepository;
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

        public async Task<List<VariantGroupResponse>> GetVariantGroupsWithVariants(Guid menuItemId, Guid storeId)
        {
            var variantGroups = await _menuItemVariantGroupRepository
                .AsQueryable()
                .Where(x => x.MenuItemId == menuItemId)
                .Include(x => x.VariantGroup)
                .Include(x => x.MenuItemVariantGroupItems)
                    .ThenInclude(i => i.MenuItemVariant)
                .ToListAsync();

            ConditionCheck.CheckCondition(variantGroups.Any(), Errors.Common.NotFound);

            var result = variantGroups.Select(vg =>
            {
                var dto = _mapper.Map<VariantGroupResponse>(vg.VariantGroup);
                dto.PrepPerTime = 1;
                dto.QuantityPerTime = 1;
                dto.Variants = _mapper.Map<List<MenuItemVariantDTO>>(
                    vg.MenuItemVariantGroupItems.Select(i => i.MenuItemVariant).ToList()
                );
                return dto;
            }).ToList();

            return result;
        }

        public async Task<bool> RemoveVariantGroupsFromProduct(RemoveVariantGroupFromProduct request, Guid menuItemId, string storeId)
        {
            try
            {
                var removeObjects = await _menuItemVariantGroupRepository.AsQueryable().Where(x => x.MenuItemId == menuItemId && request.VariantGroupIds.Contains(x.VariantGroupId)).ToListAsync();

                _menuItemVariantGroupRepository.RemoveRange(removeObjects);

                await _menuItemVariantGroupRepository.SaveChangesAsync();

                return true;
            } catch (Exception ex) { return false; }
        }
    }
}
