using System;
using System.Collections.Generic;
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
    }
}
