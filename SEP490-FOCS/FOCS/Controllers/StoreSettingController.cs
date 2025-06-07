using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Authorize(Roles = Roles.Manager)]
    [Authorize(Roles = Roles.Admin)]
    [Route("api/admin/store-setting")]
    [ApiController]
    public class StoreSettingController : FocsController
    {
        private readonly IStoreSettingService _storeSettingService;

        public StoreSettingController(IStoreSettingService storeService)
        {
            _storeSettingService = storeService;
        }

        [HttpGet("{storeId}")]
        public async Task<StoreSettingDTO> GetStoreSettingAsync(Guid storeId)
        {
            return await _storeSettingService.GetStoreSettingAsync(storeId);
        }

        [HttpPut("{storeId}")]
        public async Task<IActionResult> UpdateStoreSettingAsync(Guid storeId, StoreSettingDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _storeSettingService.UpdateStoreSettingAsync(storeId, dto, UserId);
            return success ? Ok() : NotFound();
        }

        [HttpPut("reset-setting/{storeId}")]
        public async Task<IActionResult> ResetStoreSettingAsync(Guid storeId)
        {
            var success = await _storeSettingService.ResetStoreSettingAsync(storeId, UserId);
            return success ? Ok() : NotFound();
        }
    }
}
