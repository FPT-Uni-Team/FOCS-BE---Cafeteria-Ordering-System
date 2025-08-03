using FOCS.Common.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FOCS.Common.Interfaces
{
    public interface ICloudinaryService
    {
        Task<List<UploadedImageResult>> UploadImageAsync(List<IFormFile> files, List<bool> isMain, string storeId, string menuItemId);

        Task<UploadedImageResult> UploadQrCodeForTable(IFormFile file, string storeId, string tableId);

        Task<List<UploadedImageResult>> UploadImageFeedbackAsync(List<IFormFile> files, string storeId, string orderId);

        Task<object> RemoveImageFromCloud(List<string> ids, string objectId, string storeId);
    }
}
