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

        public MenuItemManagementService(IMenuItemsVariantGroupService menuVariantGroupService, ICloudinaryService cloudinaryService, IRepository<MenuItemImage> menuItemImageRepository, IMenuItemsVariantGroupItemService menuItemsVariantGroupItemService, IAdminMenuItemService adminMenuItemService)
        {
            _menuVariantGroupService = menuVariantGroupService;
            _menuItemsVariantGroupItemService = menuItemsVariantGroupItemService;
            _adminMenuItemService = adminMenuItemService;
            _menuItemImageRepository = menuItemImageRepository;
            _cloudinaryService = cloudinaryService;
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

            // Step 2: Link each VariantGroup for MenuItem
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

            // Step 3: link MenuItemVariantGroupItem (Link with spec variant)
            var groupItemRequests = createdVariantGroups.Select(group =>
            {
                var matchedGroup = request.VariantGroupRequests.FirstOrDefault(x => x.Id == group.VariantGroupId);

                return new MenuItemVariantGroupItemRequest
                {
                    MenuItemVariantGroupId = group.Id,
                    VariantIds = matchedGroup?.VariantIds ?? new List<Guid>()
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
                var uploads = await _cloudinaryService.UploadImageAsync(files, isMain, menuItemId, storeId);

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
    }
}
