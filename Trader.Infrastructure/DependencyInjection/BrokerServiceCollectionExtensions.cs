using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Commands;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.DependencyInjection
{
    public static class BrokerServiceCollectionExtensions
    {
        public static IServiceCollection AddBroker<TClient>(
            this IServiceCollection services,
            BrokerType brokerType)
            where TClient : class, IBrokerClient
        {
            // concrete — یک instance در هر scope
            services.AddScoped<TClient>();

            // همه‌ی interfaceها به همون instance اشاره می‌کنن
            services.AddKeyedScoped<IBrokerClient>(brokerType,
                (sp, _) => sp.GetRequiredService<TClient>());

            services.AddKeyedScoped<IBrokerSessionManager>(brokerType,
                (sp, _) => sp.GetRequiredService<TClient>());

            services.AddKeyedScoped<IBrokerTradingCommands>(brokerType,
                (sp, _) => sp.GetRequiredService<TClient>());

            services.AddKeyedScoped<IBrokerFinanceCommands>(brokerType,
                (sp, _) => sp.GetRequiredService<TClient>());

            services.AddKeyedScoped<IBrokerMarketDataReader>(brokerType,
                (sp, _) => sp.GetRequiredService<TClient>());

            services.AddKeyedScoped<IBrokerAccountReader>(brokerType,
                (sp, _) => sp.GetRequiredService<TClient>());

            services.AddKeyedScoped<IBrokerOrderHistoryReader>(brokerType,
                (sp, _) => sp.GetRequiredService<TClient>());

            services.AddKeyedScoped<IBrokerAnalysisReader>(brokerType,
                (sp, _) => sp.GetRequiredService<TClient>());

            services.AddKeyedScoped<IBrokerWatchlistReader>(brokerType,
                (sp, _) => sp.GetRequiredService<TClient>());

            return services;
        }
    }

}
