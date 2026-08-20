using Contact.Domain.Enums;
using Core.Domain.Common.EntityProperties;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Entities
{
    public class ContactResource : BaseEntity, IAuditableEntity, IOwnerableEntity
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


        public ContactTypeEnum ContactType { get; private set; }
        /// <summary>
        /// مقدار راه ارتباطی (شماره تلفن، آدرس ایمیل، آیدی اینستاگرام، لینک و...)
        /// </summary>
        public string Value { get; protected set; }
        /// <summary>
        /// عنوان اختصاصی آیتم (مثلاً: "موبایل شخصی"، "واتس‌اپ کاری"، "ایمیل پشتیبانی")
        /// </summary>
        public string? Label { get; private set; }

        // فیلدهای اولویت و وضعیت
        public bool IsPrimary { get; private set; } // آیا کانال اصلی این نوع است؟
        public int? SortOrder { get; private set; } // ترتیب نمایش در UI

        /// <summary>
        /// اشاره به یک ContactItem دیگر (مثلاً متصل بودن این شماره به شماره همگانی/اصلی)
        /// </summary>
        public Guid? ParentContactResourceId { get; private set; }
        public ContactResource ParentContactResource { get; private set; }

        public ContactRelationTypeEnum? RelationType { get; private set; }
        public ICollection<ContactProfileAssignment> Assignments { get; private set; } = new List<ContactProfileAssignment>();
        public ICollection<ContactResource> ChildContactResources { get; private set; } = new List<ContactResource>();
        //public virtual Employment Employment { get; private set; } = null!;
        // Constructor for EF
        protected ContactResource() { }
        public ContactResource
            (ContactTypeEnum _ContactType,
            string _Value,
            DateTime? _EffectiveFrom = null,
            string? _Label = null, 
            bool _IsPrimary = true, 
            int? _SortOrder = null, 
            Guid? _ParentContactItemId = null, 
            ContactRelationTypeEnum? _RelationType = null,
            DateTime? _EffectiveTo = null,
            bool _isCurrent = true
            )
        {
            ContactType = _ContactType;
            Value = _Value;
            Label= _Label;
            IsPrimary = _IsPrimary;
            SortOrder = _SortOrder;
            ParentContactItemId = _ParentContactItemId;
            RelationType = _RelationType;
        }
    }
}
