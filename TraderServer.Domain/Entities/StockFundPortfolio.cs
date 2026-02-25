using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderServer.Domain.Entities
{


    [SecuredResource("Trader.StockFundPortfolio")]
    public class StockFundPortfolio : BaseEntity, IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
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
