using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;
using Polly.Timeout;
using Polly;
using Microsoft.Data.SqlClient;
using System.Net.Mail;
namespace Core.Infrastructure.Resilience
{
    public static class ResilienceExtensions
    {
        public static IServiceCollection AddResiliencePolicies(this IServiceCollection services, IConfiguration configuration)
        {
            var registry = new PolicyRegistry();

            var section = configuration.GetSection("Resilience:Policies");
            foreach (var policySection in section.GetChildren())
            {
                var name = policySection.Key;
                var type = policySection["Type"]?.Trim();

                switch (type)
                {
                    case "Retry":
                        registry.Add(name, BuildRetryPolicy(policySection));
                        break;
                    case "CircuitBreaker":
                        registry.Add(name, BuildCircuitBreaker(policySection));
                        break;
                    case "Timeout":
                        registry.Add(name, BuildTimeoutPolicy(policySection));
                        break;
                    default:
                        // می‌توان لاگ هشدار زد یا از سیاست پیش‌فرض استفاده کرد
                        break;
                }
            }

            services.AddSingleton<IReadOnlyPolicyRegistry<string>>(registry);
            return services;
        }

        private static IAsyncPolicy BuildRetryPolicy(IConfigurationSection s)
        {
            var retries = int.TryParse(s["Retries"], out var r) ? r : 3;
            var baseDelay = int.TryParse(s["BaseDelaySeconds"], out var d) ? d : 2;
            var jitter = bool.TryParse(s["Jitter"], out var j) && j;
            var handle = s["Handle"];

            // تعیین نوع Exceptionهای قابل Retry
            Func<Exception, bool> predicate = handle switch
            {
                "SqlException" => ex => ex is SqlException || IsTransient(ex),
                "SmtpException" => ex => ex is SmtpException || IsTransient(ex),
                _ => ex => IsTransient(ex) || ex is Exception
            };

            var delays = Enumerable.Range(1, retries)
                .Select(i =>
                {
                    var backoff = TimeSpan.FromSeconds(Math.Pow(baseDelay, i));
                    if (jitter)
                    {
                        var rand = new Random();
                        var jitterMs = rand.Next(50, 250);
                        backoff = backoff + TimeSpan.FromMilliseconds(jitterMs);
                    }
                    return backoff;
                });

            return Policy
                .Handle<Exception>(predicate)
                .WaitAndRetryAsync(delays);
        }

        private static IAsyncPolicy BuildCircuitBreaker(IConfigurationSection s)
        {
            var failures = int.TryParse(s["Failures"], out var f) ? f : 5;
            var breakSeconds = int.TryParse(s["BreakDurationSeconds"], out var b) ? b : 60;

            return Policy
                .Handle<Exception>(IsTransient)
                .CircuitBreakerAsync(failures, TimeSpan.FromSeconds(breakSeconds));
        }

        private static IAsyncPolicy BuildTimeoutPolicy(IConfigurationSection s)
        {
            var timeoutSeconds = int.TryParse(s["TimeoutSeconds"], out var t) ? t : 10;
            return Policy.TimeoutAsync(TimeSpan.FromSeconds(timeoutSeconds), TimeoutStrategy.Optimistic);
        }

        // Transient heuristic قابل توسعه
        private static bool IsTransient(Exception ex)
        {
            var msg = ex.Message.ToLowerInvariant();
            return msg.Contains("timeout") || msg.Contains("deadlock") || msg.Contains("network") || msg.Contains("connection");
        }
    }
}
