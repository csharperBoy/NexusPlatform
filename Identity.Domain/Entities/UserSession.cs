using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    public class UserSession : AuditableEntity, IEntity<Guid>
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public Guid UserId { get; private set; }
        public string RefreshToken { get; private set; } = default!;
        public string DeviceInfo { get; private set; } = string.Empty;
        public string IpAddress { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }

        public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

        public ApplicationUser User { get; private set; } = default!;

        protected UserSession() { }

        public UserSession(Guid userId, string token, string device, string ip, DateTime expiresAt)
        {
            UserId = userId;
            RefreshToken = token;
            DeviceInfo = device;
            IpAddress = ip;
            ExpiresAt = expiresAt;
        }

        public void Revoke()
        {
            RevokedAt = DateTime.UtcNow;
            LastModifiedOn = DateTime.UtcNow;
        }
    }
}