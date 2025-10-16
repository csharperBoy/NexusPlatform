using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions
{
    public interface IRepository<TEntity, TKey>
        where TEntity : class
        where TKey : IEquatable<TKey>
    {
        #region Basic CRUD
        Task<TEntity?> GetByIdAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TKey id);
        Task DeleteAsync(TEntity entity);
        Task RemoveRangeAsync(IEnumerable<TEntity> entities);
        #endregion

        #region Query Operations
        Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
             Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task<TEntity?> SingleOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
           Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

        Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            int pageIndex = 0,
            int pageSize = 20,
             Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        IQueryable<TEntity> AsQueryable();
        IQueryable<TEntity> AsNoTrackingQueryable();
        #endregion
    }
}
