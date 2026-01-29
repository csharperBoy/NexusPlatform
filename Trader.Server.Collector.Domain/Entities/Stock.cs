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
        public Stock(
             string _Isin,
             string? _Title,
             decimal? _BuyCommissionRate,
             decimal? _SellCommissionRate,
             StockTypeOfMarketEnum? _TypeOfMarket = null,
             TimeOnly? _PreOpeningTimeStart = null,
             TimeOnly? _PreOpeningTimeEnd = null,
             TimeOnly? _OpenTime = null,
             TimeOnly? _CloseTime = null,
             int? _TPlus = null,
             int? _MinValueBuyOrder = null,
             int? _MinValueSellOrder = null,
             int? _StepPrice = null,
             bool? _IsActive = null,
             string? _CodeOfTsetmc = null,
             DateOnly? _ReleaseDate = null,
             double? _PercentOfDailyTolerance = null)
        {

            if (_PreOpeningTimeStart == null)
                PreOpeningTimeStart = new TimeOnly(8, 45);
            else
                PreOpeningTimeStart = _PreOpeningTimeStart;

            if (_PreOpeningTimeEnd == null)
                PreOpeningTimeEnd = new TimeOnly(9);
            else
                PreOpeningTimeEnd = _PreOpeningTimeEnd;

            if (_OpenTime == null)
                OpenTime = new TimeOnly(9);
            else
                OpenTime = _OpenTime;

            if (_CloseTime == null)
                CloseTime = new TimeOnly(12, 30);
            else
                CloseTime = _CloseTime;

            Isin = _Isin;
            Title = _Title;

            if (_TypeOfMarket == null)
                TypeOfMarket = StockTypeOfMarketEnum.burs;
            else
                TypeOfMarket = _TypeOfMarket;


            if (_TPlus == null)
                TPlus = 1;
            else
                TPlus = _TPlus;

            if (_BuyCommissionRate == null)
                BuyCommissionRate = 0.01m;
            else
                BuyCommissionRate = _BuyCommissionRate;

            if (_SellCommissionRate == null)
                SellCommissionRate = 0.01m;
            else
                SellCommissionRate = _SellCommissionRate;

            if (_MinValueBuyOrder == null)
                MinValueBuyOrder = 5000000;
            else
                MinValueBuyOrder = _MinValueBuyOrder;

            if (_MinValueSellOrder == null)
                MinValueSellOrder = 5000000;
            else
                MinValueSellOrder = _MinValueSellOrder;

            if (_StepPrice == null)
                StepPrice = 1;
            else
                StepPrice = _StepPrice;

            if (_IsActive == null)
                IsActive = true;
            else
                IsActive = _IsActive;


            if (_PercentOfDailyTolerance == null)
                PercentOfDailyTolerance = 0.03;
            else
                PercentOfDailyTolerance = _PercentOfDailyTolerance;

            CodeOfTsetmc = _CodeOfTsetmc;

            ReleaseDate = _ReleaseDate;
        }
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
        public double PercentOfDailyTolerance { get; set; }

    }

}
