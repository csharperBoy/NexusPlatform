using Core.Application.Abstractions;
using Core.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Repositories
{
    public class EfSpecificationRepository<TEntity, TKey> : ISpecificationRepository<TEntity, TKey>
         where TEntity : class
         where TKey : IEquatable<TKey>
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<TEntity> _dbSet;

        public EfSpecificationRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetBySpecAsync(ISpecification<TEntity> specification)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> ListBySpecAsync(ISpecification<TEntity> specification)
        {
            return await ApplySpecification(specification).ToListAsync();
        }

        public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> FindBySpecAsync(ISpecification<TEntity> specification)
        {
            // Count with filter only to avoid heavy includes
            var countQuery = _dbSet.AsQueryable();
            if (specification.Criteria != null)
                countQuery = countQuery.Where(specification.Criteria);
            var totalCount = await countQuery.CountAsync();

            var query = ApplySpecification(specification);
            if (specification.IsPagingEnabled)
                query = query.Skip(specification.Skip).Take(specification.Take);

            var items = await query.ToListAsync();
            return (items, totalCount);
        }

        public virtual async Task<int> CountBySpecAsync(ISpecification<TEntity> specification)
        {
            var query = _dbSet.AsQueryable();
            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);
            return await query.CountAsync();
        }

        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            IQueryable<TEntity> query = _dbSet;

            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
            query = specification.IncludeFunctions.Aggregate(query, (current, includeFunction) => includeFunction(current));
            query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

            query = ApplyOrdering(query, specification);

            return query;
        }

        private IQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> query, ISpecification<TEntity> specification)
        {
            IOrderedQueryable<TEntity>? orderedQuery = null;

            if (specification.OrderBy != null)
                orderedQuery = query.OrderBy(specification.OrderBy);
            else if (specification.OrderByDescending != null)
                orderedQuery = query.OrderByDescending(specification.OrderByDescending);

            if (orderedQuery != null && specification.ThenOrderBy.Any())
            {
                foreach (var (keySelector, isDescending) in specification.ThenOrderBy)
                {
                    orderedQuery = isDescending ?
                        orderedQuery.ThenByDescending(keySelector) :
                        orderedQuery.ThenBy(keySelector);
                }
                return orderedQuery;
            }

            return orderedQuery ?? query;
        }
    }
}