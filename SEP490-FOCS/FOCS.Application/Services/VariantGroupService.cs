﻿using AutoMapper;
using FOCS.Application.Services.Interface;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;

namespace FOCS.Application.Services
{
    public class VariantGroupService : IVariantGroupService
    {
        private readonly IMenuItemVariantService _menuItemVariantService;
        private readonly IAdminMenuItemService _menuItemService;

        private readonly IRepository<VariantGroup> _variantGroup;
        private readonly IRepository<MenuItemVariantGroup> _menuItemVariantGroup;
        private readonly IMapper _mapper;

        public VariantGroupService(IMenuItemVariantService menuItemVariantService, IMapper mapper, IAdminMenuItemService menuItemService, IRepository<VariantGroup> variantGroup, IRepository<MenuItemVariantGroup> menuItemVariantGroup)
        {
            _menuItemService = menuItemService;
            _menuItemVariantService = menuItemVariantService;
            _variantGroup = variantGroup;
            _mapper = mapper;
            _menuItemVariantGroup = menuItemVariantGroup;
        }

        public async Task<bool> AddMenuItemVariantToGroupAsync(AddVariantToGroupRequest request, Guid storeId)
        {
            try
            {
                var menuItem = await _menuItemService.GetMenuDetailAsync(request.MenuItemId);
                ConditionCheck.CheckCondition(menuItem != null, Errors.Common.NotFound);

                var variants = await _menuItemVariantService.ListVariantsWithIds(request.VariantIds, storeId);

                var groupNameExist = await _variantGroup.AsQueryable().AnyAsync(x => x.Name == request.GroupName);
                ConditionCheck.CheckCondition(!groupNameExist, Errors.Common.NotFound);

                var newGroupVariant = new VariantGroup
                {
                    Id = Guid.NewGuid(),
                    Name = request.GroupName,
                    //IsRequired = request.IsRequired,
                    //MaxSelect = request.MaxSelect,
                    //MinSelect = request.MinSelect,
                    //MenuItemId = request.MenuItemId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = storeId.ToString()
                };

                await _variantGroup.AddAsync(newGroupVariant);

                await _menuItemVariantService.AssignVariantGroupToVariants(request.VariantIds, newGroupVariant.Id);

                await _variantGroup.SaveChangesAsync();
            } catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<PagedResult<VariantGroupDetailDTO>> GetVariantGroupsByStore(UrlQueryParameters urlQueryParameters, string storeId)
        {
            var variantsGroup = _variantGroup
                .AsQueryable()
                .Include(x => x.Variants)
                .ThenInclude(x => x.VariantGroup)
                .Where(x => x.CreatedBy == storeId);

            // Search
            if (!string.IsNullOrWhiteSpace(urlQueryParameters.SearchBy) && !string.IsNullOrWhiteSpace(urlQueryParameters.SearchValue))
            {
                variantsGroup = urlQueryParameters.SearchBy.ToLower() switch
                {
                    "name" => variantsGroup.Where(x => x.Name.Contains(urlQueryParameters.SearchValue)),
                    _ => variantsGroup
                };
            }

            // Sort
            if (!string.IsNullOrEmpty(urlQueryParameters.SortBy))
            {
                var sortDirection = urlQueryParameters.SortOrder?.ToLower() ?? "asc";

                variantsGroup = (urlQueryParameters.SortBy.ToLower(), sortDirection) switch
                {
                    ("name", "asc") => variantsGroup.OrderBy(x => x.Name),
                    ("name", "desc") => variantsGroup.OrderByDescending(x => x.Name),
                    _ => variantsGroup
                };
            }

            int page = urlQueryParameters.Page > 0 ? urlQueryParameters.Page : 1;
            int pageSize = urlQueryParameters.PageSize > 0 ? urlQueryParameters.PageSize : 10;

            variantsGroup = variantsGroup.Skip((page - 1) * pageSize).Take(pageSize);

            var result = await variantsGroup.Select(x => new VariantGroupDetailDTO
            {
                Id = x.Id,
                GroupName = x.Name,
                Variants = x.Variants.Select(v => new VariantOptionDTO
                {
                    Id = v.Id,
                    Name = v.Name,
                    IsAvailable = v.IsAvailable,
                    Price = v.Price,
                }).ToList()
            }).ToListAsync();

            var total = await variantsGroup.CountAsync();

            return new PagedResult<VariantGroupDetailDTO>(result, total, page, pageSize);
        }

        public async Task<bool> CreateVariantGroup(CreateVariantGroupRequest request, string storeId)
        {
            try
            {
                var isExist = await _variantGroup.AsQueryable().AnyAsync(x => x.Name == request.Name && x.CreatedBy == storeId);
                ConditionCheck.CheckCondition(!isExist, Errors.Common.IsExist, "name");

                var newVariantGroup = _mapper.Map<VariantGroup>(request);
                newVariantGroup.Id = Guid.NewGuid();
                newVariantGroup.CreatedBy = storeId;

                await _variantGroup.AddAsync(newVariantGroup);
                await _variantGroup.SaveChangesAsync();

                return true;
            } catch(Exception ex)
            {
                //loger
                return false;
            }
        }

        public async Task<List<string>> GetGroupNamesByMenuItemAsync(Guid menuItemId)
        {
            var menuItem = await _menuItemService.GetMenuDetailAsync(menuItemId);
            ConditionCheck.CheckCondition(menuItem != null, Errors.Common.NotFound);

            return await _menuItemVariantGroup.AsQueryable().Include(x => x.VariantGroup).Where(x => x.MenuItemId == menuItemId).Select(x => x.VariantGroup.Name).ToListAsync();
        }

        public async Task<List<VariantGroupDetailDTO>> GetVariantGroupsByMenuItemAsync(Guid menuItemId)
        {
            //var menuItem = await _menuItemService.GetMenuDetailAsync(menuItemId);
            //ConditionCheck.CheckCondition(menuItem != null, Errors.Common.NotFound);

            //var groups = await _menuItemVariantGroup.AsQueryable().Include(x => x.).Where(x => x.MenuItemId == menuItemId).ToListAsync();

            //return groups.Select(x => new VariantGroupDetailDTO
            //{
            //    GroupName = x.Name,
            //    IsRequired = x.IsRequired,
            //    MaxSelect = x.MaxSelect,
            //    MinSelect = x.MinSelect,
            //    Variants = _mapper.Map<List<VariantOptionDTO>>(x.Variants)
            //}).ToList();

            return new List<VariantGroupDetailDTO>();
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
            //try
            //{
            //    var group = await _variantGroup.AsQueryable().FirstOrDefaultAsync(x => x.MenuItemId == menuItemId && x.Name == groupName);
            //    ConditionCheck.CheckCondition(group != null, Errors.Common.NotFound);

            //    group!.IsRequired = request.IsRequired;
            //    group!.MaxSelect = request.MaxSelect;
            //    group!.MinSelect = request.MinSelect;

            //    _variantGroup.Update(group);
            //    await _variantGroup.SaveChangesAsync();
            //} catch(Exception ex)
            //{
            //    return false;
            //}
            //return true;
            return false;
        }
    }
}
