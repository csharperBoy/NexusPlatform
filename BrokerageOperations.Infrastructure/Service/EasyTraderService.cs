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
using Microsoft.Playwright;
using Microsoft.AspNetCore.Identity.Data;

namespace BrokerageOperations.Infrastructure.Service
{
    public class EasyTraderService : IBrokerageOperationsService
    {
        private IWebScrapperServicee<IElementHandle> _scrapper;
        private ILogger<EasyTraderService> _logger;
        public BrokerageUserDto _user { get; protected set; }
        private EasyTraderProperties _easyTraderProperties;
        public EasyTraderService(IWebScrapperServicee<IElementHandle> scrapper, ILogger<EasyTraderService> logger)
        {
            _logger = logger;
            _scrapper = scrapper;
            _easyTraderProperties = new EasyTraderProperties();
            _scrapper.InitializeAsync().Wait();
        }
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
                await _scrapper.Click(_easyTraderProperties.BuyButton);
                await _scrapper.Fill(_easyTraderProperties.VolumeInput, quantity.ToString());
                await _scrapper.Fill(_easyTraderProperties.PriceInput, price.ToString());
                await _scrapper.Click(_easyTraderProperties.SendBuyOrderButton);
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
                await _scrapper.Click(_easyTraderProperties.SellButton);
                await _scrapper.Fill(_easyTraderProperties.VolumeInput, quantity.ToString());
                await _scrapper.Fill(_easyTraderProperties.PriceInput, price.ToString());
                await _scrapper.Click(_easyTraderProperties.SendSellOrderButton);
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
                await _scrapper.Table_ClickOnTableSubElement(_easyTraderProperties.TodayOrdersTable, "Select", filtere);
                await _scrapper.Table_ClickOnTableSubElement(_easyTraderProperties.TodayOrdersTable, "delete", filtere);
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
                    await _scrapper.Click(_easyTraderProperties.OrderHistoryStockTitleInput);
                    await _scrapper.Fill(_easyTraderProperties.OrderHistoryStockTitleInput, stock.Title);
                }
                if (from != null)
                {

                }
                if (to != null)
                {

                }
                TableDto tableData = await _scrapper.Table_GetTableContent(_easyTraderProperties.OrderHistoryTable);
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
                                date = ConvertToDateOnly(column.value);
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
                                order.BaseOrderQuantity = StringToInt(column.value.Trim());
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
                    await _scrapper.Click(_easyTraderProperties.OrderHistoryStockTitleInput);
                    await _scrapper.Fill(_easyTraderProperties.OrderHistoryStockTitleInput, stock.Title);
                }
                await _scrapper.Click(_easyTraderProperties.OrderHistoryFromDateInput);
                await _scrapper.Click(_easyTraderProperties.OrderHistoryFromDateSelectTodayButton);

                await _scrapper.Click(_easyTraderProperties.OrderHistoryToDateInput);
                await _scrapper.Click(_easyTraderProperties.OrderHistoryToDateSelectTodayButton);


                TableDto tableData = await _scrapper.Table_GetTableContent(_easyTraderProperties.OrderHistoryTable);
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
                                date = ConvertToDateOnly(column.value);
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
                                order.BaseOrderQuantity = StringToInt(column.value.Trim());
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
                _logger.LogError(ex, $"GetTodayOrders error in EasyTraderService - stockTitle= {stock.Title}  ");

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
                    await _scrapper.Click(_easyTraderProperties.OrderHistoryStockTitleInput);
                    await _scrapper.Fill(_easyTraderProperties.OrderHistoryStockTitleInput, stock.Title);
                }
                await _scrapper.Click(_easyTraderProperties.OrderHistoryFromDateInput);
                await _scrapper.Click(_easyTraderProperties.OrderHistoryFromDateSelectTodayButton);

                await _scrapper.Click(_easyTraderProperties.OrderHistoryToDateInput);
                await _scrapper.Click(_easyTraderProperties.OrderHistoryToDateSelectTodayButton);

                // همه سفارشات رو جداول جزئیاتش رو باز میکنه تا برای خواندن آماده بشوند
                await _scrapper.Table_ClickOnTableSubElement(_easyTraderProperties.OrderHistoryTable, "OpenSubOrders");
                TableDto tableData = await _scrapper.Table_GetTableContent(_easyTraderProperties.OrderHistoryDetailsTable);
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
                                date = ConvertToDateOnly(column.value);
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
                                order.BaseOrderQuantity = StringToInt(column.value.Trim());
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
                _logger.LogError(ex, $"GetTodayOrders error in EasyTraderService - stockTitle= {stock.Title}  ");

                throw;
            }
        }

        public async Task<SnapShotDto> GetSnapShotFromTrade(StockDto stock)
        {
            try
            {
                await EnsureStockPage(stock);
                SnapShotDto snapShot = new SnapShotDto();
                snapShot.TotalTradedValue = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalVolum));
                snapShot.DateTime = DateTime.Now;
                snapShot.stockTitle = await _scrapper.InnerText(_easyTraderProperties.StockTitle);
                snapShot.LastPrice = StringToInt(await _scrapper.InnerText(_easyTraderProperties.LastPrice));
                snapShot.ClosePrice = StringToInt(await _scrapper.InnerText(_easyTraderProperties.ClosePrice));
                snapShot.TotalTradedVolume = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalVolum));
                snapShot.ZeroPrice = StringToInt(await _scrapper.InnerText(_easyTraderProperties.ZeroPrice));


                snapShot = await GetTopFiveOrderTableContent(snapShot);

                snapShot.TotalBuyOrderCount = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalBuyOrderCount));
                snapShot.TotalSellOrdersCount = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalSellOrderCount));
                snapShot.TotalBuyOrderVolume = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalBuyOrderVolume));
                snapShot.TotalSellOrderVolume = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalSellOrderVolume));

                snapShot.TotalSellLegalPersonalityCount = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalSellLegalPersonalityCount));
                snapShot.TotalBuyLegalPersonalityCount = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalBuyLegalPersonalityCount));
                snapShot.TotalBuyTruePersonalityCount = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalBuyTruePersonalityCount));
                snapShot.TotalSellTruePersonalityCount = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalSellTruePersonalityCount));

                snapShot.TotalSellLegalPersonalityVolume = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalSellLegalPersonalityVolume));
                snapShot.TotalBuyLegalPersonalityVolume = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalBuyLegalPersonalityVolume));
                snapShot.TotalBuyTruePersonalityVolume = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalBuyTruePersonalityVolume));
                snapShot.TotalSellTruePersonalityVolume = StringToInt(await _scrapper.InnerText(_easyTraderProperties.TotalSellTruePersonalityVolume));

                if (stock.stockType == StockType.sandogh)
                    snapShot.NavPrice = StringToInt(await _scrapper.InnerText(_easyTraderProperties.NavPrice));

                if (stock.stockType == StockType.Option)
                {
                    snapShot.ContractOpenPosition = StringToInt(await _scrapper.InnerText(_easyTraderProperties.ContractOpenPosition));
                    snapShot.ContractOpenPositionGroup = StringToInt(await _scrapper.InnerText(_easyTraderProperties.ContractOpenPositionGroup));
                    snapShot.ContractTradesValue = StringToInt(await _scrapper.InnerText(_easyTraderProperties.ContractTradesValue));

                }
                return snapShot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetTodayOrders error in EasyTraderService - stockTitle= {stock.Title}  ");

                throw;
            }

        }


        /// <summary>
        /// دریافت محتوای جدول 5 سفارش برتر خرید و فروش در صفحه سهم و اضافه کردن به snapShot
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<SnapShotDto> GetTopFiveOrderTableContent(SnapShotDto snapShot)
        {
            TableDto tableData = await _scrapper.Table_GetTableContent(_easyTraderProperties.TopFiveOrderTable);
            for (int i = 1; i <= 5; i++)
            {
                foreach (var column in tableData.rows[i].columns)
                {
                    switch (column.key)
                    {
                        case "SellPrice":
                            snapShot.SellOrderPrice[i] = StringToInt(column.value.Trim());
                            break;
                        case "BuyPrice":
                            snapShot.BuyOrderPrice[i] = StringToInt(column.value.Trim());
                            break;
                        case "SellVolume":
                            snapShot.SellOrderVolume[i] = StringToInt(column.value.Trim());
                            break;
                        case "BuyVolume":
                            snapShot.BuyOrderVolume[i] = StringToInt(column.value.Trim());
                            break;
                        case "SellCount":
                            snapShot.SellOrderCount[i] = StringToInt(column.value.Trim());
                            break;
                        case "BuyCount":
                            snapShot.BuyOrderCount[i] = StringToInt(column.value.Trim());
                            break;
                        default:
                            break;
                    }
                }
            }
            return snapShot;
        }

        public Task<IEnumerable<TurnoverDto>> GetTurnover(DateTime? from = null, DateTime? to = null, string? description = null)
        {
            throw new NotImplementedException();
        }



        public async Task EnsureStockPage(StockDto stock)
        {
            try
            {
                while (!await _scrapper.ElementIsExist(_easyTraderProperties.StockTitle))
                {
                    while (!await _scrapper.ElementIsExist(_easyTraderProperties.SearchButton))
                    {
                        await Login();
                        await _scrapper.GoToUrl(_easyTraderProperties.BaseUrl);
                    }
                    await _scrapper.Click(_easyTraderProperties.SearchButton);
                    await _scrapper.Fill(_easyTraderProperties.SearchInput, stock.ISIN);
                    await _scrapper.Table_ClickOnTableRowSubElement(_easyTraderProperties.SearchResultTable.GetnumberOfRowFullXpath("1"), "StockSelect");

                }
                while (await _scrapper.InnerText(_easyTraderProperties.StockTitle) != stock.Title)
                {
                    await _scrapper.Click(_easyTraderProperties.SearchButton);
                    await _scrapper.Fill(_easyTraderProperties.SearchInput, stock.ISIN);
                    await _scrapper.Table_ClickOnTableRowSubElement(_easyTraderProperties.SearchResultTable.GetnumberOfRowFullXpath("1"), "StockSelect");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EnsureStockPage error in EasyTraderService - stockTitle= {stock.Title}  ");

                throw;
            }
        }

        private async Task Login()
        {
            try
            {
                await _scrapper.GoToUrl(_easyTraderProperties.LoginUrl);
                while (await IsLoginPage())
                {
                    await _scrapper.Fill(_easyTraderProperties.UserNameInput, _user.UserName);
                    await _scrapper.Fill(_easyTraderProperties.PasswordInput, _user.Password);
                    await _scrapper.Click(_easyTraderProperties.LoginButton);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login error in EasyTraderService - stockTitle= {_user.UserName}  ");

                throw;
            }
        }

        public async Task EnsureOrderPage()
        {
            try
            {
                while (await _scrapper.GetCurrentUrl() != _easyTraderProperties.OrderHistoryUrl)
                {
                    while (await IsLoginPage())
                    {
                        await Login();
                    }
                    await _scrapper.GoToUrl(_easyTraderProperties.OrderHistoryUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EnsureOrderPage error in EasyTraderService   ");

                throw;
            }
        }

        private async Task<bool> IsLoginPage()
        {
            try
            {
                if (await _scrapper.GetCurrentUrl() == _easyTraderProperties.LoginUrl)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {

                throw;
            }
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
