using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerageOperations.Shared.DTOs
{
    public class StockDto
    {
        public string ISIN { get; set; }
        public string Title { get; set; }
        public string CodeOfTsetmc { get; set; }
        public StockType stockType { get; set; } = StockType.sahm;
    }

    public enum StockType
    {
        [Description("سهم")]
        sahm = 1,
        [Description("آپشن")]
        Option = 2,
        [Description("صندوق سهامی")]
        sandogh = 3
    }
}
