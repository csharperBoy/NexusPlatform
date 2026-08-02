using Core.Domain.Common.EntityProperties;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    public class PostContact : BaseEntity, IAuditableEntity, IOwnerableEntity,IHasEffectivePeriod
    {
        #region IAuditableEntity Impelement
        public void Touch() => ModifiedAt = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        #region IOwnerableEntity Impelement
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

        #region Impelement IHasEffectivePeriod
        public DateTime? EffectiveFrom { get; private set; }
        public DateTime? EffectiveTo { get; private set; }
        public bool IsCurrent { get; private set; }

        public void SetEffectiveFrom(DateTime? value)
        {
            EffectiveFrom = value;
            Touch();
        }

        public async Task SetEffectiveTo(DateTime? value)
        {
            EffectiveTo = value;
            Touch();
            await Task.CompletedTask;
        }
        public async Task SetIsCurrent(bool value)
        {
            IsCurrent = value;
            Touch();
            await Task.CompletedTask;
        }
        #endregion



        public HrContactType ContactType { get; protected set; }
        public string Value { get; protected set; }
        public Guid FkPostId { get; private set; }

        public virtual Post Post { get; private set; } = null!;
        // Constructor for EF
        protected PostContact() { }
        public PostContact
            (HrContactType _ContactType,
            string _Value,
            Guid _PostId,
            DateTime? _EffectiveFrom = null,
            DateTime? _EffectiveTo =null,
            bool _isCurrent = true
            )
        {
            ContactType = _ContactType;
            Value = _Value;
            FkPostId = _PostId;
            EffectiveFrom = _EffectiveFrom;
            EffectiveTo = _EffectiveTo;
            IsCurrent = _isCurrent;
        }
    }
}
