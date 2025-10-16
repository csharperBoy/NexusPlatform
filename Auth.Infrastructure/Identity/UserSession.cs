using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Identity
{
    public class UserSession
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsActive => RevokedAt == null;
    }
}
