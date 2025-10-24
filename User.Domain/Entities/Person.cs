using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string NationalCode { get; private set; } = string.Empty;
        public string? FirstName { get; private set; } = string.Empty;
        public string? LastName { get; private set; } = string.Empty;
        public DateTime? BirthDate { get; private set; }
        public string? BirthPlace { get; private set; } = string.Empty;


        public string? FatherName { get; private set; }

        public Gender? Gender { get; private set; }


        public string? OneTimePassword { get; set; }
        // Audit
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string CreatedBy { get; private set; } = string.Empty;

        // Constructor
        protected Person() { } // For EF Core

        public Person(string nationalCode, string firstName, string lastName,
                     DateTime birthDate, string birthPlace, string createdBy)
        {
            NationalCode = nationalCode;
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            BirthPlace = birthPlace;
            CreatedBy = createdBy;
        }

        // روش‌های کسب اطلاعات
        public string GetFullName() => $"{FirstName} {LastName}";
        public int GetAge() => DateTime.Now.Year - (BirthDate ?? DateTime.Now).Year;
    }

    public enum Gender
    {
        Other = 0,
        Male = 1,
        Female = 2
    }
}
