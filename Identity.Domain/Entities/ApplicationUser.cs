using Core.Domain.Interfaces;
using Core.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    /*
     از Guid به عنوان کلید اصلی استفاده می‌کنیم (قابل گسترش‌تر).

    فیلدهایی مثل IsActive, IsLocked, LastLoginIp، و LastLoginTime برای سیاست‌های امنیتی بعدی استفاده می‌شن.

    RefreshTokens برای مدیریت سشن‌ها.
     */
    public class ApplicationUser : IdentityUser<Guid>, IAggregateRoot
    {
        public Guid FkPersonId { get; private set; }

        public FullName? FullName { get; private set; }
        public bool IsActive { get; private set; } = true;

        public string? LastLoginIp { get; private set; }
        public DateTime? LastLoginTime { get; private set; }
        public bool IsLocked { get; private set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

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

        private void Touch() => UpdatedAt = DateTime.UtcNow;

        public void SetFullName(FullName fullName)
        {
            FullName = fullName;
            Touch();
        }
        public void SetFullName(string firstName , string lastName)
        {
            FullName = FullName.Create(firstName , lastName);
            Touch();
        }
        public void UpdateLoginInfo(string ip)
        {
            LastLoginIp = ip;
            LastLoginTime = DateTime.UtcNow;
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
    }
}
