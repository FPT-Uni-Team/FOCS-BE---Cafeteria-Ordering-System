using FOCS.Application.DTOs;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities;

namespace FOCS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : FocsController
    {
        private readonly IMenuItemManagementService _menuManagementService;

        public MenuItemController(IMenuItemManagementService menuService)
        {
            _menuManagementService = menuService;
        }

        [HttpPost]
        public async Task<bool> CreateNewMenuItem([FromBody] CreateMenuItemWithVariantRequest createMenuItemWithVariantRequest)
        {
            return await _menuManagementService.CreateNewMenuItemWithVariant(createMenuItemWithVariantRequest);
        }
        
        [HttpGet("product/images/{menuItemId}")]
        public async Task<List<UploadedImageResult>> GetImagesOfProduct(Guid menuItemId)
        {
            return await _menuManagementService.GetImagesOfProduct(menuItemId, StoreId);
        }

        [HttpPost("upload")]
        public async Task<bool> UploadImage(
                [FromForm] List<IFormFile> files,
                [FromForm] List<bool> isMain,
                [FromForm] string menuItemId,
                [FromHeader] string storeId)
        {
            return await _menuManagementService.UploadImagesAsync(files, isMain, menuItemId, storeId);
        }

        [HttpPut("images/update")]
        public async Task<bool> UpdateImages(
                [FromForm] List<string> urls,
                [FromForm] List<IFormFile> files,
                [FromForm] List<bool> isMain,
                [FromHeader] string storeId)
        {
            return await _menuManagementService.UpdateImagesAsync(urls, files, isMain, storeId);
        }

        [HttpDelete("images/delete")]
        public async Task<bool> RemoveImageAsync(List<string> urls)
        {
            return await _menuManagementService.RemoveImageAsync(urls);
        }
    }
}
