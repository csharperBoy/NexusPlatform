using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.HR;
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
    public class Person : IAuditableEntity, IAggregateRoot, IEntity<Guid>
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        public Guid Id { get; private set; } = Guid.NewGuid();

        // اطلاعات ثابت (هرگز تغییر نمی‌کنند)
        public NationalCode NationalCode { get; private set; } = null!;
        public FullName FullName { get; private set; } = null!;

        public DateTime? BirthDate { get; private set; }
        public string? BirthPlace { get; private set; }

        public string? FatherName { get; private set; }
        public Gender? Gender { get; private set; }

        public string? OneTimePassword { get; set; }

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
        public void SetFullName(FullName fullName)
        {
            FullName = fullName;
            Touch();
        }
        public void SetFullName(string firstName, string lastName)
        {
            FullName = FullName.Create(firstName, lastName);
            Touch();
        }
        private void Touch() => ModifiedAt = DateTime.UtcNow;
        public bool ApplyChange(
         string? _NationalCode,
         string? _FirstName,
         string? _LastName,
        DateTime? _BirthDate,
         string? _BirthPlace,
        string? _FatherName,
         Gender? _Gender
          )
        {
            bool hasChange = false;

            if (_NationalCode != null && _NationalCode != this.NationalCode.ToString())
            {
                this.NationalCode = NationalCode.Create(_NationalCode);
                hasChange = true;
            }
            if ((_FirstName != null && _FirstName != this.FullName?.FirstName) || (_LastName != null && _LastName != this.FullName?.LastName))
            {
                this.SetFullName(_FirstName, _LastName);
                hasChange = true;
            }
            if (_BirthDate != null && _BirthDate != this.BirthDate)
            {
                this.BirthDate = _BirthDate;
                hasChange = true;
            }
            if (_BirthPlace != null && _BirthPlace != this.BirthPlace)
            {
                this.BirthPlace = _BirthPlace;
                hasChange = true;
            }
            if (_FatherName != null && _FatherName != this.FatherName)
            {
                this.FatherName = _FatherName;
                hasChange = true;
            }
            if (_Gender != null && _Gender != this.Gender)
            {
                this.Gender = _Gender;
                hasChange = true;
            }
            if (hasChange)
                Touch();
            return hasChange;
        }
    }


}