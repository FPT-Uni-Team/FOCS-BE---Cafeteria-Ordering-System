using FOCS.Common.Models;
using Microsoft.AspNetCore.Http;

namespace FOCS.Common.Interfaces
{
    public interface IMenuItemManagementService
    {
        Task<bool> CreateNewMenuItemWithVariant(CreateMenuItemWithVariantRequest request);
        Task<List<UploadedImageResult>> GetImagesOfProduct(Guid menuItemId, string storeId);
        Task<bool> RemoveImageAsync(List<string> urls);
        Task<bool> UploadImagesAsync(List<IFormFile> files, List<bool> isMain, string menuItemId, string storeId);
        Task<bool> UpdateImagesAsync(List<string> urls, List<IFormFile> files, List<bool> isMainList, string storeId);
        Task<bool> AddVariantGroupAndVariant(AddVariantGroupAndVariant request, Guid id, string storeId);
        Task<bool> RemoveVariantGroupAndVariantFromProduct(RemoveProductVariantFromProduct request, Guid menuItemId, string storeId);
        Task<bool> RemoveVariantGroupsFromProduct(RemoveVariantGroupFromProduct request, Guid menuItemId, string storeId);

    }
}
