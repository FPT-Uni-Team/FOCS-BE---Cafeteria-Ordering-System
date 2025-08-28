using FOCS.Application.DTOs;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/manager")]
    [ApiController]
    public class TableController : FocsController
    {
        private readonly ITableService _tableService;

        public TableController(ITableService tableService)
        {
            _tableService = tableService;
        }

        [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
        [HttpPost("table")]
        public async Task<IActionResult> CreateTable([FromBody] TableDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _tableService.CreateTableAsync(dto, StoreId, UserId);
            return Ok(created);
        }

        [HttpPost("tables")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager + "," + Roles.Staff + "," + Roles.KitchenStaff)]
        public async Task<IActionResult> GetAllTables([FromBody] UrlQueryParameters query, Guid storeId)
        {
            var result = await _tableService.GetAllTablesAsync(query, UserId, storeId);
            return Ok(result);
        }

        [HttpGet("table/{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager + "," + Roles.Staff + "," + Roles.KitchenStaff)]
        public async Task<IActionResult> GetTable(Guid id)
        {
            var table = await _tableService.GetTableByIdAsync(id, StoreId);

            if (table == null)
                return NotFound();

            return Ok(table);
        }

        [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
        [HttpPut("table/{id}")]
        public async Task<IActionResult> UpdateTable(Guid id, [FromBody] TableDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.StoreId = Guid.Parse(StoreId);
            var updated = await _tableService.UpdateTableAsync(id, dto, UserId);
            if (!updated)
                return NotFound();

            return Ok();
        }

        [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
        [HttpDelete("table/{id}")]
        public async Task<IActionResult> DeleteTable(Guid id)
        {
            var deleted = await _tableService.DeleteTableAsync(id, UserId);
            if (!deleted)
                return NotFound();

            return Ok();
        }

        [HttpPut("table/status")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Manager + "," + Roles.Staff + "," + Roles.KitchenStaff)]
        public async Task<IActionResult> SetTableStatus(Guid tableId, [FromBody] TableStatus status, Guid storeId)
        {
            var success = await _tableService.SetTableStatusAsync(tableId, status, UserId, storeId);

            if (!success)
                return NotFound();

            return Ok();
        }

        [Authorize(Roles = Roles.Admin + "," + Roles.Manager)]
        [HttpPut("table/qrcode")]
        public async Task<IActionResult> GenerateQrCodeForTable(Guid tableId, Guid storeId)
        {
            try
            {
                var qrCodeBase64 = await _tableService.GenerateQrCodeForTableAsync("update", tableId, UserId, storeId);

                return Ok(new
                {
                    TableId = tableId,
                    QrCodeBase64 = qrCodeBase64
                });
            }
            catch (CustomException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
