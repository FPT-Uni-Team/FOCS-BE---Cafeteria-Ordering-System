using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FOCS.Infrastructure.Identity.Common.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(object id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task AddAsync(TEntity entity);

        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        void RemoveRange(List<TEntity> entity);

        Task<int> SaveChangesAsync();
        IQueryable<TEntity> AsQueryable();
        Task<IQueryable<TEntity>> IncludeAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> include, bool asNoTracking = false);
    }
}