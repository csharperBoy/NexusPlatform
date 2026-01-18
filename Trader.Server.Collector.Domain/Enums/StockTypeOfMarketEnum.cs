using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Server.Collector.Domain.Enums
{
    public enum StockTypeOfMarketEnum
    {
        [Description("بازار بورس")]
        burs = 1, 
        [Description("بازار فرابورس")]
        faraburs = 2,
        [Description("صندوق طلا")]
        gold = 3,
        [Description("صندوق سهامی")]
        sandogh = 4
    }

}
