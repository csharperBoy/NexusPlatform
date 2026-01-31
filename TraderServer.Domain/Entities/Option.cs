using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderServer.Domain.Enums;

namespace TraderServer.Domain.Entities
{
    /// <summary>
    /// اطلاعات مربوط به اوراق اختیار خرید یا فروش سهم
    /// </summary>
    [SecuredResource("Trader.Option")]
    public class Option : DataScopedAndResourcedEntity, IAggregateRoot
    {
        /// <summary>
        /// کلید خارجی به قرارداد
        /// </summary>
        public Guid FkOptionContractId { get; set; }
        /// <summary>
        /// شناسه
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// عنوان
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// طرف معامله (خرید یا فروش)
        /// </summary>
        public OptionSideEnum Side { get; set; }
        /// <summary>
        /// قیمت اعمال
        /// </summary>
        public long DuePrice { get; set; }
        public bool IsActive { get; set; }
    }
}
