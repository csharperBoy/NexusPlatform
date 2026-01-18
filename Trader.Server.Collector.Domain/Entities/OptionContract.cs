using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Server.Collector.Domain.Entities
{
    /// <summary>
    /// اطلاعات مربوط به قرار داد های اختیار معامله
    /// </summary>
    [SecuredResource("Collector.OptionContract")]
    public class OptionContract : DataScopedAndResourcedEntity, IAggregateRoot
    {
        /// <summary>
        /// شناسه
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// تاریخ سررسید
        /// </summary>
        public DateOnly DueDate { get; set; }
        /// <summary>
        /// کلید خارجی به سهام پایه
        /// </summary>
        public Guid FkStockId { get; set; }
        /// <summary>
        /// تعداد موقعیت باز مجاز
        /// </summary>
        public int NumberOfOpenPositionAllow { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
