using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
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
            return await _storeSettingService.GetStoreSettingAsync(storeId, UserId);
        }

        [HttpPut("update/{storeId}")]
        public async Task<IActionResult> UpdateStoreSettingAsync(Guid storeId, StoreSettingDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _storeSettingService.UpdateStoreSettingAsync(storeId, dto, UserId);
            return success ? Ok() : NotFound();
        }

        [HttpPut("{storeId}")]
        public async Task<StoreSettingDTO> CreateStoreSettingAsync(Guid storeId, StoreSettingDTO dto)
        {
            var created = await _storeSettingService.CreateStoreSettingAsync(storeId, dto, UserId);
            return created;
        }
    }
}
