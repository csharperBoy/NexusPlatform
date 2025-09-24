using Core.Application.Abstractions;
using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Infrastructure.Repositories
{
    public class EfRepository<T, TContext> : IRepository<T>
        where T : BaseEntity
        where TContext : DbContext
    {
        private readonly TContext _db;
        private readonly DbSet<T> _set;

        public EfRepository(TContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
            => await _set.AddAsync(entity, cancellationToken);

        public IQueryable<T> Query() => _set.AsQueryable();

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _set.FindAsync(new object[] { id }, cancellationToken);

        public void Remove(T entity) => _set.Remove(entity);

        public void Update(T entity) => _set.Update(entity);
    }
}
