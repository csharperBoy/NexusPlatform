using Core.Domain.Interfaces;
using Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace User.Domain.Entities
{
    /// <summary>
    /// اطلاعات ثابت و غیرقابل تغییر افراد
    /// مثل: کد ملی، نام، نام خانوادگی، تاریخ تولد
    /// </summary>
    public class Person : IAggregateRoot, IEntity<Guid>
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        // اطلاعات ثابت (هرگز تغییر نمی‌کنند)
        public NationalCode NationalCode { get; private set; } = null!;
        public FullName FullName { get; private set; } = null!;
        public DateTime? BirthDate { get; private set; }
        public string? BirthPlace { get; private set; }

        public string? FatherName { get; private set; }
        public Gender? Gender { get; private set; }

        public string? OneTimePassword { get; set; }

        // Audit
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string CreatedBy { get; private set; } = string.Empty;

        // Constructor for EF
        protected Person() { }

        public Person(NationalCode nationalCode, FullName fullName,
                      DateTime birthDate, string birthPlace, string createdBy)
        {
            NationalCode = nationalCode ?? throw new ArgumentNullException(nameof(nationalCode));
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            BirthDate = birthDate;
            BirthPlace = birthPlace;
            CreatedBy = createdBy;
        }

        // روش‌های کسب اطلاعات
        public string GetFullName() => FullName.ToString();
        public int GetAge()
        {
            if (!BirthDate.HasValue) return 0;
            var today = DateTime.Today;
            var age = today.Year - BirthDate.Value.Year;
            if (BirthDate.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    public enum Gender
    {
        Other = 0,
        Male = 1,
        Female = 2
    }
}