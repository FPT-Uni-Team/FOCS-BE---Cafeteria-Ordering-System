using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FOCS.Application.DTOs;
using FOCS.Application.Services.Interface;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FOCS.Application.Services
{
    public class MenuItemManagementService : IMenuItemManagementService
    {
        private readonly IAdminMenuItemService _adminMenuItemService;
        private readonly IMenuItemsVariantGroupService _menuVariantGroupService;
        private readonly IMenuItemsVariantGroupItemService _menuItemsVariantGroupItemService;

        private readonly IRepository<MenuItemImage> _menuItemImageRepository;
        private readonly ICloudinaryService _cloudinaryService;

        private readonly IMenuItemCategoryService _menuItemCategoryService;
        private readonly IRepository<MenuItem> _menuItemRepository;
        private readonly UserManager<User> _userManager;
        private readonly IRepository<UserStore> _userStoreRepository;

        public MenuItemManagementService(
            IMenuItemsVariantGroupService menuVariantGroupService,
            IMenuItemCategoryService menuItemCategoryService,
            ICloudinaryService cloudinaryService,
            IRepository<MenuItemImage> menuItemImageRepository,
            IMenuItemsVariantGroupItemService menuItemsVariantGroupItemService,
            IAdminMenuItemService adminMenuItemService,
            IRepository<MenuItem> menuItemRepository,
            UserManager<User> userManager,
            IRepository<UserStore> userStoreRepository)
        {
            _menuVariantGroupService = menuVariantGroupService;
            _menuItemsVariantGroupItemService = menuItemsVariantGroupItemService;
            _adminMenuItemService = adminMenuItemService;
            _menuItemImageRepository = menuItemImageRepository;
            _cloudinaryService = cloudinaryService;
            _menuItemCategoryService = menuItemCategoryService;
            _menuItemRepository = menuItemRepository;
            _userManager = userManager;
            _userStoreRepository = userStoreRepository;
        }

        public async Task<bool> CreateNewMenuItemWithVariant(CreateMenuItemWithVariantRequest request)
        {
            // Step 1: Create new menu item
            var newMenuItem = await _adminMenuItemService.CreateMenuAsync(new DTOs.AdminServiceDTO.MenuItemAdminDTO
            {
                Id = request.Id ?? Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                BasePrice = request.BasePrice,
                IsAvailable = request.IsAvailable,
                StoreId = request.StoreId
            }, request.StoreId.ToString());

            // Step 2: Link categories to menu item
            if (request.CategoryIds!= null && request.CategoryIds.Any())
            {
                await _menuItemCategoryService.AssignCategoriesToMenuItem(request.CategoryIds, (Guid)newMenuItem.Id, request.StoreId.ToString());
            }

            // Step 3: Link each VariantGroup for MenuItem
            var createdVariantGroups = new List<MenuItemVariantGroup>();

            foreach (var variantGroup in request.VariantGroupRequests)
            {
                var assignResult = await _menuVariantGroupService.AssignMenuItemToVariantGroup(new CreateMenuItemVariantGroupRequest
                {
                    MenuItemId = newMenuItem.Id,
                    VariantGroupIds = new List<Guid> { variantGroup.Id },
                    IsRequired = variantGroup.IsRequired,
                    MinSelect = variantGroup.MinSelect,
                    MaxSelect = variantGroup.MaxSelect
                });

                createdVariantGroups.AddRange(assignResult);
            }

            ConditionCheck.CheckCondition(createdVariantGroups.Any(), Errors.Variant.FailWhenAssign);

            // Step 4: link MenuItemVariantGroupItem (Link with spec variant)
            var groupItemRequests = createdVariantGroups.Select(group =>
            {
                var matchedGroup = request.VariantGroupRequests.FirstOrDefault(x => x.Id == group.VariantGroupId);

                return new MenuItemVariantGroupItemRequest
                {
                    MenuItemVariantGroupId = group.Id,
                    Variants = matchedGroup?.Variants ?? new List<VariantRequest>()
                };
            }).ToList();

            var assignVariantsResult = await _menuItemsVariantGroupItemService.AssignMenuItemVariantGroupToMenuItemVariantItemGroup(new CreateMenuItemVariantGroupItemRequest
            {
                MenuItemVariantGroupItemRequests = groupItemRequests
            });

            ConditionCheck.CheckCondition(assignVariantsResult, Errors.Variant.FailWhenAssign);

            return true;
        }

        public async Task<bool> AddVariantGroupAndVariant(AddVariantGroupAndVariant request, Guid menuItemId, string storeId)
        {
            try
            {
                // Step 1: Link each VariantGroup for MenuItem
                var createdVariantGroups = new List<MenuItemVariantGroup>();

                var assignResult = await _menuVariantGroupService.AssignMenuItemToVariantGroup(new CreateMenuItemVariantGroupRequest
                {
                    MenuItemId = menuItemId,
                    VariantGroupIds = new List<Guid> { request.VariantGroupId },
                    IsRequired = request.IsRequired,
                    MinSelect = request.MinSelect,
                    MaxSelect = request.MaxSelect
                });

                createdVariantGroups.AddRange(assignResult);

                ConditionCheck.CheckCondition(createdVariantGroups.Any(), Errors.Variant.FailWhenAssign);

                // Step 2: link MenuItemVariantGroupItem (Link with spec variant)
                var groupItemRequests = createdVariantGroups.Select(group =>
                {
                    return new MenuItemVariantGroupItemRequest
                    {
                        MenuItemVariantGroupId = group.Id,
                        Variants = request?.Variants ?? new List<VariantRequest>()
                    };
                }).ToList();

                var assignVariantsResult = await _menuItemsVariantGroupItemService.AssignMenuItemVariantGroupToMenuItemVariantItemGroup(new CreateMenuItemVariantGroupItemRequest
                {
                    MenuItemVariantGroupItemRequests = groupItemRequests
                });

                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SyncMenuItemImages(List<IFormFile> files, string metadata, Guid menuItemId, string storeId)
        {
            try
            {
                var imageMetaList = JsonSerializer.Deserialize<List<ImageSycnMetaData>>(metadata, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                ConditionCheck.CheckCondition(imageMetaList != null || imageMetaList.Any(), Errors.Common.NotFound);

                int fileIndex = 0;
                foreach (var imageMeta in imageMetaList)
                {
                    if (imageMeta.IsDeleted)
                    {
                        if (imageMeta.Id.HasValue)
                        {
                            var entity = await _menuItemImageRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == imageMeta.Id && x.CreatedBy == storeId);
                            if (entity == null) continue;

                            _menuItemImageRepository.Remove(entity);
                            //await _cloudinaryService.RemoveImageFromCloud();
                        }
                    }
                    else if (imageMeta.Id.HasValue)
                    {
                        var existing = await _menuItemImageRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == imageMeta.Id && x.CreatedBy == storeId);

                        if (existing == null) continue;

                        existing.IsMain = imageMeta.IsMain;
                        _menuItemImageRepository.Update(existing);
                    }
                    else
                    {
                        //create
                        var file = files[fileIndex++];
                        var url = await _cloudinaryService.UploadImageAsync(new List<IFormFile> { file }, new List<bool> { imageMeta.IsMain }, storeId, menuItemId.ToString());

                        ConditionCheck.CheckCondition(url != null, Errors.Common.NotFound);

                        var newImage = new MenuItemImage
                        {
                            Id = Guid.NewGuid(),
                            CreatedAt = DateTime.Now,
                            CreatedBy = storeId,
                            IsMain = imageMeta.IsMain,
                            MenuItemId = menuItemId,
                            Url = url!.FirstOrDefault()!.Url,
                        };

                        await _menuItemImageRepository.AddAsync(newImage);
                    }
                }

                await _menuItemImageRepository.SaveChangesAsync();

                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> RemoveVariantGroupAndVariantFromProduct(RemoveProductVariantFromProduct request, Guid menuItemId, string storeId)
        {
            return await _menuItemsVariantGroupItemService.RemoveVariantsFromMenuItemVariantGroup(request, menuItemId, storeId);
        }

        public async Task<bool> RemoveVariantGroupsFromProduct(RemoveVariantGroupFromProduct request, Guid menuItemId, string storeId)
        {
            return await _menuVariantGroupService.RemoveVariantGroupsFromProduct(request, menuItemId, storeId);
        }

        public async Task<List<UploadedImageResult>> GetImagesOfProduct(Guid menuItemId, string storeId)
        {
            var images = await _menuItemImageRepository.AsQueryable().Where(x => x.MenuItemId == menuItemId).ToListAsync();

            return images.Select(x => new UploadedImageResult
            {
                IsMain = x.IsMain,
                Url = x.Url,
            }).ToList();
        }
        public async Task<bool> ActivateMenuItemAsync(Guid menuItemId, string userId)
        {
            return await UpdateMenuItemStatusAsync(menuItemId, userId, isActive: true,
                condition: item => !item.IsActive,
                error: Errors.MenuItemError.MenuItemActive,
                fieldName: Errors.FieldName.IsActive);
        }

        public async Task<bool> DeactivateMenuItemAsync(Guid menuItemId, string userId)
        {
            return await UpdateMenuItemStatusAsync(menuItemId, userId, isActive: false,
                condition: item => item.IsActive,
                error: Errors.MenuItemError.MenuItemInactive,
                fieldName: Errors.FieldName.IsActive);
        }

        public async Task<bool> EnableMenuItemAsync(Guid menuItemId, string userId)
        {
            return await UpdateMenuItemStatusAsync(menuItemId, userId, isAvailable: true,
                condition: item => !item.IsAvailable,
                error: Errors.MenuItemError.MenuItemAvailable,
                fieldName: Errors.FieldName.IsAvailable);
        }

        public async Task<bool> DisableMenuItemAsync(Guid menuItemId, string userId)
        {
            return await UpdateMenuItemStatusAsync(menuItemId, userId, isAvailable: false,
                condition: item => item.IsAvailable,
                error: Errors.MenuItemError.MenuItemUnavailable,
                fieldName: Errors.FieldName.IsAvailable);
        }

        private async Task<bool> UpdateMenuItemStatusAsync(Guid menuItemId, string userId,
            bool? isActive = null, bool? isAvailable = null,
            Func<MenuItem, bool> condition = null, string error = null, string fieldName = null)
        {
            var menuItem = await GetMenuItemAsync(menuItemId, userId);
            if (menuItem == null) return false;

            ConditionCheck.CheckCondition(condition(menuItem), error, fieldName);

            if (isActive.HasValue)
                menuItem.IsActive = isActive.Value;
            if (isAvailable.HasValue)
                menuItem.IsAvailable = isAvailable.Value;

            menuItem.UpdatedAt = DateTime.UtcNow;
            menuItem.UpdatedBy = userId;

            await _menuItemRepository.SaveChangesAsync();
            return true;
        }

        private async Task<MenuItem?> GetMenuItemAsync(Guid menuItemId, string userId)
        {
            var menuItem = await _menuItemRepository.AsQueryable()
                                            .Where(x => x.Id == menuItemId && !x.IsDeleted).FirstOrDefaultAsync();
            if (menuItem == null) return null;

            var user = await _userManager.FindByIdAsync(userId);

            var storesOfUser = (await _userStoreRepository.FindAsync(x => x.UserId == Guid.Parse(userId))).Distinct().ToList();

            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound, Errors.FieldName.UserId);
            ConditionCheck.CheckCondition(storesOfUser.Select(x => x.StoreId).Contains(menuItem.StoreId), Errors.AuthError.UserUnauthor, Errors.FieldName.UserId);

            return menuItem;
        }
    }
}
