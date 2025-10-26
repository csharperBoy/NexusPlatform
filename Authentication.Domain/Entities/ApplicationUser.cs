using Core.Domain.ValueObjects;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Domain.Entities
{
    /*
     از Guid به عنوان کلید اصلی استفاده می‌کنیم (قابل گسترش‌تر).

    فیلدهایی مثل IsActive, IsLocked, LastLoginIp، و LastLoginTime برای سیاست‌های امنیتی بعدی استفاده می‌شن.

    RefreshTokens برای مدیریت سشن‌ها.
     */
    public class ApplicationUser : IdentityUser<Guid>
    {
        // لینک به Person اصلی
        public Guid FkPersonId { get; private set; }

        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;

        // اطلاعات اضافی برای کنترل امنیت
        public string? LastLoginIp { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public bool IsLocked { get; set; }

        // برای audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
        // Constructor
        protected ApplicationUser() { }

        public ApplicationUser(Guid personId, string userName, string email)
        {
            FkPersonId = personId;
            UserName = userName;
            Email = email;
            EmailConfirmed = true; // بعداً از طریق ایمیل تأیید شود
        }

        public void UpdateLoginInfo(string ipAddress)
        {
            LastLoginIp = ipAddress;
            LastLoginTime = DateTime.UtcNow;
        }

    }
}
