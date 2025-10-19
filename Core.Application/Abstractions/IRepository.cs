using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions
{
    public interface IRepository<TDbContext, TEntity, TKey>
       where TDbContext : DbContext
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
        Task<(IEnumerable<TEntity> Items, int TotalCount)> FindAsync(
              Expression<Func<TEntity, bool>>? predicate = null,
              Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
              Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
              int? pageIndex = null,
              int? pageSize = null);
        Task<(IEnumerable<TEntity> Items, int TotalCount)> FindAsync(
             Expression<Func<TEntity, bool>>? predicate = null,
             Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
             int? pageIndex = null,
             int? pageSize = null,
             params Expression<Func<TEntity, object>>[] includes
            );

        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task<TEntity?> SingleOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
           Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);


        IQueryable<TEntity> AsQueryable();
        IQueryable<TEntity> AsNoTrackingQueryable();
        #endregion
    }
}
