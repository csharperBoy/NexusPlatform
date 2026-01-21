using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapper.Application.DTOs;
using WebScrapper.Domain.Common;
using static System.Net.Mime.MediaTypeNames;
namespace WebScrapper.Application.Interfaces
{


    public interface IWebScrapperServicee
    {


        /// <summary>
        /// باز کردن تب جدید
        /// </summary>
        /// <param name="url"> آدرس صفحه</param>
        /// <returns></returns>
        Task NewTabPage<TPage, TWindow>(string url, TPage page, TWindow? window)
             where TPage : IPageContract
            where TWindow : IWindowContract<TPage>;
            
        /// <summary>
        /// باز کردن پنجره جدید
        /// </summary>
        /// <param name="url"> آدرس صفحه</param>
        /// <returns></returns>
        Task NewWindow<TWindow>(string url, TWindow window)
            where TWindow : IWindowContract<IPageContract>;
        /// <summary>
        /// رفتن به آدرس
        /// </summary>
        /// <param name="url">آدرس</param>
        /// <param name="windowCode">کد پنجره</param>
        /// <param name="pageCode">کد پیج</param>
        /// <returns></returns>
        Task GoToUrl(string url, string windowCode = "default", string pageCode = "default");
        /// <summary>
        /// پر کردن فیلد
        /// </summary>
        /// <param name="elementPath">دسترسی به المنت</param>
        /// <param name="value">مقدار</param>
        /// <returns></returns>
        Task Fill(ElementAccessPath elementPath, string value);
             
        /// <summary>
        /// کلیک روی المنت
        /// </summary>
        /// <param name="elementPath">دسترسی به المنت</param>
        /// <returns></returns>
        Task Click(ElementAccessPath elementPath);
        /// <summary>
        ///دریافت متن داخل المنت
        /// </summary>
        /// <param name="elementPath">دسترسی به المنت</param>
        /// <returns>متن داخل المنت</returns>
        Task<string> InnerText(ElementAccessPath elementPath);
        /// <summary>
        /// تعویق به میلی ثانیه
        /// </summary>
        /// <param name="millisecond">میلی ثانیه</param>
        /// <returns></returns>
        Task Wait(int millisecond);
        /// <summary>
        /// دریافت آدرس فعلی
        /// </summary>
        /// <returns>آدرس url</returns>
        Task<string> GetCurrentUrl(string windowCode = "default", string pageCode = "default");
        /// <summary>
        /// تعلیق تا لود المنت
        /// </summary>
        /// <param name="elementPath">دسترسی به المنت</param>
        /// <returns></returns>
        Task WaitForLoad(ElementAccessPath elementPath);
        /// <summary>
        /// بررسی وجود المنت در صفحه
        /// </summary>
        /// <param name="elementPath">دسترسی به المنت</param>
        /// <returns>وجود یا عدم وجود المنت</returns>
        Task<bool> ElementIsExist(ElementAccessPath elementPath);
        /// <summary>
        /// آماده سازی بدلیل اینکه در سازنده نمیتوان async استفاده کرد
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync();
        /// <summary>
        /// دریافت رکورد های داخل یک جدول بصورت json
        /// </summary>
        /// <param name="elementPath"></param>
        /// <returns></returns>
        Task<List<string>> GetTableElementRows(ElementAccessPath elementPath);
    }
}
