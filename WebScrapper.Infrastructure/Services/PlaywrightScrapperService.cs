using Azure;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Auditing;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.Security;
using Core.Infrastructure.Repositories;
using Core.Shared.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebScrapper.Application.DTOs;
using WebScrapper.Application.Interfaces;
using WebScrapper.Domain.Enums;
using Microsoft.Playwright;
using WebScrapper.Domain.Common;

namespace WebScrapper.Infrastructure.Services
{
    public class PlaywrightScrapperService : IWebScrapperServicee
       
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _page;
        private IEnumerable<PlaywrightWindowDto> _windows;
        public PlaywrightScrapperService(
         IPlaywright playwright,
         IBrowser browser,
         IBrowserContext context,
         IPage page)
        {
            _playwright = playwright;
            _browser = browser;
            _context = context;
            _page = page;
            _windows = new List<PlaywrightWindowDto>();
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

                throw;
            }
        }

        public async Task NewTabPage<TPage, TWindow>(string url , TPage page , TWindow? window) 
            where TPage : IPageContract
            where TWindow : IWindowContract<TPage>
        {
            _page = await _context.NewPageAsync();
            await _page.GotoAsync(url);
        }
       
        public async Task Wait(int millisecond)
        {
            //await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(millisecond);
        }
        public async Task<string> GetCurrentUrl()
            where TPage : IPageContract
            where TWindow : IWindowContract<TPage>
        {
            //await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            return _page.Url;
        }
        public async Task WaitForLoad(ElementAccessPath elementPath)
            where TPage : IPageContract
            where TWindow : IWindowContract<TPage>
        {
            //await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Task.Delay(2000);
        }
        public async Task<bool> ElementIsExist(ElementAccessPath elementPath)
            
        {
            try
            {

                var element = await FindElement(elementPath);

                if (element == null)
                    return false;
                else
                    return true;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public Task NewWindow<TWindow>(string url, TWindow window) where TWindow : IWindowContract<IPageContract>
        {
            throw new NotImplementedException();
        }

        public async Task Fill(ElementAccessPath elementPath, string value)
        {
            //await _page.FillAsync(elementPath.FullXpath, value);
            var element = await FindElement(elementPath);
            await element.FillAsync(value);
        }

        public async Task Click(ElementAccessPath elementPath)
        { 
            //await _page.ClickAsync(elementPath.FullXpath);
            var element = await FindElement(elementPath);
            await element.ClickAsync();
        }

        public async Task<string> InnerText(ElementAccessPath elementPath)
        {
            try
            {

                var element = await FindElement(elementPath);

                return await element.InnerTextAsync();

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// یافتن المنت
        /// </summary>
        /// <param name="elementPath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<IElement> FindElement(ElementAccessPath elementPath)
           
        {
            IElement element;
            switch (elementPath.DefaultAccessPath)
            {
                case ElementPathEnum.FullXpath:
                    element = await _page.QuerySelectorAsync(elementPath.FullXpath);
                    break;
                case ElementPathEnum.SelectorXpath:
                    element = await _page.QuerySelectorAsync(elementPath.SelectorXpath);
                    break;
                case ElementPathEnum.JSpath:
                    element = await _page.QuerySelectorAsync(elementPath.JSpath);
                    break;
                case ElementPathEnum.localXpath:
                    element = await _page.QuerySelectorAsync(elementPath.localXpath);
                    break;
                default:
                    element = await _page.QuerySelectorAsync(elementPath.FullXpath);
                    break;
            }
            if (element == null)
            {
                throw new Exception("Element not found!!!");
            }
            return element;
        }
    }
}
