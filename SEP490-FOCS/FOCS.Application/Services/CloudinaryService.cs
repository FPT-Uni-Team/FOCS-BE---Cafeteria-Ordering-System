using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FOCS.Application.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
            _cloudinary.Api.Secure = true;
        }

        public async Task<List<UploadedImageResult>> UploadImageAsync(List<IFormFile> files, List<bool> isMain, string storeId, string menuItemId)
        {
            var images = Enumerable.Range(0, Math.Min(files.Count, isMain.Count))
                        .Select(i => new CreateImageRequest
                        {
                            ImageFile = files[i],
                            IsMain = isMain[i]
                        })
                        .ToList();

            var results = new List<UploadedImageResult>();

            foreach (var image in images)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.ImageFile.FileName, image.ImageFile.OpenReadStream()),
                    Folder = $"stores/{storeId}/{menuItemId}",
                    PublicId = Guid.NewGuid().ToString(),
                    Transformation = new Transformation().Crop("limit").Width(800).Height(800)
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    results.Add(new UploadedImageResult
                    {
                        Url = result.SecureUrl.ToString(),
                        IsMain = image.IsMain
                    });
                }
                else
                {
                    throw new Exception("Upload failed");
                }
            }

            return results;
        }

        public async Task<UploadedImageResult> UploadQrCodeForTable(IFormFile file, string storeId, string tableId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("QR file is empty", nameof(file));

            var folderPath = $"stores/{storeId}/tables";
            var publicId = $"table_{tableId}_{Guid.NewGuid()}";

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folderPath,
                PublicId = publicId,
                Transformation = new Transformation().Crop("limit").Width(800).Height(800)
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
                throw new Exception("QR code upload failed to Cloudinary");

            return new UploadedImageResult
            {
                Url = uploadResult.SecureUrl.ToString(),
                IsMain = true
            };
        }

    }
}
