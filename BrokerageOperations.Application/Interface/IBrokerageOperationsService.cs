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
        public BrokerageUserDto User { get; set; }
        /// <summary>
        /// باز کردن صفحه مربوط به سهم یا آپشن
        /// شامل لاگین کردن و سرچ کردن و بقیه موارد مورد نیاز که با توجه به سامانه یا کارگزاری پیاده میشوند
        /// </summary>
        /// <param name="stock">مشخصات مورد نیاز سهم یا آپشن</param>
        /// <returns></returns>
        //Task OpenStockPage(StockDto stock);

        /// <summary>
        /// گرفتن اسنپ شات از وضعیت معاملات سهم که شامل تمام مراحل مورد نیاز با توجه به سامانه خواهد بود
        /// </summary>
        /// <returns></returns>
        Task<SnapShotDto> GetSnapShotFromTrade(StockDto stock);

    }
}
