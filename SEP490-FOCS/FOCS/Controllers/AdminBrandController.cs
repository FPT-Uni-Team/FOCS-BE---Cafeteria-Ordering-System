using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Route("api/admin")]
    [ApiController]
    public class AdminBrandController : FocsController
    {
        private readonly IAdminBrandService _adminBrandService;

        public AdminBrandController(IAdminBrandService adminBrand)
        {
            _adminBrandService = adminBrand;
        }

        [HttpPost("brand")]
        public async Task<IActionResult> CreateBrand([FromBody] CreateAdminBrandRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _adminBrandService.CreateBrandAsync(dto, UserId);
            return Ok(created);
        }

        [HttpPost("brands")]
        public async Task<IActionResult> GetAllBrands([FromBody] UrlQueryParameters query)
        {
            var pagedResult = await _adminBrandService.GetAllBrandsAsync(query, UserId);
            return Ok(pagedResult);
        }

        [HttpPut("brand/{id}")]
        public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] BrandAdminDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _adminBrandService.UpdateBrandAsync(id, dto, UserId);
            if (!updated)
                return NotFound();

            return Ok();
        }

        [HttpDelete("brand/{id}")]
        public async Task<IActionResult> DeleteBrand(Guid id)
        {
            var deleted = await _adminBrandService.DeleteBrandAsync(id, UserId);
            if (!deleted)
                return NotFound();

            return Ok();
        }

    }
}
