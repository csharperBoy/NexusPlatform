using Core.Domain.Common.EntityProperties;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.People;
using People.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Domain.Entities
{
    public class PartyContact : BaseEntity , IAuditableEntity , IOwnerableEntity , IHasEffectivePeriod
    {
        #region Impelement IHasEffectivePeriod
        public DateOnly EffectiveFrom { get; private set; }
        public DateOnly? EffectiveTo { get; private set; }
        public bool IsCurrent { get; private set; }

        #endregion
        #region IAuditableEntity Impelement
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





        public PartyContactType ContactType { get; protected set; }
        public string Value { get; protected set; }
        public Guid FkPartyId { get; private set; }

        public virtual Party Party { get; private set; } = null!;
        // Constructor for EF
        protected PartyContact() { }
        public PartyContact
            (PartyContactType _ContactType,
            string _Value,
            Guid _PartyId)
        {
            ContactType = _ContactType;
            Value = _Value;
            FkPartyId = _PartyId;
        }

    }
}
