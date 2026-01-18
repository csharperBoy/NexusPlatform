using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Server.Collector.Domain.Enums;

namespace Trader.Server.Collector.Domain.Entities
{
    /// <summary>
    /// اطلاعات مربوط به سهم در اون وجود داره
    /// </summary>
    [SecuredResource("Collector.Stock")]
    public class Stock : DataScopedAndResourcedEntity, IAggregateRoot
    {
        /// <summary>
        /// شناسه
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// کد
        /// </summary>
        public string Isin { get; set; } = null!;
        /// <summary>
        /// عنوان نماد
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// نوع بازار
        /// </summary>
        public StockTypeOfMarketEnum? TypeOfMarket { get; set; }
        /// <summary>
        /// ساعت شروع پیش گشایش
        /// </summary>
        public TimeOnly? PreOpeningTimeStart { get; set; }
        /// <summary>
        /// ساعت پایان پیش گشایش
        /// </summary>
        public TimeOnly? PreOpeningTimeEnd { get; set; }
        /// <summary>
        /// ساعت آغاز معاملات
        /// </summary>
        public TimeOnly? OpenTime { get; set; }
        /// <summary>
        /// ساعت پایان معاملات
        /// </summary>
        public TimeOnly? CloseTime { get; set; }
        /// <summary>
        /// T تسویه در بازار بورس
        /// </summary>
        public int? TPlus { get; set; }
        /// <summary>
        ///درصد کارمزد خرید 
        /// </summary>
        public decimal? BuyCommissionRate { get; set; }
        /// <summary>
        /// درصد کارمزد فروش
        /// </summary>
        public decimal? SellCommissionRate { get; set; }
        /// <summary>
        /// حداقل ارزش مجاز خرید به ریال
        /// </summary>
        public int? MinValueBuyOrder { get; set; }
        /// <summary>
        /// حداقل ارزش مجاز فروش به ریال
        /// </summary>
        public int? MinValueSellOrder { get; set; }
        /// <summary>
        /// میزان هر پله افزایش یا کاهش قیمت ( 1 ریال یا 10 ریال)
        /// </summary>
        public int? StepPrice { get; set; }
        /// <summary>
        /// فعال یا غیر فعال بودن
        /// </summary>
        public bool? IsActive { get; set; }
        /// <summary>
        /// کد منحصر به فرد سهم در سامانه Tsetmc
        /// </summary>
        public string? CodeOfTsetmc { get; set; }
        /// <summary>
        /// تاریخ افتتاح سهم
        /// </summary>
        public DateOnly? ReleaseDate { get; set; }
        /// <summary>
        /// بازه قیمتی مجاز روزانه (مثلا +5 و -5 درصد)
        /// </summary>
        public int PercentOfDailyTolerance { get; set; }

    }

}
