using FOCS.Order.Infrastucture.Common.Repositories;
using FOCS.Order.Infrastucture.Context;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace FOCS.Order.Infrastucture.Common.UnitOfWorks
{
    public class UnitOfWork<TContext> : IUnitOfWork where TContext : OrderDbContext
    {
        private readonly TContext _context;
        private IDbContextTransaction? _transaction;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();

        public UnitOfWork(TContext context)
        {
            _context = context;
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);

            if (_repositories.ContainsKey(type))
                return (IRepository<TEntity>)_repositories[type]!;

            var repositoryInstance = new Repository<TEntity>(_context);
            _repositories.TryAdd(type, repositoryInstance);
            return repositoryInstance;
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
                _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
