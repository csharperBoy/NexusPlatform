using Microsoft.Extensions.DependencyInjection;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Commands;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Domain.Enums;
using Trader.ExternalApiClient.EasyTrader;
using Trader.Infrastructure.Brokers.EasyTrader.Capabilities;

namespace Trader.Infrastructure.Brokers.EasyTrader
{
    public static class EasyTraderBrokerRegistration
    {
        public static IServiceCollection AddEasyTraderBroker(
            this IServiceCollection services)
        {
            const BrokerType type = BrokerType.EasyTrader;

            // زیرساخت مشترک
            services.AddHttpClient("EasyTrader", c =>
            {
                c.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddSingleton<EasyTraderApiClient>();

            // Capabilities — هر کدوم مستقل
            services.AddKeyedScoped<IBrokerSessionManager, EasyTraderSessionManager>(type);
            services.AddKeyedScoped<IBrokerTradingCommands, EasyTraderTradingCommands>(type);
            services.AddKeyedScoped<IBrokerFinanceCommands, EasyTraderFinanceCommands>(type);
            services.AddKeyedScoped<IBrokerMarketDataReader, EasyTraderMarketDataReader>(type);
            services.AddKeyedScoped<IBrokerAccountReader, EasyTraderAccountReader>(type);
            services.AddKeyedScoped<IBrokerOrderHistoryReader, EasyTraderOrderHistoryReader>(type);
            services.AddKeyedScoped<IBrokerAnalysisReader, EasyTraderAnalysisReader>(type);
            services.AddKeyedScoped<IBrokerWatchlistReader, EasyTraderWatchlistReader>(type);

            return services;
        }
    }
}