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
            var query = ApplySpecification(specification, true);
            var totalCount = await query.CountAsync();

            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip).Take(specification.Take);
            }

            var items = await query.ToListAsync();
            return (items, totalCount);
        }

        public virtual async Task<int> CountBySpecAsync(ISpecification<TEntity> specification)
        {
            return await ApplySpecification(specification, true).CountAsync();
        }

        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification, bool forCount = false)
        {
            IQueryable<TEntity> query = _dbSet;

            // اعمال فیلتر (Criteria)
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // اعمال Includes با استفاده از Expression
            query = specification.Includes
                .Aggregate(query, (current, include) => current.Include(include));

            // اعمال Includes با استفاده از رشته
            query = specification.IncludeStrings
                .Aggregate(query, (current, include) => current.Include(include));

            // اعمال مرتب‌سازی (اگر برای شمارش نیست)
            if (!forCount)
            {
                if (specification.OrderBy != null)
                {
                    query = query.OrderBy(specification.OrderBy);
                }
                else if (specification.OrderByDescending != null)
                {
                    query = query.OrderByDescending(specification.OrderByDescending);
                }

                // اعمال GroupBy
                if (specification.GroupBy != null)
                {
                    query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
                }
            }

            return query;
        }
    }
}
