using Core.Domain.Common;
using Core.Domain.Interfaces;
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
    public class RefreshToken : AuditableEntity, IEntity<Guid>
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public Guid UserId { get; private set; }
        public string Token { get; private set; } = default!;
        public DateTime ExpiryDate { get; private set; }
        public bool IsRevoked { get; private set; }
        public string? CreatedByIp { get; private set; }
        public string? DeviceInfo { get; private set; }
        public string? ReplacedByToken { get; private set; }

        public ApplicationUser User { get; private set; } = default!;

        protected RefreshToken() { }

        public RefreshToken(Guid userId, string token, DateTime expiry, string? ip, string? device)
        {
            UserId = userId;
            Token = token;
            ExpiryDate = expiry;
            CreatedByIp = ip;
            DeviceInfo = device;
        }

        public void Revoke(string? replacedBy)
        {
            IsRevoked = true;
            ReplacedByToken = replacedBy;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}