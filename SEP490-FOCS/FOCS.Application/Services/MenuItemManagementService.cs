using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FOCS.Application.DTOs;
using FOCS.Application.Services.Interface;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public MenuItemManagementService(IMenuItemsVariantGroupService menuVariantGroupService, IMenuItemCategoryService menuItemCategoryService, ICloudinaryService cloudinaryService, IRepository<MenuItemImage> menuItemImageRepository, IMenuItemsVariantGroupItemService menuItemsVariantGroupItemService, IAdminMenuItemService adminMenuItemService)
        {
            _menuVariantGroupService = menuVariantGroupService;
            _menuItemsVariantGroupItemService = menuItemsVariantGroupItemService;
            _adminMenuItemService = adminMenuItemService;
            _menuItemImageRepository = menuItemImageRepository;
            _cloudinaryService = cloudinaryService;
            _menuItemCategoryService = menuItemCategoryService;
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

        public async Task<List<UploadedImageResult>> GetImagesOfProduct(Guid menuItemId, string storeId)
        {
            var images = await _menuItemImageRepository.AsQueryable().Where(x => x.MenuItemId == menuItemId).ToListAsync();

            return images.Select(x => new UploadedImageResult
            {
                IsMain = x.IsMain,
                Url = x.Url,
            }).ToList();
        }

        public async Task<bool> UploadImagesAsync(List<IFormFile> files, List<bool> isMain, string menuItemId, string storeId)
        {
            try
            {
                var uploads = await _cloudinaryService.UploadImageAsync(files, isMain, storeId, menuItemId);

                var images = uploads.Select(x => new MenuItemImage
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = storeId,
                    IsMain = x.IsMain,
                    Url = x.Url,
                    MenuItemId = Guid.Parse(menuItemId)
                });

                await _menuItemImageRepository.AddRangeAsync(images);
                await _menuItemImageRepository.SaveChangesAsync();

                return true;
            } catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateImagesAsync(List<string> urls, List<IFormFile> files, List<bool> isMainList, string storeId)
        {
            try
            {
                if (urls.Count != files.Count || urls.Count != isMainList.Count)
                    return false;

                var existingImages = await _menuItemImageRepository.FindAsync(i => urls.Contains(i.Url));
                var imageDict = existingImages.ToDictionary(i => i.Url);

                var uploadFiles = new List<IFormFile>();
                var isMainFlags = new List<bool>();
                var updateTargets = new List<(MenuItemImage image, int index)>();

                for (int i = 0; i < urls.Count; i++)
                {
                    var url = urls[i];
                    var file = files[i];
                    var isMain = isMainList[i];

                    if (!imageDict.TryGetValue(url, out var image))
                        continue;

                    uploadFiles.Add(file);
                    isMainFlags.Add(isMain);
                    updateTargets.Add((image, uploadFiles.Count - 1));
                }

                if (uploadFiles.Count == 0)
                    return true;

                var menuItemId = existingImages.First().MenuItemId.ToString();
                var uploaded = await _cloudinaryService.UploadImageAsync(uploadFiles, isMainFlags, storeId, menuItemId);

                for (int i = 0; i < updateTargets.Count; i++)
                {
                    var (image, index) = updateTargets[i];
                    var result = uploaded[index];

                    image.Url = result.Url;
                    image.IsMain = result.IsMain;
                    image.UpdatedAt = DateTime.UtcNow;
                    image.UpdatedBy = storeId;
                }

                await _menuItemImageRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveImageAsync(string url)
        {
            try
            {
                var image = await _menuItemImageRepository.AsQueryable().FirstOrDefaultAsync(x => x.Url == url);

                ConditionCheck.CheckCondition(image != null, Errors.Common.NotFound);

                _menuItemImageRepository.Remove(image!);
                await _menuItemImageRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
