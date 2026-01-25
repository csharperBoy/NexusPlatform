using Azure;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Auditing;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.Security;
using Core.Infrastructure.Repositories;
using Core.Shared.Results;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebScrapper.Application.DTOs;
using WebScrapper.Application.Interfaces;
using WebScrapper.Domain.Common;
using WebScrapper.Domain.Enums;

namespace WebScrapper.Infrastructure.Services
{
    public class PlaywrightScrapperService : IWebScrapperServicee<Element>

    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _page;
        private IEnumerable<PlaywrightWindowDto> _windows;
        private ILogger<PlaywrightScrapperService> _logger;
        public PlaywrightScrapperService(
         IPlaywright playwright,
         IBrowser browser,
         IBrowserContext context,
         IPage page,
         ILogger<PlaywrightScrapperService> logger)
        {
            _playwright = playwright;
            _browser = browser;
            _context = context;
            _page = page;
            _windows = new List<PlaywrightWindowDto>();
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _playwright = await Playwright.CreateAsync();

                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false,
                    Args = new[] { "--start-maximized" }
                });

                // ایجاد context با viewport بزرگ
                //var context = await _browser.NewContextAsync(new BrowserNewContextOptions
                //{
                //    ViewportSize = ViewportSize.NoViewport // غیرفعال کردن viewport ثابت
                //});
                _context = await _browser.NewContextAsync(new BrowserNewContextOptions
                {
                    ViewportSize = ViewportSize.NoViewport // غیرفعال کردن viewport ثابت
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"InitializeAsync error in PlaywrightScrapperService");

                throw;
            }
        }

        public async Task NewTabPage<TPage, TWindow>(string url, TPage page, TWindow? window)
            where TPage : IPageContract
            where TWindow : IWindowContract<TPage>
        {
            try
            {
                _page = await _context.NewPageAsync();
                await _page.GotoAsync(url);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"NewTabPage error in PlaywrightScrapperService - url={url} - page = {page.Title} - window = {window?.title}");

                throw;
            }
        }
        public Task NewWindow<TWindow>(string url, TWindow window) where TWindow : IWindowContract<IPageContract>
        {
            try
            {
                throw new NotImplementedException();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"NewWindow error in PlaywrightScrapperService - url={url} - window = {window?.title}");

                throw;
            }
        }
        public Task GoToUrl(string url, string windowCode = "default", string pageCode = "default")
        {
            throw new NotImplementedException();
        }

        public async Task Wait(int millisecond)
        {
            try
            {

                //await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(millisecond);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Wait error in PlaywrightScrapperService - millisecond={millisecond}");

                throw;
            }
        }
        public async Task<string> GetCurrentUrl(string windowCode = "default", string pageCode = "default")
        {
            try
            {

                //await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                return _page.Url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetCurrentUrl error in PlaywrightScrapperService - windowCode={windowCode} - pageCode = {pageCode}");

                throw;
            }
        }
        public async Task WaitForLoad(ElementAccessPath elementPath, Element? BaseElement = null)
        {
            try
            {

                //await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"WaitForLoad error in PlaywrightScrapperService - element = {elementPath.Title} - page = {elementPath.pageCode} - window = {elementPath.windowCode}");

                throw;
            }
        }
        public async Task<bool> ElementIsExist(ElementAccessPath elementPath, Element? BaseElement = null)
        {
            try
            {

                var element = await FindElement(elementPath, BaseElement);

                if (element == null)
                    return false;
                else
                    return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ElementIsExist error in PlaywrightScrapperService - element = {elementPath.Title} - page = {elementPath.pageCode} - window = {elementPath.windowCode}");

                throw;
            }
        }

        public async Task Fill(ElementAccessPath elementPath, string value, Element? BaseElement = null)
        {
            try
            {

                //await _page.FillAsync(elementPath.FullXpath, value);
                var element = await FindElement(elementPath, BaseElement);
                await element.FillAsync(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fill error in PlaywrightScrapperService - element = {elementPath.Title} - page = {elementPath.pageCode} - window = {elementPath.windowCode}");

                throw;
            }
        }

        public async Task Click(ElementAccessPath elementPath, Element? BaseElement = null)
        {
            try
            {

                //await _page.ClickAsync(elementPath.FullXpath);
                var element = await FindElement(elementPath, BaseElement);
                await element.ClickAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Click error in PlaywrightScrapperService - element = {elementPath.Title} - page = {elementPath.pageCode} - window = {elementPath.windowCode}");

                throw;
            }
        }
        public async Task<string> InnerText(ElementAccessPath elementPath, Element? BaseElement = null)
        {
            try
            {

                var element = await FindElement(elementPath, BaseElement);

                return await element.InnerTextAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"InnerText error in PlaywrightScrapperService - element = {elementPath.Title} - page = {elementPath.pageCode} - window = {elementPath.windowCode}");

                throw;
            }
        }
        
        #region عملیات های مربوط به جداول

        public async Task<TableDto> Table_GetTableContent(TableElementAccessPath tableElementPath, TableRowDto? filterValues = null)
        {
            try
            {
                TableDto tableContent = new TableDto();
                var table = await FindElement(tableElementPath);
                var rowPath = tableElementPath.rowAccessPath;
                var rows = FindElements(rowPath, table);
                foreach (var row in rows)
                {
                    TableRowDto tableRow = new TableRowDto();
                    foreach (var columnPath in rowPath.columnsAccessPath.Where(c => c.ElementType == ElementTypeEnum.TableColumn))
                    {
                        TableColumnDto column = new TableColumnDto();
                        column.value = await InnerText(columnPath, row);
                        column.key = columnPath.Code;
                        tableRow.columns.Add(column);
                    }
                    if (filterValues == null || !tableRow.columns.Any(r => filterValues.columns.Any(f => f.key == r.key && f.value != r.value)))
                    {
                        tableContent.rows.Add(tableRow);
                    }
                }
                return tableContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetTableContent error in PlaywrightScrapperService - element = {tableElementPath.Title} - page = {tableElementPath.pageCode} - window = {tableElementPath.windowCode}");

                throw;
            }
        }
        public async Task Table_ClickOnTableSubElement(TableElementAccessPath tableElementPath, string buttonColumnKey, TableRowDto? filterValues = null)
        {
            try
            {
                var table = await FindElement(tableElementPath);
                var rowPath = tableElementPath.rowAccessPath;
                var rows = FindElements(rowPath, table);
                foreach (var row in rows)
                {
                    TableRowDto tableRow = new TableRowDto();
                    if (filterValues != null)
                    {

                        foreach (var columnPath in rowPath.columnsAccessPath.Where(c => c.ElementType == ElementTypeEnum.TableColumn))
                        {
                            TableColumnDto column = new TableColumnDto();
                            column.value = await InnerText(columnPath);
                            column.key = columnPath.Code;
                            tableRow.columns.Add(column);
                        }
                    }
                    if (filterValues == null || !tableRow.columns.Any(r => filterValues.columns.Any(f => f.key == r.key && f.value != r.value)))
                    {
                        /// اگر همه شروط برقرار باشد
                        foreach (var columnPath in rowPath.columnsAccessPath.Where(c => c.ElementType == ElementTypeEnum.Button))
                        {
                            if (columnPath.Code == buttonColumnKey)
                            {
                                await Click(columnPath, row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetTableContent error in PlaywrightScrapperService - element = {tableElementPath.Title} - page = {tableElementPath.pageCode} - window = {tableElementPath.windowCode}");

                throw;
            }
        }
      
        public async Task<TableRowDto> Table_GetTableRowContent(TableRowElementAccessPath tableRowElementPath)
        {
            try
            {
                TableRowDto rowContent = new TableRowDto();
                var row = FindElement(tableRowElementPath);

                foreach (var columnPath in tableRowElementPath.columnsAccessPath.Where(c => c.ElementType == ElementTypeEnum.TableColumn))
                {
                    TableColumnDto column = new TableColumnDto();
                    column.value = await InnerText(columnPath, row);
                    column.key = columnPath.Code;
                    rowContent.columns.Add(column);
                }

                return rowContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetTableRowContent error in PlaywrightScrapperService - element = {tableRowElementPath.Title} - page = {tableRowElementPath.pageCode} - window = {tableRowElementPath.windowCode}");

                throw;
            }
        }
        public async Task Table_ClickOnTableRowSubElement(TableRowElementAccessPath tableRowElementPath, string buttonColumnKey)
        {
            try
            {
                var rows = FindElement(tableRowElementPath);
                foreach (var row in rows)
                {
                    TableRowDto tableRow = new TableRowDto();

                    foreach (var columnPath in tableRowElementPath.columnsAccessPath.Where(c => c.ElementType == ElementTypeEnum.Button))
                    {
                        if (columnPath.Code == buttonColumnKey)
                        {
                            await Click(columnPath, row);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ClickOnTableRowSubElement error in PlaywrightScrapperService - element = {tableRowElementPath.Title} - page = {tableRowElementPath.pageCode} - window = {tableRowElementPath.windowCode}");

                throw;
            }
        }
        public async Task<int> Table_GetTableRowCount(TableElementAccessPath tableElementPath)
        {
            try
            {
                var table = await FindElement(tableElementPath);
                var rowPath = tableElementPath.rowAccessPath;
                var rows = FindElements(rowPath, table);
                
                return rows.count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Table_GetTableRowCount error in PlaywrightScrapperService - element = {tableElementPath.Title} - page = {tableElementPath.pageCode} - window = {tableElementPath.windowCode}");

                throw;
            }
        }

        #endregion

        #region اختصاصی همین کلاس

        /// <summary>
        /// یافتن المنت
        /// </summary>
        /// <param name="elementPath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<Element> FindElement(ElementAccessPath elementPath, Element? BaseElement = null)
        {
            try
            {
                Element element;
                switch (elementPath.DefaultAccessPath)
                {
                    case ElementPathEnum.FullXpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAsync(elementPath.FullXpath);
                        else
                            element = await BaseElement.QuerySelectorAsync(elementPath.FullXpath);

                        break;
                    case ElementPathEnum.SelectorXpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAsync(elementPath.SelectorXpath);
                        else
                            element = await BaseElement.QuerySelectorAsync(elementPath.SelectorXpath);

                        break;
                    case ElementPathEnum.JSpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAsync(elementPath.JSpath);
                        else
                            element = await BaseElement.QuerySelectorAsync(elementPath.JSpath);

                        break;
                    //case ElementPathEnum.localXpath:
                    //    element = await _page.QuerySelectorAsync(elementPath.localXpath);
                    //    break;
                    case ElementPathEnum.Xpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAsync(elementPath.Xpath);
                        else
                            element = await BaseElement.QuerySelectorAsync(elementPath.Xpath);

                        break;
                    default:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAsync(elementPath.FullXpath);
                        else
                            element = await BaseElement.QuerySelectorAsync(elementPath.FullXpath);

                        break;
                }
                if (element == null)
                {

                    throw new Exception("Element not found!!!");
                }
                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FindElement error in PlaywrightScrapperService - element = {elementPath.Title} - page = {elementPath.pageCode} - window = {elementPath.windowCode}");

                throw;
            }
        }

        /// <summary>
        /// یافتن المنت
        /// </summary>
        /// <param name="elementPath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<IEnumerable<Element>> FindElements(ElementAccessPath elementPath, Element? BaseElement = null)
        {
            try
            {
                IEnumerable<Element> element;
                switch (elementPath.DefaultAccessPath)
                {
                    case ElementPathEnum.FullXpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAsync(elementPath.FullXpath);
                        else
                            element = await BaseElement.QuerySelectorAsync(elementPath.FullXpath);

                        break;
                    case ElementPathEnum.SelectorXpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAsync(elementPath.SelectorXpath);
                        else
                            element = await BaseElement.QuerySelectorAsync(elementPath.SelectorXpath);

                        break;
                    case ElementPathEnum.JSpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAsync(elementPath.JSpath);
                        else
                            element = await BaseElement.QuerySelectorAsync(elementPath.JSpath);

                        break;
                    //case ElementPathEnum.localXpath:
                    //    element = await _page.QuerySelectorAsync(elementPath.localXpath);
                    //    break;
                    case ElementPathEnum.Xpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAsync(elementPath.Xpath);
                        else
                            element = await BaseElement.QuerySelectorAsync(elementPath.Xpath);

                        break;
                    default:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAsync(elementPath.FullXpath);
                        else
                            element = await BaseElement.QuerySelectorAsync(elementPath.FullXpath);

                        break;
                }
                if (element == null)
                {

                    throw new Exception("Element not found!!!");
                }
                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FindElement error in PlaywrightScrapperService - element = {elementPath.Title} - page = {elementPath.pageCode} - window = {elementPath.windowCode}");

                throw;
            }
        }
        #endregion

    }
}
