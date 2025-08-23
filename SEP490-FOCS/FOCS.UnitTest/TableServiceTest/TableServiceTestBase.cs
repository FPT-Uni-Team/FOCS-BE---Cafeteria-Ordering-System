using AutoMapper;
using FOCS.Application.Services;
using FOCS.Common.Interfaces;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.TableServiceTest
{
    public class TableServiceTestBase
    {
        protected readonly Mock<IRepository<Table>> _tableRepositoryMock;
        protected readonly Mock<IMapper> _mapperMock;
        protected readonly Mock<ICloudinaryService> _cloudinaryServiceMock;

        protected readonly TableService _tableService;

        protected TableServiceTestBase()
        {
            _tableRepositoryMock = new Mock<IRepository<Table>>();
            _mapperMock = new Mock<IMapper>();
            _cloudinaryServiceMock = new Mock<ICloudinaryService>();

            _tableService = new TableService(
                _tableRepositoryMock.Object,
                _mapperMock.Object,
                _cloudinaryServiceMock.Object
            );
        }

        protected void SetupQueryable(IEnumerable<Table> tables)
        {
            var queryable = tables.AsQueryable();

            _tableRepositoryMock.Setup(r => r.AsQueryable())
                                .Returns(queryable);
        }

        protected void SetupGetById(Guid id, Table? table)
        {
            _tableRepositoryMock.Setup(r => r.GetByIdAsync(id))
                                .ReturnsAsync(table);
        }

        protected void VerifySaveChanges(Times times)
        {
            _tableRepositoryMock.Verify(r => r.SaveChangesAsync(), times);
        }

        protected void VerifyAdd(Table table, Times times)
        {
            _tableRepositoryMock.Verify(r => r.AddAsync(table), times);
        }
    }
}
