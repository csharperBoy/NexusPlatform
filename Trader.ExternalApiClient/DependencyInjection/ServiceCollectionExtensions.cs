
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
           
            /* ═══════════ Broker Clients ═══════════ */
            services.Configure<EasyTraderOptions>(
                configuration.GetSection(EasyTraderOptions.SectionName));

            /* ✅ HttpClient مشترک برای همه‌ی درخواست‌های EasyTrader (به‌جز login) */
            services.AddHttpClient("EasyTrader", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                MaxConnectionsPerServer = 20,
                AutomaticDecompression = System.Net.DecompressionMethods.All,
            });

            services.AddSingleton<EasyTraderApiClient>();


           
            return services;
        }
    }
}