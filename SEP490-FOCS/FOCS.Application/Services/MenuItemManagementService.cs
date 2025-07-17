using FOCS.Application.Services.Interface;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

        public MenuItemManagementService(
            IMenuItemsVariantGroupService menuVariantGroupService,
            IMenuItemCategoryService menuItemCategoryService,
            ICloudinaryService cloudinaryService,
            IRepository<MenuItemImage> menuItemImageRepository,
            IMenuItemsVariantGroupItemService menuItemsVariantGroupItemService,
            IAdminMenuItemService adminMenuItemService)
        {
            _menuVariantGroupService = menuVariantGroupService;
            _menuItemsVariantGroupItemService = menuItemsVariantGroupItemService;
            _adminMenuItemService = adminMenuItemService;
            _menuItemImageRepository = menuItemImageRepository;
            _cloudinaryService = cloudinaryService;
            _menuItemCategoryService = menuItemCategoryService;
        }

        public async Task<Guid> CreateNewMenuItemWithVariant(CreateMenuItemWithVariantRequest request)
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

            return newMenuItem.Id.Value;
        }

        public async Task<bool> AddVariantGroupAndVariant(AddVariantGroupsAndVariants variants, Guid menuItemId, string storeId)
        {
            foreach(var request in variants.VariantGroupsAndVariants)
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
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
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
    }
}
