using Microsoft.AspNetCore.Identity;

namespace Authentication.Infrastructure.Identity
{
    /*
     از Guid به عنوان کلید اصلی استفاده می‌کنیم (قابل گسترش‌تر).

    فیلدهایی مثل IsActive, IsLocked, LastLoginIp، و LastLoginTime برای سیاست‌های امنیتی بعدی استفاده می‌شن.

    RefreshTokens برای مدیریت سشن‌ها.
     */
    public class ApplicationUser : IdentityUser<Guid>
    {
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

    }
}
