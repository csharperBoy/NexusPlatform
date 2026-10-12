using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.ExternalApiClient
{
    /// <summary>
    /// Session خروجی از لاگین هر broker.
    /// abstract — هر broker شکل خودش رو داره.
    /// </summary>
    public abstract record ExternalSystemSession
    {
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
        public abstract DateTimeOffset? ExpiresAt { get; }
    }
}
