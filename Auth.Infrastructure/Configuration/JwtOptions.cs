using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Configuration
{
    public class JwtOptions
    {
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int AccessTokenExpiryMinutes { get; set; } = 60;
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }
}
