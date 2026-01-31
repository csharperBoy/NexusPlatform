using BrokerageOperations.Application.Interface;
using BrokerageOperations.Shared.DTOs;
using Core.Application.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderServer.Application.DTO;
using TraderServer.Application.Interface;
using TraderServer.Domain.Entities;
using TraderServer.Infrastructure.DependencyInjection;

namespace TraderServer.Infrastructure.Service
{
    public class CollectorService : ICollectorService
    {
        private readonly IStockService _stockService;
        private readonly IRepository<TraderDbContext, SnapShotFromStockTrading, Guid> _repository;
        private readonly IUnitOfWork<TraderDbContext> _uow;
        private readonly ILogger<CollectorService> _logger;
        private IBrokerageOperationsService _brokerageService;
        private IBrokerageFactory _brokerageFactory;
        private BrokerageAccount _account;
        public CollectorService(ILogger<CollectorService> logger, /*IBrokerageOperationsService brokerageService,*/ IStockService stockService, IBrokerageFactory brokerageFactory)
        {
            _logger = logger;
            //_brokerageService = brokerageService;
            _stockService = stockService;
            _brokerageFactory = brokerageFactory;
            _brokerageService = _brokerageFactory.CreateBrokerageService(_account?.Platform ?? BrokerageOperations.Domain.Enums.BrokeragePlatformEnum.EasyTrader);
        }
        public async Task SetAccount(BrokerageAccount account)
        {
            _account = account;
        }
        public async Task SaveSnapShotStockTrading(SaveSnapShotRequest req)
        {
            try
            {
                Stock stock = await _stockService.GetStockByISIN(req.ISIN);
                StockDto stockReq = new StockDto()
                {
                    ISIN = stock.Isin,
                    Title = stock.Title,
                    CodeOfTsetmc = stock.CodeOfTsetmc

                };
                if (stock.TypeOfMarket == Domain.Enums.StockTypeOfMarketEnum.sandogh || stock.TypeOfMarket == Domain.Enums.StockTypeOfMarketEnum.gold)
                    stockReq.stockType = StockType.sandogh;
                else
                    stockReq.stockType = StockType.sahm;


                SnapShotDto snapShotTemp = await _brokerageService.GetSnapShotFromTrade(stockReq);
                if (snapShotTemp != null)
                {
                    SnapShotFromStockTrading snapShot = new SnapShotFromStockTrading()
                    {
                        BuyOrder1Count = snapShotTemp.BuyOrderCount[1],
                        BuyOrder1Price = snapShotTemp.BuyOrderPrice[1],
                        BuyOrder1Volume = snapShotTemp.BuyOrderVolume[1],
                        BuyOrder2Count = snapShotTemp.BuyOrderCount[2],
                        BuyOrder2Price = snapShotTemp.BuyOrderPrice[2],
                        BuyOrder2Volume = snapShotTemp.BuyOrderVolume[2],
                        BuyOrder3Count = snapShotTemp.BuyOrderCount[3],
                        BuyOrder3Price = snapShotTemp.BuyOrderPrice[3],
                        BuyOrder3Volume = snapShotTemp.BuyOrderVolume[3],
                        BuyOrder4Count = snapShotTemp.BuyOrderCount[4],
                        BuyOrder4Price = snapShotTemp.BuyOrderPrice[4],
                        BuyOrder4Volume = snapShotTemp.BuyOrderVolume[4],
                        BuyOrder5Count = snapShotTemp.BuyOrderCount[5],
                        BuyOrder5Price = snapShotTemp.BuyOrderPrice[5],
                        BuyOrder5Volume = snapShotTemp.BuyOrderVolume[5],
                        SellOrder1Count = snapShotTemp.SellOrderCount[1],
                        SellOrder1Price = snapShotTemp.SellOrderPrice[1],
                        SellOrder1Volume = snapShotTemp.SellOrderVolume[1],
                        SellOrder2Count = snapShotTemp.SellOrderCount[2],
                        SellOrder2Price = snapShotTemp.SellOrderPrice[2],
                        SellOrder2Volume = snapShotTemp.SellOrderVolume[2],
                        SellOrder3Count = snapShotTemp.SellOrderCount[3],
                        SellOrder3Price = snapShotTemp.SellOrderPrice[3],
                        SellOrder3Volume = snapShotTemp.SellOrderVolume[3],
                        SellOrder4Count = snapShotTemp.SellOrderCount[4],
                        SellOrder4Price = snapShotTemp.SellOrderPrice[4],
                        SellOrder4Volume = snapShotTemp.SellOrderVolume[4],
                        SellOrder5Count = snapShotTemp.SellOrderCount[5],
                        SellOrder5Price = snapShotTemp.SellOrderPrice[5],
                        SellOrder5Volume = snapShotTemp.SellOrderVolume[5],
                        ClosePrice = snapShotTemp.ClosePrice,
                        DateTime = snapShotTemp.DateTime,
                        FkStockId = stock.Id,
                        LastPrice = snapShotTemp.LastPrice,
                        MarketCapitalization = snapShotTemp.MarketCapitalization,
                        TotalBuyLegalPersonalityCount = snapShotTemp.TotalBuyLegalPersonalityCount,
                        TotalBuyLegalPersonalityVolume = snapShotTemp.TotalBuyLegalPersonalityVolume,
                        TotalBuyOrdersCount = snapShotTemp.TotalBuyOrderCount,
                        TotalBuyOrderVolume = snapShotTemp.TotalBuyOrderVolume,
                        TotalBuyTruePersonalityCount= snapShotTemp.TotalBuyTruePersonalityCount,
                        TotalBuyTruePersonalityVolume = snapShotTemp.TotalBuyTruePersonalityVolume,
                        TotalSellLegalPersonalityCount = snapShotTemp.TotalSellLegalPersonalityCount,
                        TotalSellLegalPersonalityVolume = snapShotTemp.TotalSellLegalPersonalityVolume,
                        TotalSellOrdersCount = snapShotTemp.TotalSellOrdersCount,
                        TotalSellOrderVolume = snapShotTemp.TotalSellOrderVolume,
                        TotalSellTruePersonalityCount = snapShotTemp.TotalSellTruePersonalityCount,
                        TotalSellTruePersonalityVolume = snapShotTemp.TotalSellTruePersonalityVolume,
                        TotalTradedValue = snapShotTemp.TotalTradedValue,
                        TotalTradedVolume = snapShotTemp.TotalTradedVolume,
                        TotalTradesCount = snapShotTemp.TotalTradesCount
                    };

                    await _repository.AddAsync(snapShot);
                    await _uow.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
