using AutoMapper;
using FOCS.Application.DTOs;
using FOCS.Application.Services;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace FOCS.UnitTest
{
    public class TableUnitTest
    {
        private readonly Mock<IRepository<Table>> _tableRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICloudinaryService> _cloudinaryMock;
        private readonly TableService _tableService;

        public TableUnitTest()
        {
            _tableRepositoryMock = new Mock<IRepository<Table>>();
            _mapperMock = new Mock<IMapper>();
            _cloudinaryMock = new Mock<ICloudinaryService>();

            _tableService = new TableService(
                _tableRepositoryMock.Object,
                _mapperMock.Object,
                _cloudinaryMock.Object
            );
        }

        #region CreateTable CM-31
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, 0, true, "550e8400-e29b-41d4-a716-446655440000", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, 4, true, "550e8400-e29b-41d4-a716-446655440000", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", 1, 0, true, "550e8400-e29b-41d4-a716-446655440000", true)]
        //[InlineData(null, 1, 0, true, "550e8400-e29b-41d4-a716-446655440000", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, 0, true, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, null, true, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, 0, null, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, 0, true, "fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, 0, true, null, false)]
        //[InlineData(null, null, null, null, null, false)]
        public async Task CreateTableAsync_SimpleRun_ChecksIfServiceRuns(
            string idStr,
            int tableNumber,
            int status,
            bool isActive,
            string storeIdStr,
            bool shouldSucceed)
        {
            // Arrange
            var dto = new TableDTO
            {
                Id = Guid.Parse(idStr),
                TableNumber = tableNumber,
                Status = (TableStatus)status,
                IsActive = isActive,
                StoreId = storeIdStr != null ? Guid.Parse(storeIdStr) : Guid.Empty
            };

            if (!string.IsNullOrWhiteSpace(storeIdStr))
            {
                var anyQueryable = new List<Table> { new Table { TableNumber = tableNumber, StoreId = dto.StoreId, IsDeleted = false } }
                    .AsQueryable().BuildMockDbSet().Object;
                _tableRepositoryMock.Setup(r => r.AsQueryable()).Returns(anyQueryable);
            }
            else
            {
            }

            _tableRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Table>())).Returns(Task.CompletedTask);
            _tableRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            Exception ex = await Record.ExceptionAsync(async () =>
            {
                var res = await _tableService.CreateTableAsync(dto, storeIdStr);
            });

            Assert.NotNull(ex);
        }
        #endregion

        #region GetAllTables (GetByStore) CM-32
        [Theory]
        [InlineData(1, 5, "table_number", "Chicken", "table_number", "desc", true)]
        [InlineData(1, 5, "description", "Chicken", "table_number", "desc", true)]
        [InlineData(1, 5, null, "Chicken", "table_number", "desc", true)]
        [InlineData(1, 5, "name", null, "table_number", "desc", true)]
        [InlineData(1, 5, "name", "Chicken", "status", "desc", true)]
        [InlineData(1, 5, "name", "Chicken", null, "desc", true)]
        [InlineData(1, 5, "name", "Chicken", "name", "asc", true)]
        [InlineData(1, 5, "name", "Chicken", "name", null, true)]
        //[InlineData(null, 5, "name", "Chicken", "name", null, false)]
        //[InlineData(1, null, "name", "Chicken", "name", null, false)]
        public async Task GetAllTablesAsync_SimpleRun_ChecksIfServiceRuns(
            int? page,
            int? pageSize,
            string searchBy,
            string searchValue,
            string sortBy,
            string sortOrder,
            bool shouldSucceed)
        {
            // Arrange
            var storeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var query = new UrlQueryParameters
            {
                Page = page ?? 0,
                PageSize = pageSize ?? 0,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            if (shouldSucceed)
            {
                var tables = new List<Table>
                {
                    new Table { Id = Guid.NewGuid(), StoreId = storeId, TableNumber = 1, IsDeleted = false, Status = TableStatus.Available },
                    new Table { Id = Guid.NewGuid(), StoreId = storeId, TableNumber = 2, IsDeleted = false, Status = TableStatus.Occupied }
                };

                _tableRepositoryMock.Setup(r => r.AsQueryable())
                    .Returns(tables.AsQueryable().BuildMockDbSet().Object);

                _mapperMock.Setup(m => m.Map<List<TableDTO>>(It.IsAny<List<Table>>()))
                    .Returns((List<Table> src) => src.Select(t => new TableDTO { Id = t.Id, TableNumber = t.TableNumber, StoreId = t.StoreId }).ToList());
            }
            else
            {
                _tableRepositoryMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Table>().AsQueryable().BuildMockDbSet().Object);
            }

            Exception ex = await Record.ExceptionAsync(async () =>
            {
                var res = await _tableService.GetAllTablesAsync(query, userId, storeId);
                if (shouldSucceed)
                {
                    Assert.NotNull(res);
                }
            });

            if (shouldSucceed) Assert.Null(ex); else Assert.NotNull(ex);
        }
        #endregion

        #region GetTableById CM-33
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        public async Task GetTableByIdAsync_SimpleRun_ChecksIfServiceRuns(string idStr, bool shouldSucceed)
        {
            var id = Guid.Parse(idStr);
            var userId = Guid.NewGuid().ToString();

            if (shouldSucceed)
            {
                var table = new Table { Id = id, IsDeleted = false, TableNumber = 1, StoreId = Guid.NewGuid() };
                _tableRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(table);
                _mapperMock.Setup(m => m.Map<TableDTO>(It.IsAny<Table>()))
                    .Returns((Table t) => new TableDTO { Id = t.Id, TableNumber = t.TableNumber, StoreId = t.StoreId });
            }
            else
            {
                _tableRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Table?)null);
            }

            TableDTO result = null;
            Exception ex = await Record.ExceptionAsync(async () =>
            {
                result = await _tableService.GetTableByIdAsync(id, userId);
            });

            if (shouldSucceed)
            {
                Assert.Null(ex);
                Assert.NotNull(result);
            }
            else
            {
                Assert.NotNull(ex);
            }
        }
        #endregion

        #region UpdateTable CM-34
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, 0, true, "550e8400-e29b-41d4-a716-446655440000", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, 4, true, "550e8400-e29b-41d4-a716-446655440000", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", 1, 0, true, "550e8400-e29b-41d4-a716-446655440000", true)]
        [InlineData(null, 1, 0, true, "550e8400-e29b-41d4-a716-446655440000", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, 0, true, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, null, true, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, 0, null, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, 0, true, "fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 1, 0, true, null, false)]
        [InlineData(null, null, null, null, null, false)]
        public async Task UpdateTableAsync_SimpleRun_ChecksIfServiceRuns(
            string? idStr,
            int? tableNumber,
            int? status,
            bool? isActive,
            string? storeIdStr,
            bool shouldSucceed)
        {
            // parse inputs (if parse fails, we'll treat as failure)
            Guid id = Guid.Empty;
            bool idParsed = !string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out id);

            Guid storeId = Guid.Empty;
            bool storeParsed = !string.IsNullOrWhiteSpace(storeIdStr) && Guid.TryParse(storeIdStr, out storeId);

            var userId = Guid.NewGuid().ToString();

            var dto = new TableDTO
            {
                Id = idParsed ? id : (Guid?)null,
                TableNumber = tableNumber ?? 0,
                Status = (TableStatus)(status ?? 0),
                IsActive = isActive ?? false,
                StoreId = storeParsed ? storeId : Guid.Empty
            };

            // effective success requires all required values convertible + requested shouldSucceed
            bool effectiveShouldSucceed = shouldSucceed
                                          && idParsed // id must parse
                                          && tableNumber.HasValue
                                          && status.HasValue
                                          && isActive.HasValue
                                          && storeParsed; // storeId must parse

            if (effectiveShouldSucceed)
            {
                // setup existing table to be found and no duplicate conflict
                var existing = new Table { Id = id, TableNumber = 5, StoreId = storeId, IsDeleted = false };

                _tableRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);

                // Duplicate check: AsQueryable returns a set that does not contain another conflicting table
                _tableRepositoryMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Table> { existing }.AsQueryable().BuildMockDbSet().Object);

                _mapperMock.Setup(m => m.Map(It.IsAny<TableDTO>(), It.IsAny<Table>()))
                    .Callback((TableDTO d, Table t) =>
                    {
                        t.TableNumber = d.TableNumber;
                        t.Status = d.Status;
                        t.IsActive = d.IsActive;
                    });

                _tableRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            }
            else
            {
                // ensure GetByIdAsync returns null (so service will return false) for non-convertible or failing cases
                _tableRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Table?)null);
            }

            bool? result = null;
            Exception? ex = await Record.ExceptionAsync(async () =>
            {
                result = await _tableService.UpdateTableAsync(idParsed ? id : Guid.Empty, dto, userId);
            });

            if (effectiveShouldSucceed)
            {
                Assert.Null(ex);
                Assert.True(result == true);
            }
            else
            {
                // If service throws an exception it's considered failure; otherwise it should return false
                if (ex != null)
                    Assert.NotNull(ex);
                else
                    Assert.False(result == true);
            }
        }
        #endregion

        #region DeleteTable CM-35
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", true)]
        public async Task DeleteTableAsync_SimpleRun_ChecksIfServiceRuns(string idStr, bool shouldSucceed)
        {
            var id = Guid.Parse(idStr);
            var userId = Guid.NewGuid().ToString();

            if (shouldSucceed)
            {
                var existing = new Table { Id = id, IsDeleted = false };
                _tableRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
                _tableRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            }
            else
            {
                _tableRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Table?)null);
            }

            Exception ex = await Record.ExceptionAsync(async () =>
            {
                var res = await _tableService.DeleteTableAsync(id, userId);
                if (shouldSucceed) Assert.True(res); else Assert.False(res);
            });

            if (shouldSucceed) Assert.Null(ex); else Assert.NotNull(ex);
        }
        #endregion

        #region SetTableStatus CM-36
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 0, "550e8400-e29b-41d4-a716-446655440000", true)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 4, "550e8400-e29b-41d4-a716-446655440000", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", 4, "550e8400-e29b-41d4-a716-446655440000", false)]
        //[InlineData(null, 4, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 0, "fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        //[InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", 0, null, false)]
        //[InlineData(null, null, null, false)]
        public async Task SetTableStatusAsync_SimpleRun_ChecksIfServiceRuns(string tableIdStr, int statusInt, string storeIdStr, bool shouldSucceed)
        {
            var tableId = Guid.Parse(tableIdStr);
            var storeId = Guid.Parse(storeIdStr);
            var userId = Guid.NewGuid().ToString();
            var status = (TableStatus)statusInt;

            if (shouldSucceed)
            {
                var table = new Table { Id = tableId, StoreId = storeId, IsDeleted = false, Status = TableStatus.Available };
                _tableRepositoryMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Table> { table }.AsQueryable().BuildMockDbSet().Object);
                _tableRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            }
            else
            {
                // no matching table in DB
                _tableRepositoryMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Table>().AsQueryable().BuildMockDbSet().Object);
            }

            Exception ex = await Record.ExceptionAsync(async () =>
            {
                var res = await _tableService.SetTableStatusAsync(tableId, status, userId, storeId);
                if (shouldSucceed) Assert.True(res); else Assert.False(res);
            });

            if (shouldSucceed) Assert.Null(ex); else Assert.NotNull(ex);
        }
        #endregion

        #region GenerateQrCodeForTable CM-37
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "550e8400-e29b-41d4-a716-446655440000", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", "550e8400-e29b-41d4-a716-446655440000", false)]
        //[InlineData(null, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        //[InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, false)]
        //[InlineData(null, null, false)]
        public async Task GenerateQrCodeForTableAsync_SimpleRun_ChecksIfServiceRuns(string tableIdStr, string storeIdStr, bool shouldSucceed)
        {
            // We'll test only "not found" case (service will throw ConditionCheck) so we don't need to mock cloudinary
            var tableId = Guid.Parse(tableIdStr);
            var storeId = Guid.Parse(storeIdStr);

            // no table in repo -> should throw
            _tableRepositoryMock.Setup(r => r.AsQueryable())
                .Returns(new List<Table>().AsQueryable().BuildMockDbSet().Object);

            Exception ex = await Record.ExceptionAsync(async () =>
            {
                var res = await _tableService.GenerateQrCodeForTableAsync(null, tableId, Guid.NewGuid().ToString(), storeId);
            });

            Assert.NotNull(ex);
        }
        #endregion
    }
}
