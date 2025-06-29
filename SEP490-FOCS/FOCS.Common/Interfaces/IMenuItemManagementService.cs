using FOCS.Common.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IMenuItemManagementService
    {
        Task<bool> CreateNewMenuItemWithVariant(CreateMenuItemWithVariantRequest request);
        Task<List<UploadedImageResult>> GetImagesOfProduct(Guid menuItemId, string storeId);
        Task<bool> UploadImagesAsync(List<IFormFile> files, List<bool> isMain, string menuItemId, string storeId);
    }
}
