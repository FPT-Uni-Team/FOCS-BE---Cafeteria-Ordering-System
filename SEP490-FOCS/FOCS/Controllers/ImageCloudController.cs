using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/image-cloud")]
    [ApiController]
    public class ImageCloudController : FocsController
    {
        private readonly ICloudinaryService _cloudinaryService;

        public ImageCloudController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImages(
                [FromForm] List<IFormFile> files,
                [FromForm] List<bool> isMain,
                [FromForm] string menuItemId,
                [FromHeader] string storeId)
        {
            var imageUrls = await _cloudinaryService.UploadImageAsync(files, isMain, storeId, menuItemId);
            return Ok(imageUrls);
        }

        [HttpPost("remove-object")]
        public async Task<object> Remove([FromBody] List<string> urls, [FromHeader] string objectId)
        {
            return await _cloudinaryService.RemoveImageFromCloud(urls, objectId, StoreId);
        }
    }
}
