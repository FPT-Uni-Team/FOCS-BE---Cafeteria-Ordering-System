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
    public class AdminStoreController : FocsController
    {
        private readonly IAdminStoreService _adminStoreService;

        public AdminStoreController(IAdminStoreService storeService)
        {
            _adminStoreService = storeService;
        }

        [HttpPost("store")]
        public async Task<IActionResult> CreateStore([FromBody] StoreAdminDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _adminStoreService.CreateStoreAsync(dto, UserId);
            return Ok(created);
        }

        [HttpPost("stores")]
        public async Task<IActionResult> GetStores([FromBody] UrlQueryParameters query)
        {
            var result = await _adminStoreService.GetAllStoresAsync(query, UserId);
            return Ok(result);
        }

        [HttpPut("store/{id}")]
        public async Task<IActionResult> UpdateStore(Guid id, [FromBody] StoreAdminDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _adminStoreService.UpdateStoreAsync(id, dto, UserId);
            return success ? Ok() : NotFound();
        }

        [HttpDelete("store/{id}")]
        public async Task<IActionResult> DeleteStore(Guid id)
        {
            var success = await _adminStoreService.DeleteStoreAsync(id, UserId);
            return success ? Ok() : NotFound();
        }
    }
}
