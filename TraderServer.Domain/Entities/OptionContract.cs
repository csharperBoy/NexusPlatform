using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderServer.Domain.Entities
{
    /// <summary>
    /// اطلاعات مربوط به قرار داد های اختیار معامله
    /// </summary>
    [SecuredResource("Trader.OptionContract")]
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
