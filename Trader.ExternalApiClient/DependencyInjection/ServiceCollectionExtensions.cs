using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trader.ExternalApiClient.EasyTrader;
using Trader.ExternalApiClient.Tgju;
using Trader.ExternalApiClient.Tsetmc;

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


            // ─── TSETMC ───
            services.Configure<TsetmcOptions>(
                configuration.GetSection(TsetmcOptions.SectionName));

            services.AddHttpClient("Tsetmc", (sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<TsetmcOptions>>().Value;

                client.BaseAddress = new Uri(opts.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);

                client.DefaultRequestHeaders.UserAgent.ParseAdd(opts.UserAgent);
                client.DefaultRequestHeaders.Accept
                    .ParseAdd("application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "Accept-Language", opts.AcceptLanguage);
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "Referer", opts.Referer);
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "Cache-Control", "no-cache");
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    "Pragma", "no-cache");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                // نگه‌داشتن connection برای پرفورمنس بالا (این endpointها حجم زیاد برمی‌گردانند)
                PooledConnectionLifetime = TimeSpan.FromSeconds(30),
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(20),
                MaxConnectionsPerServer = 20,
                AutomaticDecompression = System.Net.DecompressionMethods.All,
                ConnectTimeout = TimeSpan.FromSeconds(10),
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 3,
            });

            services.AddSingleton<TsetmcApiClient>();



            return services;
        }
    }
}