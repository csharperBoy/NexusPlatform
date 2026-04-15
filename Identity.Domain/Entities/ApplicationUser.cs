using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Core.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Identity.Domain.Entities
{
    /*
     از Guid به عنوان کلید اصلی استفاده می‌کنیم (قابل گسترش‌تر).

    فیلدهایی مثل IsActive, IsLocked, LastLoginIp، و LastLoginTime برای سیاست‌های امنیتی بعدی استفاده می‌شن.

    RefreshTokens برای مدیریت سشن‌ها.
     */
    public class ApplicationUser : IdentityUser<Guid>, IAggregateRoot , IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        public Guid? FkPersonId { get; private set; }
        public string? NickName {  get;  set; }
        public bool IsActive { get; private set; } = true;

        public string? LastLoginIp { get; private set; }
        public DateTime? LastLoginTime { get; private set; }
        public bool IsLocked { get; private set; }

      

        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
        public ICollection<UserSession> Sessions { get; private set; } = new List<UserSession>();

        protected ApplicationUser() : base() { }

        public ApplicationUser(Guid personId, string userName, string email)
            : base(userName)
        {
            FkPersonId = personId;
            UserName = userName;
            Email = email;
            EmailConfirmed = true;
            NormalizedUserName = userName.ToUpperInvariant();
            NormalizedEmail = email.ToUpperInvariant();

            SecurityStamp = Guid.NewGuid().ToString();
        }
        public ApplicationUser(
            string _UserName,
        string _Email,
        string? _NickName,
        string? _phoneNumber,
        Guid? _personId = null
            )
             : base(_UserName)
        {
            FkPersonId = _personId;
            UserName = _UserName;
            Email = _Email;
            EmailConfirmed = true;
            NickName = _NickName;
            PhoneNumber = _phoneNumber;
            NormalizedUserName = _UserName.ToUpperInvariant();
            NormalizedEmail = _Email.ToUpperInvariant();

            SecurityStamp = Guid.NewGuid().ToString();
        }

        private void Touch() => ModifiedAt = DateTime.UtcNow;

      
        public void UpdateLoginInfo(string ip)
        {
            LastLoginIp = ip;
            LastLoginTime = DateTime.UtcNow;
            Touch();
        }
        public void SetPersonId(Guid personId)
        {
            FkPersonId = personId;
            Touch();
        }
        public void Lock()
        {
            IsLocked = true;
            Touch();
        }

        public void Unlock()
        {
            IsLocked = false;
            Touch();
        }

        public void Deactivate()
        {
            IsActive = false;
            Touch();
        }

        public void Activate()
        {
            IsActive = true;
            Touch();
        }

        public bool ApplyChange(
             string _UserName,
             string? _NickName,
             string? _Password,
             string? _Email,
             string? _phoneNumber,
            UserManager<ApplicationUser> _userManager,
            Guid? _personId = null
            )
        {
            bool hasChange = false;
            // آپدیت فیلدها
            if (_UserName != null && _UserName != this.UserName)
            {
                this.UserName = _UserName;
                hasChange = true;
            }
            if (_NickName != null && _NickName != this.NickName)
            {
                this.NickName = _NickName;
                hasChange = true;
            }
           
            if (_Email != null && _Email.ToString() != this.Email)
            {
                this.Email =_Email.ToString();
                hasChange = true;
            }
            if (_phoneNumber != null && _phoneNumber != this.PhoneNumber)
            {
                this.PhoneNumber = _phoneNumber;
                hasChange = true;
            }
            if (_Password != null)
            {
                this.PasswordHash = _userManager.PasswordHasher.HashPassword(this, _Password);
                hasChange = true;
            }

            if (_personId != null && _personId != this.FkPersonId)
            {
                this.SetPersonId((Guid)_personId);
                hasChange = true;
            }

            if (hasChange)
            {
                Touch();
            }
            return hasChange;
        }
    }
}
