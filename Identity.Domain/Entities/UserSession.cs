using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    public class UserSession
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; } = default!;
        public string DeviceInfo { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

        public ApplicationUser User { get; set; } = default!;
    }

}