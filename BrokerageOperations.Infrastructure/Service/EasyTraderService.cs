using BrokerageOperations.Application.Interface;
using BrokerageOperations.Domain.Property;
using BrokerageOperations.Shared.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebScrapper.Application.DTOs;
using WebScrapper.Application.Interfaces;
using WebScrapper.Domain.Enums;

namespace BrokerageOperations.Infrastructure.Service
{
    public class EasyTraderService : IBrokerageOperationsService
    {
        private IWebScrapperServicee<Element> _scrapper;
        private ILogger<EasyTraderService> _logger;
        //private EasyTraderProperties _easyTraderProperties;
        public EasyTraderService(IWebScrapperServicee<Element> scrapper, ILogger<EasyTraderService> logger)
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
        public async Task CancelOrders(StockDto? stock = null, int? price = null, int? quantity = null)
        {
            try
            {
                TableRowDto? filtere = null;
                if (stock != null || price != null || quantity != null)
                {
                    filtere = new TableRowDto();

                    if (stock != null)
                    {
                        filtere.columns.Add(
                            new TableColumnDto()
                            {
                                key = "StockTitle",
                                value = stock.Title
                            });
                    }
                    if (price != null)
                    {


                        filtere.columns.Add(
                            new TableColumnDto()
                            {
                                key = "VolumOrder",
                                value = quantity.ToString()
                            });
                    }
                    if (quantity != null)
                    {
                        filtere.columns.Add(
                            new TableColumnDto()
                            {
                                key = "Price",
                                value = price.ToString()
                            });
                    }
                }
                await _scrapper.Table_ClickOnTableSubElement(EasyTraderProperties.TodayOrdersTable, "Select", filtere);
                await _scrapper.Table_ClickOnTableSubElement(EasyTraderProperties.TodayOrdersTable, "delete", filtere);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CancelOrders error in EasyTraderService - stockTitle= {stock.Title} - price = {price} - quantity = {quantity} ");

                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrders(DateTime? from = null, DateTime? to = null, StockDto? stock = null)
        {
            try
            {
                /*
                TableRowDto? filtere = null;
                if (stock != null)
                {
                    
                    filtere = new TableRowDto();

                    if (stock != null)
                    {
                        filtere.columns.Add(
                            new TableColumnDto()
                            {
                                key = "StockTitle",
                                value = stock.Title
                            });
                    }
                    


                }
                */
                IEnumerable<OrderDto> orders = new List<OrderDto>();
                await EnsureOrderPage();
                if (stock != null)
                {
                    await _scrapper.Click(EasyTraderProperties.OrderHistoryStockTitleInput);
                    await _scrapper.Fill(EasyTraderProperties.OrderHistoryStockTitleInput, stock.Title);
                }
                if (from != null)
                {

                }
                if (to != null)
                {

                }
                TableDto tableData = await _scrapper.Table_GetTableContent(EasyTraderProperties.OrderHistoryTable);
                foreach (var row in tableData.rows)
                {
                    OrderDto order = new OrderDto();
                    DateOnly date = new DateOnly();
                    TimeOnly time = new TimeOnly();
                    foreach (var column in row.columns)
                    {
                        switch (column.key)
                        {
                            case "Date":
                                date = ConvertToDateOnly( column.value);
                                break;
                            case "Time":
                                time = TimeOnly.Parse(column.value);
                                break;
                            case "Side":
                                order.OrderSide = column.value.Trim() == "فروش" ? Shared.Enums.OrderSideEnum.sell : Shared.Enums.OrderSideEnum.buy;
                                break;
                            case "StockTitle":
                                order.StockTitle = column.value.Trim();
                                break;
                            case "OrderVolum":
                                order.BaseOrderQuantity = StringToInt( column.value.Trim());
                                break;
                            case "Price":
                                order.PriceOfUnit = StringToInt(column.value.Trim());
                                break;
                            case "DoneVolume":
                                order.DoneOrderQuantity = StringToInt(column.value.Trim());
                                break;
                            default:
                                break;
                        }


                    }
                    order.DateTime = date.ToDateTime(time);
                }
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetOrders error in EasyTraderService - stockTitle= {stock.Title} - from = {from} - to = {to} ");

                throw;
            }
        }

        private int StringToInt(string v)
        {
            try
            {
                return int.Parse(v.Replace(",", ""));
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private DateOnly ConvertToDateOnly(string value)
        {
            try
            {

                PersianCalendar pc = new PersianCalendar();
                string[] dateParts = value.Split('/');
                DateTime persianDate = pc.ToDateTime(
                    int.Parse(dateParts[0]),
                    int.Parse(dateParts[1]),
                    int.Parse(dateParts[2]),
                    0, 0, 0, 0
                );
                DateOnly datePart = DateOnly.FromDateTime(persianDate);
                return datePart;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetTodayOrders(StockDto? stock = null)
        {
            try
            {

                IEnumerable<OrderDto> orders = new List<OrderDto>();
                await EnsureOrderPage();
                if (stock != null)
                {
                    await _scrapper.Click(EasyTraderProperties.OrderHistoryStockTitleInput);
                    await _scrapper.Fill(EasyTraderProperties.OrderHistoryStockTitleInput, stock.Title);
                }
                if (from != null)
                {

                }
                if (to != null)
                {

                }
                await _scrapper.Table_GetTableContent(EasyTraderProperties.OrderHistoryTable);
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetOrders error in EasyTraderService - stockTitle= {stock.Title} - from = {from} - to = {to} ");

                throw;
            }
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

        public Task<List<SnapShotDto>> GetSnapShotFromMarketWatch(string MarketWatchTitle)
        {
            throw new NotImplementedException();
        }
    }
}
