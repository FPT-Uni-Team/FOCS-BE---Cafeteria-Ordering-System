using AutoMapper;
using FOCS.Application.Services.Interface;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class VariantGroupService : IVariantGroupService
    {
        private readonly IMenuItemVariantService _menuItemVariantService;
        private readonly IAdminMenuItemService _menuItemService;

        private readonly IRepository<VariantGroup> _variantGroup;
        private readonly IMapper _mapper;

        public VariantGroupService(IMenuItemVariantService menuItemVariantService, IMapper mapper, IAdminMenuItemService menuItemService, IRepository<VariantGroup> variantGroup)
        {
            _menuItemService = menuItemService;
            _menuItemVariantService = menuItemVariantService;
            _variantGroup = variantGroup;
            _mapper = mapper;
        }

        public async Task<bool> AddMenuItemVariantToGroupAsync(AddVariantToGroupRequest request, Guid storeId)
        {
            try
            {
                var menuItem = await _menuItemService.GetMenuDetailAsync(request.MenuItemId);
                ConditionCheck.CheckCondition(menuItem != null, Errors.Common.NotFound);

                var variants = await _menuItemVariantService.ListVariantsWithIds(request.VariantIds, storeId);

                var groupNameExist = await _variantGroup.AsQueryable().AnyAsync(x => x.name == request.GroupName);
                ConditionCheck.CheckCondition(!groupNameExist, Errors.Common.NotFound);

                var newGroupVariant = new VariantGroup
                {
                    id = Guid.NewGuid(),
                    name = request.GroupName,
                    IsRequired = request.IsRequired,
                    MaxSelect = request.MaxSelect,
                    MinSelect = request.MinSelect,
                    MenuItemId = request.MenuItemId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = storeId.ToString()
                };

                await _variantGroup.AddAsync(newGroupVariant);

                await _menuItemVariantService.AssignVariantGroupToVariants(request.VariantIds, newGroupVariant.id);

                await _variantGroup.SaveChangesAsync();
            } catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<List<string>> GetGroupNamesByMenuItemAsync(Guid menuItemId)
        {
            var menuItem = await _menuItemService.GetMenuDetailAsync(menuItemId);
            ConditionCheck.CheckCondition(menuItem != null, Errors.Common.NotFound);

            return await _variantGroup.AsQueryable().Where(x => x.MenuItemId == menuItemId).Select(x => x.name).ToListAsync();
        }

        public async Task<List<VariantGroupDetailDTO>> GetVariantGroupsByMenuItemAsync(Guid menuItemId)
        {
            var menuItem = await _menuItemService.GetMenuDetailAsync(menuItemId);
            ConditionCheck.CheckCondition(menuItem != null, Errors.Common.NotFound);

            var groups = await _variantGroup.AsQueryable().Include(x => x.Variants).Where(x => x.MenuItemId == menuItemId).ToListAsync();

            return groups.Select(x => new VariantGroupDetailDTO
            {
                GroupName = x.name,
                IsRequired = x.IsRequired,
                MaxSelect = x.MaxSelect,
                MinSelect = x.MinSelect,
                Variants = _mapper.Map<List<VariantOptionDTO>>(x.Variants)
            }).ToList();
        }

        public async Task<bool> RemoveVariantFromGroupAsync(Guid variantGroupId)
        {
            try
            {
                var group = await _variantGroup.GetByIdAsync(variantGroupId);
                ConditionCheck.CheckCondition(group != null, Errors.Common.NotFound);

                _variantGroup.Remove(group);
                await _variantGroup.SaveChangesAsync();
            } catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> UpdateGroupSettingsAsync(Guid menuItemId, string groupName, UpdateGroupSettingRequest request)
        {
            try
            {
                var group = await _variantGroup.AsQueryable().FirstOrDefaultAsync(x => x.MenuItemId == menuItemId && x.name == groupName);
                ConditionCheck.CheckCondition(group != null, Errors.Common.NotFound);

                group!.IsRequired = request.IsRequired;
                group!.MaxSelect = request.MaxSelect;
                group!.MinSelect = request.MinSelect;

                _variantGroup.Update(group);
                await _variantGroup.SaveChangesAsync();
            } catch(Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
