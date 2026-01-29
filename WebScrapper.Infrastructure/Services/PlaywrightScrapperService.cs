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
using WebScrapper.Infrastructure.Common;

namespace WebScrapper.Infrastructure.Services
{
    public class PlaywrightScrapperService : IWebScrapperServicee<IElementHandle>

    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _page;
        private IEnumerable<PlaywrightWindow> _windows;
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
            _windows = new List<PlaywrightWindow>();
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
        public async Task WaitForLoad(ElementAccessPath elementPath, IElementHandle? BaseElement = null)
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
        public async Task<bool> ElementIsExist(ElementAccessPath elementPath, IElementHandle? BaseElement = null)
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

        public async Task Fill(ElementAccessPath elementPath, string value, IElementHandle? BaseElement = null)
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

        public async Task Click(ElementAccessPath elementPath, IElementHandle? BaseElement = null)
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
        public async Task<string> InnerText(ElementAccessPath elementPath, IElementHandle? BaseElement = null)
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
                IEnumerable<IElementHandle> rows =await FindElements(rowPath, table);
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
                IEnumerable<IElementHandle> rows =await FindElements(rowPath, table);
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
                IElementHandle row = await FindElement(tableRowElementPath);

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
                IElementHandle row = await FindElement(tableRowElementPath);
                //foreach (var row in rows)
               // {
                    TableRowDto tableRow = new TableRowDto();

                    foreach (var columnPath in tableRowElementPath.columnsAccessPath.Where(c => c.ElementType == ElementTypeEnum.Button))
                    {
                        if (columnPath.Code == buttonColumnKey)
                        {
                            await Click(columnPath, row);
                        }
                 //   }

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
                IEnumerable<IElementHandle> rows =await FindElements(rowPath, table);
                
                return rows.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Table_GetTableRowCount error in PlaywrightScrapperService - element = {tableElementPath.Title} - page = {tableElementPath.pageCode} - window = {tableElementPath.windowCode}");

                throw;
            }
        }


        public async Task<List<MasterDetailTableDto>> Table_GetMasterDetailContent(
    TableElementAccessPath masterTablePath,
    TableElementAccessPath detailTablePath)
        {
            try
            {
                var result = new List<MasterDetailTableDto>();

                // پیدا کردن جدول اصلی
                var masterTable = await FindElement(masterTablePath);
                var masterRowPath = masterTablePath.rowAccessPath;
                var masterRows = await FindElements(masterRowPath, masterTable);

                foreach (var masterRow in masterRows)
                {
                    var masterRowData = new TableRowDto();
                    var masterDetailData = new MasterDetailTableDto();

                    // خواندن اطلاعات ردیف اصلی
                    foreach (var columnPath in masterRowPath.columnsAccessPath
                        .Where(c => c.ElementType == ElementTypeEnum.TableColumn))
                    {
                        var column = new TableColumnDto
                        {
                            value = await InnerText(columnPath, masterRow),
                            key = columnPath.Code
                        };
                        masterRowData.columns.Add(column);
                    }

                    masterDetailData.MasterRow = masterRowData;

                    // بررسی آیا ردیف قابل گسترش است
                    if (await IsRowExpandable(masterRow))
                    {
                        // اگر بسته است، بازش کن
                        if (!await IsRowExpanded(masterRow))
                        {
                            await ClickExpandButton(masterRow);
                            await Wait(1500); // صبر برای بارگذاری کامل جزئیات
                        }

                        // حالا ردیف جزئیات را پیدا کن
                        // ردیف جزئیات معمولاً sibling بعدی است
                        var detailRow = await masterRow.EvaluateHandleAsync(@"element => {
                    let sibling = element.nextElementSibling;
                    while(sibling && !sibling.classList.contains('ag-details-row')) {
                        sibling = sibling.nextElementSibling;
                    }
                    return sibling;
                }");

                        if (detailRow != null && detailRow.ToString() != "null")
                        {
                            var detailRowElement = detailRow as IElementHandle;
                            if (detailRowElement != null)
                            {
                                // حالا جدول جزئیات را از داخل ردیف جزئیات پیدا کن
                                var detailGrid = await FindElement(detailTablePath, detailRowElement);
                                if (detailGrid != null)
                                {
                                    // خواندن ردیف‌های جدول جزئیات
                                    var detailRows = await FindElements(detailTablePath.rowAccessPath, detailGrid);

                                    foreach (var subRow in detailRows)
                                    {
                                        var detailRowData = new TableRowDto();

                                        foreach (var columnPath in detailTablePath.rowAccessPath.columnsAccessPath
                                            .Where(c => c.ElementType == ElementTypeEnum.TableColumn))
                                        {
                                            var column = new TableColumnDto
                                            {
                                                value = await InnerText(columnPath, subRow),
                                                key = columnPath.Code
                                            };
                                            detailRowData.columns.Add(column);
                                        }

                                        masterDetailData.DetailRows.Add(detailRowData);
                                    }
                                }
                            }
                        }
                    }

                    result.Add(masterDetailData);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Table_GetMasterDetailContent error");
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
        private async Task<IElementHandle> FindElement(ElementAccessPath elementPath, IElementHandle? BaseElement = null)
        {
            try
            {
                IElementHandle? element;
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

                    throw new Exception("Elementnot found!!!");
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
        private async Task<IEnumerable<IElementHandle>> FindElements(ElementAccessPath elementPath, IElementHandle? BaseElement = null)
        {
            try
            {
                IEnumerable<IElementHandle> element;
                switch (elementPath.DefaultAccessPath)
                {
                    case ElementPathEnum.FullXpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAllAsync(elementPath.FullXpath);
                        else
                            element = await BaseElement.QuerySelectorAllAsync(elementPath.FullXpath);

                        break;
                    case ElementPathEnum.SelectorXpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAllAsync(elementPath.SelectorXpath);
                        else
                            element = await BaseElement.QuerySelectorAllAsync(elementPath.SelectorXpath);

                        break;
                    case ElementPathEnum.JSpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAllAsync(elementPath.JSpath);
                        else
                            element = await BaseElement.QuerySelectorAllAsync(elementPath.JSpath);

                        break;
                    //case ElementPathEnum.localXpath:
                    //    element = await _page.QuerySelectorAsync(elementPath.localXpath);
                    //    break;
                    case ElementPathEnum.Xpath:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAllAsync(elementPath.Xpath);
                        else
                            element = await BaseElement.QuerySelectorAllAsync(elementPath.Xpath);

                        break;
                    default:
                        if (BaseElement == null)
                            element = await _page.QuerySelectorAllAsync(elementPath.FullXpath);
                        else
                            element = await BaseElement.QuerySelectorAllAsync(elementPath.FullXpath);

                        break;
                }
                if (element == null)
                {

                    throw new Exception("IElementHandle not found!!!");
                }
                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FindElement error in PlaywrightScrapperService - element = {elementPath.Title} - page = {elementPath.pageCode} - window = {elementPath.windowCode}");

                throw;
            }
        }

        private async Task<bool> IsRowExpandable(IElementHandle row)
        {
            try
            {
                // بررسی وجود کلاس ag-row-group که نشان‌دهنده قابلیت گسترش است
                var classAttribute = await row.GetAttributeAsync("class");
                return classAttribute.Contains("ag-row-group");
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> IsRowExpanded(IElementHandle row)
        {
            try
            {
                var classAttribute = await row.GetAttributeAsync("class");
                return classAttribute.Contains("ag-row-group-expanded");
            }
            catch
            {
                return false;
            }
        }

        private async Task ClickExpandButton(IElementHandle row)
        {
            try
            {
                // پیدا کردن آیکون باز کردن (معمولاً در سلول اول)
                var expandButton = await row.QuerySelectorAsync(".ag-group-contracted");
                if (expandButton != null)
                {
                    await expandButton.ClickAsync();
                    await Wait(1000); // صبر برای باز شدن
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clicking expand button");
            }
        }
        #endregion

    }
}
