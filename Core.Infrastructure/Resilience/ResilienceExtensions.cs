using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;
using Polly.Timeout;
using Polly;
using Microsoft.Data.SqlClient;
using System.Net.Mail;
namespace Core.Infrastructure.Resilience
{
    /*
     📌 ResilienceExtensions
     -----------------------
     این کلاس مجموعه‌ای از **Extension Methods** برای IServiceCollection است
     که وظیفه‌ی ثبت و پیکربندی سیاست‌های Resilience (تحمل خطا) با استفاده از کتابخانه Polly را بر عهده دارد.

     ✅ نکات کلیدی:
     - AddResiliencePolicies:
       • خواندن تنظیمات از بخش "Resilience:Policies" در IConfiguration.
       • برای هر Policy تعریف‌شده در تنظیمات، نوع آن بررسی می‌شود:
         1. Retry → سیاست تکرار با Backoff و Jitter.
         2. CircuitBreaker → سیاست قطع مدار پس از تعداد مشخصی خطا.
         3. Timeout → سیاست محدودیت زمان اجرای عملیات.
       • همه‌ی Policyها در PolicyRegistry ذخیره می‌شوند.
       • Registry به صورت Singleton در DI ثبت می‌شود تا در کل سیستم قابل استفاده باشد.

     - BuildRetryPolicy:
       • خواندن تعداد Retryها، BaseDelay و Jitter از تنظیمات.
       • تعریف Predicate برای Exceptionهای قابل Retry (SqlException, SmtpException یا خطاهای Transient).
       • محاسبه Delayها با Exponential Backoff و Jitter اختیاری.
       • ساخت Policy با WaitAndRetryAsync.

     - BuildCircuitBreaker:
       • خواندن تعداد Failures و مدت زمان Break از تنظیمات.
       • ساخت Policy با CircuitBreakerAsync برای جلوگیری از فشار مداوم روی سرویس‌های خراب.

     - BuildTimeoutPolicy:
       • خواندن TimeoutSeconds از تنظیمات.
       • ساخت Policy با TimeoutAsync (Optimistic Strategy).

     - IsTransient:
       • Heuristic ساده برای تشخیص خطاهای موقت (Timeout, Deadlock, Network, Connection).
       • قابل توسعه برای شرایط خاص.

     🛠 جریان کار:
     1. در زمان راه‌اندازی اپلیکیشن، این Extension فراخوانی می‌شود:
        services.AddResiliencePolicies(configuration);
     2. همه‌ی Policyها از تنظیمات خوانده و در Registry ثبت می‌شوند.
     3. سرویس‌ها می‌توانند با استفاده از Registry به این Policyها دسترسی داشته باشند.
     4. عملیات حساس (مثل دسترسی به دیتابیس یا سرویس‌های خارجی) با این Policyها اجرا می‌شوند.
     5. سیستم در برابر خطاهای موقت مقاوم‌تر می‌شود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Resilience & Fault Tolerance** در معماری ماژولار است
     و تضمین می‌کند که سرویس‌ها در برابر خطاهای موقت پایدارتر باشند و تجربه‌ی کاربری بهبود یابد.
    */

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
                        // 📌 می‌توان لاگ هشدار زد یا از سیاست پیش‌فرض استفاده کرد
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

            // 📌 تعیین نوع Exceptionهای قابل Retry
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

        // 📌 Heuristic ساده برای تشخیص خطاهای موقت
        private static bool IsTransient(Exception ex)
        {
            var msg = ex.Message.ToLowerInvariant();
            return msg.Contains("timeout") || msg.Contains("deadlock") || msg.Contains("network") || msg.Contains("connection");
        }
    }
}
