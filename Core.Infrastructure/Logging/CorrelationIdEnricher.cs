using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Logging
{
    public class CorrelationIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var correlationId = GetCorrelationId();
            if (!string.IsNullOrEmpty(correlationId))
            {
                var property = propertyFactory.CreateProperty("CorrelationId", correlationId);
                logEvent.AddPropertyIfAbsent(property);
            }
        }

        private static string GetCorrelationId()
        {
            var httpContextAccessor = new HttpContextAccessor();
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext?.Request?.Headers != null)
            {
                if (httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
                    return correlationId;

                if (httpContext.Request.Headers.TryGetValue("Request-Id", out var requestId))
                    return requestId;
            }

            return Guid.NewGuid().ToString();
        }
    }

  }
