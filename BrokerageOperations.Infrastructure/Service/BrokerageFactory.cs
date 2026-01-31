using BrokerageOperations.Application.Interface;
using BrokerageOperations.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapper.Application.Interfaces;

namespace BrokerageOperations.Infrastructure.Service
{
    public class BrokerageFactory : IBrokerageFactory
    {
        private readonly IServiceScopeFactory _serviceProvider;
        public BrokerageFactory(IServiceScopeFactory serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public  IBrokerageOperationsService CreateBrokerageService(BrokeragePlatformEnum platform)
        {
            try
            {


                using (var scope = _serviceProvider.CreateScope())
                {
                    switch (platform)
                    {
                        case BrokeragePlatformEnum.EasyTrader:
                            var webScrapper = scope.ServiceProvider.GetRequiredService<IWebScrapperServicee<IElementHandle>>();
                            var logger = scope.ServiceProvider.GetRequiredService<ILogger<EasyTraderService>>();
                            return new EasyTraderService(webScrapper , logger);
                            break;
                        case BrokeragePlatformEnum.Agah:
                            break;
                        default:
                            break;
                    }
                }
                throw new ArgumentException($"No strategy registered for {platform}");
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
