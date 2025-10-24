using Core.Domain.Interfaces;
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

        public Guid FkPersonId { get; set; }
        // اطلاعات قابل تغییر

        public string? Address { get; private set; }
        public string? JobTitle { get; private set; }
        public string? EducationLevel { get; private set; }

        public short? PersonalCode { get; set; }

        public MaritalStatus MaritalStatus { get; private set; }

        public DateTime? DateOfHire { get; set; }

        public DateTime? DateOfMarried { get; set; }

        public short? MemberQty { get; set; }

        public short? DependantsQty { get; set; }
        public short? NumberOfChildren { get; private set; }
        public short? StudentChildCount { get; set; }

        public string? ZipCode { get; set; }

        public string? Phone { get; set; }

        public string? Mobile { get; set; }

        public string? Mail { get; set; }

        public string? Descriptions { get; set; }

        public byte[]? Image { get; set; }

        public bool? Enablity { get; set; }

        public bool? Visiblity { get; set; }

        public bool? Remove { get; set; }

        public string? PasswordEmergency { get; set; }


        public Guid? FkchildId { get; set; }


        // Audit
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string CreatedBy { get; private set; } = string.Empty;

        // Navigation
        public virtual Person Person { get; private set; } = null!;

        // Constructor
        protected PersonProfile() { }

        public PersonProfile(Guid personId, short? numberOfChildren, string? address,
                           MaritalStatus maritalStatus, string? jobTitle,
                           string? educationLevel, DateTime effectiveFrom, string createdBy)
        {
            FkPersonId = personId;
            NumberOfChildren = numberOfChildren;
            Address = address;
            MaritalStatus = maritalStatus;
            JobTitle = jobTitle;
            EducationLevel = educationLevel;
            CreatedBy = createdBy;
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
