using Core.Domain.Common;
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
namespace People.Domain.Entities
{
    /// <summary>
    /// اطلاعات ثابت و غیرقابل تغییر افراد
    /// مثل: کد ملی، نام، نام خانوادگی، تاریخ تولد
    /// </summary>
    public class NaturalPerson : BaseEntity, IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion



        // اطلاعات ثابت (هرگز تغییر نمی‌کنند)
        public NationalCode NationalCode { get; private set; } = null!;
        public FullName FullName { get; private set; } = null!;
        public Guid FkPartyId { get; private set; }
        public DateTime? BirthDate { get; private set; }
        public string? BirthPlace { get; private set; }

        public string? FatherName { get; private set; }
        public Gender? Gender { get; private set; }

        //navigation

        public virtual Party Party { get; private set; } = null!;

        public virtual ICollection<NaturalPersonProfile> NaturalPersonProfiles { get; private set; } = new List<NaturalPersonProfile>();
        // Constructor for EF
        protected NaturalPerson() { }

        public NaturalPerson(
            NationalCode nationalCode,
            FullName fullName,
            DateTime birthDate,
            string birthPlace,
            string fatherName,
            Gender? gender,
            string? createdBy)
        {
            NationalCode = nationalCode ?? throw new ArgumentNullException(nameof(nationalCode));
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            BirthDate = birthDate;
            BirthPlace = birthPlace;
            FatherName = fatherName;
            Gender = gender;
            CreatedBy = createdBy;
        }
        public void setParty(Guid partyId)
        {
            FkPartyId = partyId;
        }
        public NaturalPerson(
            string? _NationalCode,
            string? _FirstName,
            string? _LastName,
            DateTime? _birthDate,
            string? _birthPlace,
            string _fatherName,
            Gender? _gender,
            string? _createdBy)
        {
            NationalCode = NationalCode.Create(_NationalCode);
            SetFullName(_FirstName, _LastName);
            BirthDate = _birthDate;
            BirthPlace = _birthPlace;
            FatherName = _fatherName;
            Gender = _gender ?? Core.Shared.Enums.HR.Gender.Other;
            CreatedBy = _createdBy;
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
       public void Touch() => ModifiedAt = DateTime.UtcNow;
        public async Task<bool> ApplyChange(
         Optional<string?> _NationalCode,
         Optional<string?> _FirstName,
         Optional<string?> _LastName,
         Optional<DateTime?> _BirthDate,
         Optional<string?> _BirthPlace,
         Optional<string?> _FatherName,
         Optional<Gender?> _Gender

          )
        {
            bool hasChange = false;

            if (_NationalCode.IsSet && _NationalCode.Value?.Trim() != NationalCode.ToString())
            {
                NationalCode = NationalCode.Create(_NationalCode.Value?.Trim());
                hasChange = true;
            }
            if (_FirstName.IsSet && _FirstName.Value?.Trim() != FullName?.FirstName.Trim() || _LastName.IsSet && _LastName.Value?.Trim() != FullName?.LastName.Trim())
            {
                SetFullName(_FirstName.Value?.Trim(), _LastName.Value?.Trim());
                hasChange = true;
            }
            if (_BirthDate.IsSet && _BirthDate.Value != BirthDate)
            {
                BirthDate = _BirthDate.Value;
                hasChange = true;
            }
            if (_BirthPlace.IsSet && _BirthPlace.Value?.Trim() != BirthPlace?.Trim())
            {
                BirthPlace = _BirthPlace.Value?.Trim();
                hasChange = true;
            }
            if (_FatherName.IsSet && _FatherName.Value?.Trim() != FatherName?.Trim())
            {
                FatherName = _FatherName.Value?.Trim();
                hasChange = true;
            }
            if (_Gender.IsSet && _Gender.Value != Gender)
            {
                Gender = _Gender.Value;
                hasChange = true;
            }


            if (hasChange)
                Touch();
            return hasChange;
        }
    }


}