using BrokerageOperations.Application.Interface;
using BrokerageOperations.Domain.Property;
using BrokerageOperations.Shared.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapper.Application.Interfaces;

namespace BrokerageOperations.Infrastructure.Service
{
    public class EasyTraderService : IBrokerageOperationsService
    {
        private IWebScrapperServicee _scrapper;
        private ILogger<EasyTraderService> _logger;
        //private EasyTraderProperties _easyTraderProperties;
        public EasyTraderService(IWebScrapperServicee scrapper, ILogger<EasyTraderService> logger)
        {
            _logger = logger;
            _scrapper = scrapper;

            _scrapper.InitializeAsync().Wait();
            _scrapper.GoToUrl(EasyTraderProperties.LoginUrl).Wait();
        }
        public BrokerageUserDto _user { get; protected set; }
        public Task SetUser(BrokerageUserDto user)
        {
            try
            {
                _user = user;
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"set user error in EasyTraderService - userName = {user.UserName}");
                throw;
            }
        }
        public async Task Buy(StockDto stock, int price, int quantity)
        {
            try
            {
                await EnsureStockPage(stock);
                await _scrapper.Click(EasyTraderProperties.BuyButton);
                await _scrapper.Fill(EasyTraderProperties.VolumeInput, quantity.ToString());
                await _scrapper.Fill(EasyTraderProperties.PriceInput, price.ToString());
                await _scrapper.Click(EasyTraderProperties.SendBuyOrderButton);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Buy error in EasyTraderService - stockTitle= {stock.Title} - price = {price} - quantity = {quantity} ");
                throw;
            }
        }
        public async Task Sell(StockDto stock, int price, int quantity)
        {
            try
            {
                await EnsureStockPage(stock);
                await _scrapper.Click(EasyTraderProperties.SellButton);
                await _scrapper.Fill(EasyTraderProperties.VolumeInput, quantity.ToString());
                await _scrapper.Fill(EasyTraderProperties.PriceInput, price.ToString());
                await _scrapper.Click(EasyTraderProperties.SendSellOrderButton);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sell error in EasyTraderService - stockTitle= {stock.Title} - price = {price} - quantity = {quantity} ");
                throw;
            }
        }
        public Task CancelOrders(StockDto? stock = null, int? price = null, int? quantity = null)
        {
            try
            {

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Task<IEnumerable<OrderDto>> GetOrders(DateTime? from = null, DateTime? to = null, StockDto? stock = null)
        {
            throw new NotImplementedException();
        }

        public Task<SnapShotDto> GetSnapShotFromTrade(StockDto stock)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TurnoverDto>> GetTurnover(DateTime? from = null, DateTime? to = null, string? description = null)
        {
            throw new NotImplementedException();
        }



        public async Task EnsureStockPage(StockDto stock)
        {
            throw new NotImplementedException();
        }

        public async Task EnsureOrderPage()
        {
            throw new NotImplementedException();
        }

        public async Task EnsureTurnoverPage()
        {
            throw new NotImplementedException();
        }
    }
}
