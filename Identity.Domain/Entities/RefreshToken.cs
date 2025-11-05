using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    /*
     شامل اطلاعات IP و دستگاه برای سشن‌های هم‌زمان (در آینده برای خروج از دستگاه‌های دیگر استفاده می‌کنیم).
    IsRevoked برای باطل‌کردن دستی RefreshTokenها (مثلاً بعد از تغییر پسورد یا logout all).
      */
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Token { get; set; } = default!;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedByIp { get; set; }
        public string? DeviceInfo { get; set; }
        public string? ReplacedByToken { get; set; } // برای rotate کردن توکن‌ها

        public ApplicationUser User { get; set; } = default!;
    }
}