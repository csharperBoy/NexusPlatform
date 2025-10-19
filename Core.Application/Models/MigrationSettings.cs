using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Models
{
    public class MigrationSettings
    {
        public string Strategy { get; set; } = "Resilient"; // Resilient, Force, Skip, HealthAware
        public int MaxRetryCount { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 2;
        public bool EnableHealthChecks { get; set; } = true;
        public bool SafeModeInProduction { get; set; } = true;
        public string[] CriticalDbContexts { get; set; } = new[] { "AuthDbContext", "UserManagementDbContext" };
    }
}
