using Core.Domain.Attributes;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
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
    public class Option : AuditableEntity , IDataScopedEntity, IAggregateRoot
    {
        #region IDataScopedEntity Impelement
        public Guid? OwnerOrganizationUnitId { get; protected set; }
        public Guid? OwnerPositionId { get; protected set; }
        public Guid? OwnerPersonId { get; protected set; }
        public Guid? OwnerUserId { get; protected set; }

        public void SetOwners(Guid? userId, Guid? personId, Guid? positiontId, Guid? orgUnitId)
        {
            OwnerUserId = userId;
            OwnerPersonId = personId;
            OwnerPositionId = positiontId;
            OwnerOrganizationUnitId = orgUnitId;
        }
        public void SetPersonOwner(Guid personId)
        {
            OwnerPersonId = personId;
        }
        public void SetUserOwner(Guid userId)
        {
            OwnerUserId = userId;
        }
        public void SetPositionOwner(Guid positiontId)
        {
            OwnerPositionId = positiontId;
        }
        public void SetOrganizationUnitOwner(Guid orgUnitId)
        {
            OwnerOrganizationUnitId = orgUnitId;
        }
        #endregion

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
