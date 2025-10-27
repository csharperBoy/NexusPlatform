using Core.Domain.Interfaces;
using Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Domain.Entities
{
    /// <summary>
    /// اطلاعات قابل تغییر افراد - با هر تغییر یک رکورد جدید ایجاد می‌شود
    /// مثل: تعداد فرزندان، آدرس، وضعیت تأهل
    /// </summary>
    public class PersonProfile : IAggregateRoot, IEntity<Guid>
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public Guid FkPersonId { get; private set; }

        public string? Address { get; private set; }
        public string? JobTitle { get; private set; }
        public string? EducationLevel { get; private set; }
        public short? PersonalCode { get; private set; }

        public MaritalStatus MaritalStatus { get; private set; }

        public DateTime? DateOfHire { get; private set; }
        public DateTime? DateOfMarried { get; private set; }

        public short? MemberQty { get; private set; }
        public short? DependantsQty { get; private set; }
        public short? NumberOfChildren { get; private set; }
        public short? StudentChildCount { get; private set; }

        public string? ZipCode { get; private set; }

        // ✅ جایگزینی با ValueObjectها
        public PhoneNumber? Phone { get; private set; }
        public PhoneNumber? Mobile { get; private set; }
        public Email? Email { get; private set; }

        public string? Descriptions { get; private set; }
        public byte[]? Image { get; private set; }

        public bool? Enablity { get; private set; }
        public bool? Visiblity { get; private set; }
        public bool? Remove { get; private set; }

        public string? PasswordEmergency { get; private set; }
        public Guid? FkchildId { get; private set; }

        // Audit
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string CreatedBy { get; private set; } = string.Empty;

        // Navigation
        public virtual Person Person { get; private set; } = null!;

        // Constructor for EF
        protected PersonProfile() { }

        public PersonProfile(Guid personId, short? numberOfChildren, string? address,
                             MaritalStatus maritalStatus, string? jobTitle,
                             string? educationLevel, string createdBy,
                             PhoneNumber? phone = null, PhoneNumber? mobile = null, Email? email = null)
        {
            FkPersonId = personId;
            NumberOfChildren = numberOfChildren;
            Address = address;
            MaritalStatus = maritalStatus;
            JobTitle = jobTitle;
            EducationLevel = educationLevel;
            CreatedBy = createdBy;
            Phone = phone;
            Mobile = mobile;
            Email = email;
        }
    }

    public enum MaritalStatus
    {
        Single = 1,
        Married = 2,
        Divorced = 3,
        Widowed = 4
    }
}