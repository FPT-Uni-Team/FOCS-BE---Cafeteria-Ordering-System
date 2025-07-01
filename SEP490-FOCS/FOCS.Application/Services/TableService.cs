using AutoMapper;
using FOCS.Application.DTOs;
using FOCS.Application.Services.Interface;
using FOCS.Common.Constants;
using FOCS.Common.Enums;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace FOCS.Application.Services
{
    public class TableService : ITableService
    {
        private readonly IRepository<Table> _tableRepository;
        private readonly IMapper _mapper;

        public TableService(IRepository<Table> tableRepository, IMapper mapper)
        {
            _tableRepository = tableRepository;
            _mapper = mapper;
        }

        public async Task<TableDTO> CreateTableAsync(TableDTO dto, string storeId)
        {
            // Check userId
            ConditionCheck.CheckCondition(!string.IsNullOrEmpty(storeId), TableConstants.UserIdEmpty);

            bool exists = await _tableRepository
                                    .AsQueryable()
                                    .AnyAsync(t => t.TableNumber == dto.TableNumber && t.StoreId == dto.StoreId && !t.IsDeleted);
            // Unique table number
            ConditionCheck.CheckCondition(!exists, TableConstants.UniqueTableNumber);


            var table = _mapper.Map<Table>(dto);
            table.Id = Guid.NewGuid();
            table.IsDeleted = false;
            table.CreatedAt = DateTime.UtcNow;
            table.CreatedBy = storeId;

            await _tableRepository.AddAsync(table);
            await _tableRepository.SaveChangesAsync();

            return _mapper.Map<TableDTO>(table);
        }

        public async Task<PagedResult<TableDTO>> GetAllTablesAsync(UrlQueryParameters query, string userId, Guid storeId)
        {
            // Check userId and storeId
            ConditionCheck.CheckCondition(!string.IsNullOrEmpty(userId), TableConstants.UserIdEmpty);

            var tableQuery = _tableRepository.AsQueryable().Where(t => !t.IsDeleted && t.StoreId.Equals(storeId));

            // Search
            if (!string.IsNullOrEmpty(query.SearchBy) && !string.IsNullOrEmpty(query.SearchValue))
            {
                if (query.SearchBy.Equals("table_number", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(query.SearchValue, out int tableNumber))
                {
                    tableQuery = tableQuery.Where(t => t.TableNumber == tableNumber);
                }
            }

            // Sort
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                bool desc = query.SortOrder?.ToLower() == "desc";
                tableQuery = query.SortBy.ToLower() switch
                {
                    "table_number" => desc ? tableQuery.OrderByDescending(t => t.TableNumber) : tableQuery.OrderBy(t => t.TableNumber),
                    "status" => desc ? tableQuery.OrderByDescending(t => t.Status) : tableQuery.OrderBy(t => t.Status),
                    _ => tableQuery
                };
            }

            var total = await tableQuery.CountAsync();
            var items = await tableQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<TableDTO>>(items);
            return new PagedResult<TableDTO>(mapped, total, query.Page, query.PageSize);
        }

        public async Task<TableDTO?> GetTableByIdAsync(Guid id, string userId)
        {
            ConditionCheck.CheckCondition(!string.IsNullOrEmpty(userId), TableConstants.UserIdEmpty);

            var table = await _tableRepository.GetByIdAsync(id);
            if (table == null || table.IsDeleted)
                return null;

            return _mapper.Map<TableDTO>(table);
        }

        public async Task<bool> UpdateTableAsync(Guid id, TableDTO dto, string userId)
        {
            // Check userId
            ConditionCheck.CheckCondition(!string.IsNullOrEmpty(userId), TableConstants.UserIdEmpty);

            var table = await _tableRepository.GetByIdAsync(id);
            if (table == null || table.IsDeleted)
                return false;

            // Unique table number
            bool isDuplicate = await _tableRepository.AsQueryable()
                                                     .AnyAsync(t => t.Id != id &&
                                                                    t.TableNumber == dto.TableNumber &&
                                                                    t.StoreId == dto.StoreId &&
                                                                    !t.IsDeleted);
            ConditionCheck.CheckCondition(!isDuplicate, TableConstants.UniqueTableNumber);

            _mapper.Map(dto, table);
            table.UpdatedAt = DateTime.UtcNow;
            table.UpdatedBy = userId;

            await _tableRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTableAsync(Guid id, string userId)
        {
            // Check userId
            ConditionCheck.CheckCondition(!string.IsNullOrEmpty(userId), TableConstants.UserIdEmpty);

            var table = await _tableRepository.GetByIdAsync(id);
            if (table == null || table.IsDeleted)
                return false;

            table.IsDeleted = true;
            table.UpdatedAt = DateTime.UtcNow;
            table.UpdatedBy = userId;

            await _tableRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetTableStatusAsync(Guid tableId, TableStatus status, string userId, Guid storeId)
        {
            // Check userId
            ConditionCheck.CheckCondition(!string.IsNullOrEmpty(userId), TableConstants.UserIdEmpty);
            // Check TableStatus
            ConditionCheck.CheckCondition(Enum.IsDefined(typeof(TableStatus), status), TableConstants.InvalidTableStatus);

            var table = await _tableRepository
                                    .AsQueryable()
                                    .Where(t => t.Id == tableId && t.StoreId == storeId && !t.IsDeleted)
                                    .FirstOrDefaultAsync();

            // Check table
            ConditionCheck.CheckCondition(table != null, TableConstants.TableEmpty);

            table.Status = status;
            table.UpdatedAt = DateTime.UtcNow;
            table.UpdatedBy = userId;

            await _tableRepository.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateQrCodeForTableAsync(Guid tableId, string userId, Guid storeId)
        {
            // Check user empty
            ConditionCheck.CheckCondition(!string.IsNullOrEmpty(userId), TableConstants.UserIdEmpty);

            // Find table
            var table = await _tableRepository
                                .AsQueryable()
                                .FirstOrDefaultAsync(t => t.Id == tableId && t.StoreId == storeId && !t.IsDeleted);
            //Check table
            ConditionCheck.CheckCondition(table != null, TableConstants.TableEmpty);

            var qrCodeBase64 = GenerateQrCodeBytes(tableId);
            table.QrCode = qrCodeBase64;
            table.UpdatedAt = DateTime.UtcNow;
            table.UpdatedBy = userId;

            await _tableRepository.SaveChangesAsync();
            return qrCodeBase64;
        }
        public string GenerateQrCodeBytes(Guid tableId)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(tableId.ToString(), QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }

    }
}
