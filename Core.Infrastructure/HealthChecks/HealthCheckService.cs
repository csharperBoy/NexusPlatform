using Core.Application.Abstractions.Caching;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.HealthChecks
{/*
    public class HealthCheckService : IHealthCheckService
    {
        private readonly IEnumerable<IHealthContributor> _contributors;

        public HealthCheckService(IEnumerable<IHealthContributor> contributors)
        {
            _contributors = contributors;
        }

        public async Task<SystemStatus> GetSystemStatusAsync()
        {
            var results = await Task.WhenAll(_contributors.Select(c => c.CheckAsync()));
            var isHealthy = results.All(r => r.IsHealthy);
            var message = isHealthy ? "All systems operational" :
                string.Join("; ", results.Select((r, i) => $"{_contributors.ElementAt(i).Name}: {r.IsHealthy}"));

            return new SystemStatus(isHealthy, message, DateTime.UtcNow);
        }

        public async Task<IReadOnlyList<(string Name, bool IsHealthy, string Message, long ResponseTimeMs)>> GetDetailsAsync()
        {
            var results = await Task.WhenAll(_contributors.Select(c => c.CheckAsync()));
            return results.Select((r, i) => (_contributors.ElementAt(i).Name, r.IsHealthy, r.Message, r.ResponseTimeMs)).ToList();
        }
    }
*/}
