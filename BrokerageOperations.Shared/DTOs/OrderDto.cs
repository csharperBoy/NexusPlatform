using BrokerageOperations.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerageOperations.Shared.DTOs
{
    public class OrderDto
    {
        /// <summary>
        /// تاریخ و ساعت سفارش
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// طرف سفارش - خرید یا فروش
        /// </summary>
        public OrderSideEnum OrderSide { get; set; }
        /// <summary>
        /// سهم
        /// </summary>
        //public StockDto Stock { get; set; }
        public string StockTitle { get; set; }
        /// <summary>
        /// قیمت واحد
        /// </summary>
        public int PriceOfUnit { get; set; }
        /// <summary>
        /// حجم اولیه سفارش
        /// </summary>
        public int BaseOrderQuantity { get; set; }
        /// <summary>
        /// حجم انجام شده سفارش
        /// </summary>
        public int DoneOrderQuantity { get; set; }

    }
}
