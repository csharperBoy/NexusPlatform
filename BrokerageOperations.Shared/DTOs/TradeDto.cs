using BrokerageOperations.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerageOperations.Shared.DTOs
{
    public class TradeDto
    {
        /// <summary>
        /// تاریخ و ساعت معامله
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// طرف معامله - خرید یا فروش
        /// </summary>
        public OrderSideEnum OrderSide { get; set; }
        /// <summary>
        /// سهم
        /// </summary>
        public string StockTitle { get; set; }
        /// <summary>
        /// قیمت واحد
        /// </summary>
        public int PriceOfUnit { get; set; }
        /// <summary>
        /// حجم انجام شده سفارش
        /// </summary>
        public int Quantity { get; set; }
    }
}
