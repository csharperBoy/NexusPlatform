using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.HealthChecks
{
    public class DatabaseHealthContributor<TDbContext> : IHealthContributor
         where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;
        public string Name => "Database";

        public DatabaseHealthContributor(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsHealthy, string Message, long ResponseTimeMs)> CheckAsync()
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var canConnect = await _dbContext.Database.CanConnectAsync();
                sw.Stop();
                return (canConnect, canConnect ? "Database connection successful" : "Database connection failed", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                return (false, $"Database error: {ex.Message}", 0);
            }
        }
    }

}
