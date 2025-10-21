using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Logging
{
    public class SerilogLoggingService : ILoggingService
    {
        private readonly ILogger<SerilogLoggingService> _logger;

        public SerilogLoggingService(ILogger<SerilogLoggingService> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message, params object[] args)
            => _logger.LogInformation(message, args);

        public void LogWarning(string message, params object[] args)
            => _logger.LogWarning(message, args);

        public void LogError(Exception exception, string message, params object[] args)
            => _logger.LogError(exception, message, args);

        public void LogDebug(string message, params object[] args)
            => _logger.LogDebug(message, args);

        public IDisposable BeginScope(string scopeName)
            => _logger.BeginScope(scopeName);
    }
}
