using BrokerageOperations.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerageOperations.Application.Interface
{
    public interface IBrokerageOperationsService
    {
        BrokerageUserDto _user { get; }
        Task SetUser(BrokerageUserDto user);
        /// <summary>
        /// اطمینان از حضور در صفحه سهم
        /// شامل لاگین کردن و سرچ کردن و بقیه موارد مورد نیاز که با توجه به سامانه یا کارگزاری پیاده میشوند
        /// </summary>
        /// <param name="stock">مشخصات مورد نیاز سهم یا آپشن</param>
        /// <returns></returns>
        Task EnsureStockPage(StockDto stock);
        /// <summary>
        /// اطمینان از حضور در صفحه سفارشات
        /// شامل لاگین کردن و سرچ کردن و بقیه موارد مورد نیاز که با توجه به سامانه یا کارگزاری پیاده میشوند
        /// </summary>
        /// <returns></returns>
        Task EnsureOrderPage();
        /// <summary>
        /// اطمینان از حضور در صفحه گردش حساب
        /// شامل لاگین کردن و سرچ کردن و بقیه موارد مورد نیاز که با توجه به سامانه یا کارگزاری پیاده میشوند
        /// </summary>
        /// <returns></returns>
        Task EnsureTurnoverPage();

        /// <summary>
        /// گرفتن اسنپ شات از وضعیت معاملات سهم که شامل تمام مراحل مورد نیاز با توجه به سامانه خواهد بود
        /// </summary>
        /// <returns></returns>
        Task<SnapShotDto> GetSnapShotFromTrade(StockDto stock);

        /// <summary>
        /// دریافت لیست سفارشات
        /// </summary>
        /// <param name="from">از تاریخ</param>
        /// <param name="to">تا تاریخ</param>
        /// <param name="stock">سهم</param>
        /// <returns></returns>
        Task<IEnumerable<OrderDto>> GetOrders(DateTime? from = null, DateTime? to = null, StockDto? stock = null);

        /// <summary>
        /// دریافت گردش حساب
        /// </summary>
        /// <param name="from">از تاریخ</param>
        /// <param name="to">تا تاریخ</param>
        /// <param name="description">شرح</param>
        /// <returns></returns>
        Task<IEnumerable<TurnoverDto>> GetTurnover(DateTime? from = null, DateTime? to = null, string? description = null);
        /// <summary>
        /// ثبت سفارش خرید
        /// </summary>
        /// <param name="stock">سهم</param>
        /// <param name="price">قیمت</param>
        /// <param name="quantity">حجم</param>
        /// <returns></returns>
        Task Buy(StockDto stock, int price, int quantity);
        /// <summary>
        /// ثبت سفارش فروش
        /// </summary>
        /// <param name="stock">سهم</param>
        /// <param name="price">قیمت</param>
        /// <param name="quantity">حجم</param>
        /// <returns></returns>
        Task Sell(StockDto stock, int price, int quantity);
        /// <summary>
        /// لغو سفارش
        /// </summary>
        /// <param name="stock">سهم - در صورت خالی بودن همه سهم ها</param>
        /// <param name="price">قیمت - در صورت خالی بودن همه قیمت ها</param>
        /// <param name="quantity">حجم - در صورت خالی بودن همه حجم ها</param>
        /// <returns></returns>
        Task CancelOrders(StockDto? stock = null, int? price = null, int? quantity = null);
    }
}
