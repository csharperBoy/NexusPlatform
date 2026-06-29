using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Domain.Entities
{
    /// <summary>
    /// اطلاعات قابل تغییر افراد - با هر تغییر یک رکورد جدید ایجاد می‌شود
    /// مثل: تعداد فرزندان، آدرس، وضعیت تأهل
    /// </summary>
    public class NaturalPersonProfile : BaseEntity, IAuditableEntity, IOwnerableEntity
    {
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





        public Guid FkNaturalPersonId { get; private set; }

        //public MaritalStatus MaritalStatus { get; private set; }

        //public DateTime? DateOfMarried { get; private set; }

        //public short? MemberQty { get; private set; }
        //public short? DependantsQty { get; private set; }
        //public short? NumberOfChildren { get; private set; }
        //public short? StudentChildCount { get; private set; }

        public DateOnly EffectiveFrom { get; private set; }
        public DateOnly? EffectiveTo { get; private set; }
        public bool? IsCurrent { get; private set; }
        public bool? Enablity { get; private set; }
        public bool? Visiblity { get; private set; }
        public bool? Remove { get; private set; }

        //public Guid? FkchildId { get; private set; }

        // Navigation

        public virtual NaturalPerson FkNaturalPerson { get; private set; } = null!;
        // Constructor for EF
        protected NaturalPersonProfile() { }
        public NaturalPersonProfile(
            Guid _FkPersonId
            )
        {
            FkNaturalPersonId = _FkPersonId;
            IsCurrent = true;
            Enablity = true;
            Visiblity = true;
            Remove = false;
            EffectiveFrom =DateOnly.FromDateTime( DateTime.Now);
            
        }
       
    }

    //public enum MaritalStatus
    //{
    //    Single = 1,
    //    Married = 2,
    //    Divorced = 3,
    //    Widowed = 4
    //}
}