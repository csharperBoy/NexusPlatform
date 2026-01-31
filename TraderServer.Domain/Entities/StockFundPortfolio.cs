using Core.Domain.Attributes;
using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderServer.Domain.Entities
{


    [SecuredResource("Trader.StockFundPortfolio")]
    public class StockFundPortfolio : AuditableEntity
    {
        public Guid Id { get; set; }
        /// <summary>
        /// آیدی صندوق سهامی در جدول سهام
        /// </summary>
        public Guid StockFundId  { get; set; }
        /// <summary>
        /// آیدی سهام داخل پرتفو
        /// </summary>
        public Guid StocksInPortfolio { get; set; }
        /// <summary>
        /// درصد از کل پرتفو
        /// </summary>
        public decimal PercentOfPortfolio { get; set; }
    }
}
