using Contact.Domain.Enums;
using Core.Domain.Common.EntityProperties;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Contact.Domain.Entities
{
    public class ContactProfile : BaseEntity, IAuditableEntity, IOwnerableEntity
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

        public void DeActive()
        {
            IsActive = false;
        }


        #endregion


        public string Title { get; private set; }

        public bool IsActive { get; private set; }
        public ContactProfileTypeEnum ProfileType { get; private set; }
        public ICollection<ContactItem> ContactItems { get; private set; } = new List<ContactItem>();

        // Constructor for EF
        protected ContactProfile() { }
        public ContactProfile(string _Title , ContactProfileTypeEnum _ProfileType, bool _IsActive = true)
        {
            Title = _Title;
            ProfileType = _ProfileType;
            IsActive = _IsActive;
        }
    }
}
