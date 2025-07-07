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
            var variantGroups = await _menuItemVariantGroupRepository.AsQueryable().Include(x => x.VariantGroup)
                                                                        .Where(x => x.MenuItemId == menuItemId).ToListAsync();

            ConditionCheck.CheckCondition(variantGroups != null, Errors.Common.NotFound);

            var variantGroupResponses = new List<VariantGroupResponse>();

            foreach (var variantGroup in variantGroups)
            {
                var variants = await _menuItemVariantGroupItemRepository
                    .AsQueryable()
                    .Where(x => x.MenuItemVariantGroupId == variantGroup.Id)
                    .Include(v => v.MenuItemVariant)
                    .Select(v => v.MenuItemVariant)
                    .ToListAsync();

                var variantGroupDTO = _mapper.Map<VariantGroupResponse>(variantGroup.VariantGroup);
                variantGroupDTO.Variants = _mapper.Map<List<MenuItemVariantDTO>>(variants);

                variantGroupResponses.Add(variantGroupDTO);
            }

            return variantGroupResponses;
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
