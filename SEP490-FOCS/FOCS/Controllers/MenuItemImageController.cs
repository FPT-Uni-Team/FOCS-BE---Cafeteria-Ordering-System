using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("image")]
    [ApiController]
    public class MenuItemImageController : FocsController
    {
        private readonly ICloudinaryService _cloudinaryService;

        public MenuItemImageController(ICloudinaryService cloudinaryService)
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
    }
}
