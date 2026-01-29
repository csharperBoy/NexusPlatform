using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebScrapper.Application.DTOs;
using WebScrapper.Domain.Common;
using WebScrapper.Domain.Enums;
using static System.Net.Mime.MediaTypeNames;
namespace WebScrapper.Application.Interfaces
{


    public interface IWebScrapperServicee<TElement> where TElement : class
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
        Task Fill(ElementAccessPath elementPath, string value, TElement? BaseElement = null);

        /// <summary>
        /// کلیک روی المنت
        /// </summary>
        /// <param name="elementPath">دسترسی به المنت</param>
        /// <returns></returns>
        Task Click(ElementAccessPath elementPath, TElement? BaseElement = null);
        /// <summary>
        ///دریافت متن داخل المنت
        /// </summary>
        /// <param name="elementPath">دسترسی به المنت</param>
        /// <returns>متن داخل المنت</returns>
        Task<string> InnerText(ElementAccessPath elementPath, TElement? BaseElement = null);
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
        Task WaitForLoad(ElementAccessPath elementPath, TElement? BaseElement = null);
        /// <summary>
        /// بررسی وجود المنت در صفحه
        /// </summary>
        /// <param name="elementPath">دسترسی به المنت</param>
        /// <returns>وجود یا عدم وجود المنت</returns>
        Task<bool> ElementIsExist(ElementAccessPath elementPath, TElement? BaseElement = null);
        /// <summary>
        /// آماده سازی بدلیل اینکه در سازنده نمیتوان async استفاده کرد
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync();



        #region عملیات های مربوط به جداول

        /// <summary>
        /// دریافت رکورد های داخل یک جدول بصورت ساختار تعریف شده
        ///  باید مشخصات دسترسی به جدول بصورت تو در تو و مطابق استاندارد تعریف شده برای جداول باشد
        /// </summary>
        /// <param name="tableElementPath">مشخصات دسترسی به جدول</param>
        /// <param name="filterValues">فیلتر برای اعمال روی نتیجه</param>
        /// <returns></returns>
        Task<TableDto> Table_GetTableContent(TableElementAccessPath tableElementPath, TableRowDto? filterValues = null);
        /// <summary>
        ///  کلیک بر روی یک دکمه داخل ردیف های جدول
        ///  باید مشخصات دسترسی به جدول بصورت تو در تو و مطابق استاندارد تعریف شده برای جداول باشد
        /// </summary>
        /// <param name="tableElementPath"> مشخصات دسترسی به المنت جدول</param>
        /// <param name="buttonColumnKey"> کلید تعریف شده در پراپرتی های دسترسی به جدول مربوط به همان دکمه مورد نظر که میخواهیم بر رویش کلیک شود</param>
        /// <param name="filterValues">شروطی که باید مطابق بر اون بر روی دکمه هر ردیف کلیک شود</param>
        /// <returns></returns>
        Task Table_ClickOnTableSubElement(TableElementAccessPath tableElementPath, string buttonColumnKey, TableRowDto? filterValues = null);
        /// <summary>
        /// دریافت اطلاعات یک ردیف خاص از یک جدول
        /// باید برای فراخوانی این متد از طریق TableElementAccessPath باید با متد مخصوص داخلش اطلاعات دسترسی اون ردیف مورد نظر ساخته بشه
        /// </summary>
        /// <param name="tableRowElementPath">اطلاعات دسترسی به ردیف مورد نظر جدول</param>
        /// <returns></returns>
        Task<TableRowDto> Table_GetTableRowContent(TableRowElementAccessPath tableRowElementPath);
        /// <summary>
        /// کلیک روی دکمه خاص از ردیف مشخص جدول
        /// باید برای فراخوانی این متد از طریق TableElementAccessPath باید با متد مخصوص داخلش اطلاعات دسترسی اون ردیف مورد نظر ساخته بشه
        /// </summary>
        /// <param name="tableRowElementPath">اطلاعات دسترسی به ردیف مورد نظر جدول</param>
        /// <param name="buttonColumnKey"> کلید تعریف شده در پراپرتی های دسترسی به جدول مربوط به همان دکمه مورد نظر که میخواهیم بر رویش کلیک شود</param>
        /// <returns></returns>
        Task Table_ClickOnTableRowSubElement(TableRowElementAccessPath tableRowElementPath, string buttonColumnKey);
        /// <summary>
        /// دریافت تعداد رکورد های داخل یک جدول
        /// </summary>
        /// <param name="tableElementPath">اطلاعات دسترسی به جدول</param>
        /// <returns></returns>
        Task<int> Table_GetTableRowCount(TableElementAccessPath tableElementPath);

        Task<List<MasterDetailTableDto>> Table_GetMasterDetailContent(TableElementAccessPath masterTablePath, TableElementAccessPath detailTablePath);
        #endregion
    }
}
