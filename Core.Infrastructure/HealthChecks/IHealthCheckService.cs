using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.HealthChecks
{
    public interface IHealthCheckService
    {
        Task<SystemStatus> GetSystemStatusAsync();
        Task<DatabaseStatus> GetDatabaseStatusAsync();
        Task<CacheStatus> GetCacheStatusAsync();
    }

    public record SystemStatus(bool IsHealthy, string Message, DateTime CheckedAt);
    public record DatabaseStatus(bool IsConnected, string Message, long ResponseTimeMs);
    public record CacheStatus(bool IsConnected, string Message, long ResponseTimeMs);
}
