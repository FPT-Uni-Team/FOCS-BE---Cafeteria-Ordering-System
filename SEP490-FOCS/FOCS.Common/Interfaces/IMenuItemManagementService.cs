using FOCS.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Common.Interfaces
{
    public interface IMenuItemManagementService
    {
        Task<bool> CreateNewMenuItemWithVariant(CreateMenuItemWithVariantRequest request);
        Task<List<UploadedImageResult>> GetImagesOfProduct(Guid menuItemId, string storeId);
        Task<bool> AddVariantGroupAndVariant(AddVariantGroupAndVariant request, Guid id, string storeId);
        Task<bool> RemoveVariantGroupAndVariantFromProduct(RemoveProductVariantFromProduct request, Guid menuItemId, string storeId);
        Task<bool> RemoveVariantGroupsFromProduct(RemoveVariantGroupFromProduct request, Guid menuItemId, string storeId);
        Task<bool> SyncMenuItemImages(List<IFormFile> files, string metadata, Guid menuItemId, string storeId);
        Task<bool> ActivateMenuItemAsync(Guid menuItemId, string userId);
        Task<bool> DeactivateMenuItemAsync(Guid menuItemId, string userId);
        Task<bool> EnableMenuItemAsync(Guid menuItemId, string userId);
        Task<bool> DisableMenuItemAsync(Guid menuItemId, string userId);
    }
}
