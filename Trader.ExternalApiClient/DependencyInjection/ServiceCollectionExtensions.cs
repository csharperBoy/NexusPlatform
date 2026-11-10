using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trader.ExternalApiClient.EasyTrader;
using Trader.ExternalApiClient.Tgju;

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

            //***********************************************//

            // ─── TGJU ───
            services.Configure<TgjuOptions>(
                configuration.GetSection(TgjuOptions.SectionName));

            services.AddHttpClient("Tgju", (sp, client) =>
            {
                var tgju = sp.GetRequiredService<IOptions<TgjuOptions>>().Value;
                client.BaseAddress = new Uri(tgju.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(tgju.TimeoutSeconds);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(tgju.UserAgent);
                client.DefaultRequestHeaders.Accept
                    .ParseAdd("application/json, text/javascript, */*; q=0.01");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "fa");
            });

            services.AddSingleton<TgjuApiClient>();
            return services;
        }
    }
}