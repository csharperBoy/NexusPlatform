using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderServer.Domain.Entities;

namespace TraderServer.Domain.Specifications
{
    public class StockByIsinSpec : BaseSpecification<Stock>
    {
        public StockByIsinSpec(string ISIN)
            : base(r => r.Isin == ISIN)
        {
           
        }
    }
}
