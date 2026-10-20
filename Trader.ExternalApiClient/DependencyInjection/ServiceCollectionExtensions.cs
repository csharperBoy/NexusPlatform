using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trader.ExternalApiClient.EasyTrader;

namespace Trader.ExternalApiClient.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Trader_ExternalApiClient_DependencyInjection(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<EasyTraderOptions>(
                configuration.GetSection(EasyTraderOptions.SectionName));

            services.AddHttpClient("EasyTrader", client =>
            {
                // ─── کلید Fix: Timeout کوتاه ───
                // هر requestی که بیش از ۱۰ ثانیه طول بکشد، Abort می‌شود
                // اگر Stall رخ داد، فقط ۱۰ ثانیه طول می‌کشد نه ۳۰
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                // ─── کلید Fix اصلی ───
                // Connectionهای Pool را بعد از ۲۰ ثانیه Dispose کن
                // سرور Idle Timeout تقریباً 30-60s است
                // ۲۰ ثانیه < ۳۰ ثانیه ⇒ هیچ‌وقت Connection مرده Reuse نمی‌شود
                PooledConnectionLifetime = TimeSpan.FromSeconds(20),

                // Connectionهایی که ۱۵ ثانیه بی‌استفاده مانده‌اند را ببند
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(15),

                // ─── بقیه‌ی تنظیمات ───
                MaxConnectionsPerServer = 20,
                AutomaticDecompression = System.Net.DecompressionMethods.All,
                ConnectTimeout = TimeSpan.FromSeconds(10),
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 3,
            });

            services.AddSingleton<EasyTraderApiClient>();

            return services;
        }
    }
}