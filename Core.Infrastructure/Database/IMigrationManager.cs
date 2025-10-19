// Core/Infrastructure/Database/IMigrationManager.cs
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Database
{
    public interface IMigrationManager
    {
        Task MigrateAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext;
        Task<bool> HasPendingMigrationsAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext;
    }
}