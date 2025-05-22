using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FOCS.Infrastructure.Identity.Common.Repositories;

namespace FOCS.Infrastructure.Identity.Common.UnitOfWorks
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync();

        Task BeginTransactionAsync();

        Task CommitAsync();

        Task RollbackAsync();
    }
}
