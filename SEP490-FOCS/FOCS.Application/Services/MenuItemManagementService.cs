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
