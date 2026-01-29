using BrokerageOperations.Application.Interface;
using Core.Application.Abstractions;
using Core.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Server.Collector.Application.Commands;
using Trader.Server.Collector.Application.Interface;
using Trader.Server.Collector.Domain.Entities;
using Trader.Server.Collector.Domain.Specifications;
using Trader.Server.Collector.Infrastructure.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Trader.Server.Collector.Infrastructure.Service
{
    public class StockService : IStockService
    {
        private readonly ILogger<StockService> _logger;
        private readonly IRepository<TraderDbContext, Stock, Guid> _repository;
        private readonly ISpecificationRepository<Stock, Guid> _specRepository;
        private readonly IUnitOfWork<TraderDbContext> _uow;
        public StockService(ILogger<StockService> logger, IRepository<TraderDbContext, Stock, Guid> repository, ISpecificationRepository<Stock, Guid> specRepository, IUnitOfWork<TraderDbContext> uow)
        {
            _uow = uow;
            _repository = repository;
            _specRepository = specRepository;
            _logger = logger;
        }

        public async Task<Guid> AddStock(AddStockCommand command)
        {
            try
            {
                var existing = await GetStockByISIN(command.Isin);
                if (existing != null)
                    throw new ArgumentException($"stock with Isin '{command.Isin}' already exists");



                var stock = new Stock(
                    command.Isin, command.Title, command.BuyCommissionRate, command.SellCommissionRate, command.TypeOfMarket, command.PreOpeningTimeStart, command.PreOpeningTimeEnd, command.OpenTime, command.CloseTime, command.TPlus, command.MinValueBuyOrder, command.MinValueSellOrder, command.StepPrice, command.IsActive, command.CodeOfTsetmc, command.ReleaseDate, command.PercentOfDailyTolerance
                );

                await _repository.AddAsync(stock);
                //stock.AddDomainEvent(new StockAddEvent(stock.Id));

                await _uow.SaveChangesAsync();
                //await InvalidateResourceCachesAsync();

                return stock.Id;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<Stock> GetStockByISIN(string ISIN)
        {
            try
            {
                var spec = new StockByIsinSpec(ISIN);
                Stock resource = await _specRepository.GetBySpecAsync(spec);
                return resource;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
