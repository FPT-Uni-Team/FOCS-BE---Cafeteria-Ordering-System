using AutoMapper;
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

        private readonly IRepository<VariantGroup> _variantGroupRepo;
        private readonly IRepository<MenuItemVariantGroup> _menuItemVariantGroup;
        private readonly IRepository<MenuItemVariant> _menuItemVariantRepo;

        private readonly IRepository<MenuItemVariantGroupItem> _menuItemVariantGroupItemRepo;

        private readonly IMapper _mapper;

        public VariantGroupService(IMenuItemVariantService menuItemVariantService, IRepository<MenuItemVariantGroupItem> menuItemVariantGroupItemRepo, IRepository<MenuItemVariant> menuItemVariantRepo, IMapper mapper, IAdminMenuItemService menuItemService, IRepository<VariantGroup> variantGroup, IRepository<MenuItemVariantGroup> menuItemVariantGroup)
        {
            _menuItemService = menuItemService;
            _menuItemVariantService = menuItemVariantService;
            _variantGroupRepo = variantGroup;
            _mapper = mapper;
            _menuItemVariantGroup = menuItemVariantGroup;
            _menuItemVariantRepo = menuItemVariantRepo;
            _menuItemVariantGroupItemRepo = menuItemVariantGroupItemRepo;
        }

        public async Task<bool> AddMenuItemVariantToGroupAsync(AddVariantToGroupRequest request, Guid storeId)
        {
            try
            {
                var menuItem = await _menuItemService.GetMenuDetailAsync(request.MenuItemId);
                ConditionCheck.CheckCondition(menuItem != null, Errors.Common.NotFound);

                var variants = await _menuItemVariantService.ListVariantsWithIds(request.VariantIds, storeId);

                var groupNameExist = await _variantGroupRepo.AsQueryable().AnyAsync(x => x.Name == request.GroupName);
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

                await _variantGroupRepo.AddAsync(newGroupVariant);

                await _menuItemVariantService.AssignVariantGroupToVariants(request.VariantIds, newGroupVariant.Id);

                await _variantGroupRepo.SaveChangesAsync();
            } catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<VariantGroupDetailDTO> UpdateVariantGroupAsync(Guid variantGroupId, UpdateVariantGroupRequest updateVariantGroupRequest, string storeId)
        {
            var variantGroup = await _variantGroupRepo.AsQueryable().FirstOrDefaultAsync(x => x.Id == variantGroupId);
            ConditionCheck.CheckCondition(variantGroup != null, Errors.Common.NotFound);

            var isExist = await _variantGroupRepo.AsQueryable().AnyAsync(x => x.Name == updateVariantGroupRequest.Name);
            ConditionCheck.CheckCondition(!isExist, Errors.Common.IsExist);

            variantGroup.Name = updateVariantGroupRequest.Name;
            variantGroup.UpdatedAt = DateTime.UtcNow;
            variantGroup.UpdatedBy = storeId;

            _variantGroupRepo.Update(variantGroup);
            await _variantGroupRepo.SaveChangesAsync();


            return new VariantGroupDetailDTO
            {
                Id = variantGroup.Id,
                GroupName = variantGroup.Name
            };
        }

        public async Task<VariantGroupDetailDTO> GetVariantGroupDetailAsync(Guid variantGroupId, string storeId)
        {
            var variantGroupItem = _variantGroupRepo
                .AsQueryable()
                .Include(x => x.Variants)
                .ThenInclude(x => x.VariantGroup)
                .Where(x => x.CreatedBy == storeId && x.Id == variantGroupId);

            ConditionCheck.CheckCondition(variantGroupItem != null, Errors.Common.NotFound);

            var result = variantGroupItem.Select(x => new VariantGroupDetailDTO
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
            }).First();

            return result;
        }

        public async Task<PagedResult<VariantGroupDetailDTO>> GetVariantGroupsByStore(UrlQueryParameters urlQueryParameters, string storeId)
        {
            var variantsGroup = _variantGroupRepo
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
                var isExist = await _variantGroupRepo.AsQueryable().AnyAsync(x => x.Name == request.Name && x.CreatedBy == storeId);
                ConditionCheck.CheckCondition(!isExist, Errors.Common.IsExist, "name");

                var newVariantGroup = _mapper.Map<VariantGroup>(request);
                newVariantGroup.Id = Guid.NewGuid();
                newVariantGroup.CreatedBy = storeId;

                await _variantGroupRepo.AddAsync(newVariantGroup);
                await _variantGroupRepo.SaveChangesAsync();

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

        public async Task<bool> RemoveVariantGroupAsync(Guid variantGroupId)
        {
            try
            {
                var group = await _variantGroupRepo.AsQueryable()
                    .Include(x => x.Variants)
                    .FirstOrDefaultAsync(x => x.Id == variantGroupId);

                ConditionCheck.CheckCondition(group != null, Errors.Common.NotFound);
                
                if (group.Variants?.Any() == true)
                {
                    var variantGroupLinks = await _menuItemVariantGroup.AsQueryable()
                        .Where(x => x.VariantGroupId == variantGroupId)
                        .ToListAsync();

                    if (variantGroupLinks.Any())
                    {
                        var variantGroupLinkIds = variantGroupLinks.Select(x => x.Id).ToList();

                        var groupItems = await _menuItemVariantGroupItemRepo.AsQueryable()
                            .Where(x => variantGroupLinkIds.Contains(x.MenuItemVariantGroupId))
                            .ToListAsync();

                        _menuItemVariantGroupItemRepo.RemoveRange(groupItems);
                        _menuItemVariantGroup.RemoveRange(variantGroupLinks);
                    }

                    var variantsToUpdate = await _menuItemVariantRepo.AsQueryable()
                        .Where(x => x.VariantGroupId == variantGroupId)
                        .ToListAsync();

                    foreach (var variant in variantsToUpdate)
                    {
                        variant.VariantGroupId = null;
                    }

                    _menuItemVariantRepo.UpdateRange(variantsToUpdate);

                    await _menuItemVariantGroupItemRepo.SaveChangesAsync();
                }

                _variantGroupRepo.Remove(group);
                await _variantGroupRepo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
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
