using FOCS.Application.DTOs;
using FOCS.Common.Enums;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface ITableService
    {
        Task<TableDTO> CreateTableAsync(TableDTO dto, string userId);
        Task<PagedResult<TableDTO>> GetAllTablesAsync(UrlQueryParameters query, string userId, Guid storeId);
        Task<TableDTO> GetTableByIdAsync(Guid id, string userId);
        Task<bool> UpdateTableAsync(Guid id, TableDTO dto, string userId);
        Task<bool> DeleteTableAsync(Guid id, string userId);
        Task<bool> SetTableStatusAsync(Guid tableId, TableStatus status, string userId, Guid storeId);
        Task<string> GenerateQrCodeForTableAsync(Guid tableId, string userId, Guid storeId);
    }
}
