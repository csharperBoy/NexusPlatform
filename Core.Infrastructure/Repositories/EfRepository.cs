using Core.Application.Abstractions;
using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Infrastructure.Repositories
{
    public class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey>
          where TEntity : class
          where TKey : IEquatable<TKey>
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<TEntity> _dbSet;

        public EfRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
        }

        #region Basic CRUD
        public virtual async Task<TEntity?> GetByIdAsync(TKey id) =>
            await _dbSet.FindAsync(id);

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync() =>
            await _dbSet.ToListAsync();

        public virtual async Task AddAsync(TEntity entity) =>
            await _dbSet.AddAsync(entity);

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities) =>
            await _dbSet.AddRangeAsync(entities);

        public virtual Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public virtual Task DeleteAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual Task RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }
        #endregion

        #region Query Operations
        public virtual async Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            IQueryable<TEntity> query = _dbSet;
            if (include != null)
            {
                query = include(query);
            }
            return await query.Where(predicate).ToListAsync();
        }

        public virtual async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            IQueryable<TEntity> query = _dbSet;
            if (include != null)
            {
                query = include(query);
            }
            return await query.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<TEntity?> SingleOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
             Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            IQueryable<TEntity> query = _dbSet;
            if (include != null)
            {
                query = include(query);
            }
            return await query.SingleOrDefaultAsync(predicate);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null) =>
            predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

        public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            int pageIndex = 0,
            int pageSize = 20,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            IQueryable<TEntity> query = _dbSet;
            if (predicate != null) query = query.Where(predicate);

            if (include != null)
            {
                query = include(query);
            }
            var totalCount = await query.CountAsync();
            if (orderBy != null) query = orderBy(query);

            var items = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public virtual IQueryable<TEntity> AsQueryable() => _dbSet;

        public virtual IQueryable<TEntity> AsNoTrackingQueryable() => _dbSet.AsNoTracking();
        #endregion
    }
}
