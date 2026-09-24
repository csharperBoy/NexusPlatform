using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities
{
    public class TraderAccount : BaseEntity
    {
        public BrokerType Broker { get; private set; }
        public string Name { get; private set; } = default!;
        public string Username { get; private set; } = default!;

        /// <summary>رمز عبور رمزنگاری‌شده</summary>
        public string EncryptedPassword { get; private set; } = default!;

        /// <summary>Session رمزنگاری‌شده (JSON serialized + encrypted)</summary>
        public string? EncryptedSession { get; private set; }

        /// <summary>زمان انقضای session</summary>
        public DateTimeOffset? SessionExp { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        private TraderAccount() { }

        public static TraderAccount Create(
            BrokerType broker,
            string name,
            string username,
            string encryptedPassword)
        {
            return new TraderAccount
            {
                Broker = broker,
                Name = name,
                Username = username,
                EncryptedPassword = encryptedPassword,
                CreatedAt = DateTimeOffset.UtcNow,
            };
        }

        public void SetInfo(string name, string username, string? encryptedPassword)
        {
            Name = name;
            Username = username;
            if (encryptedPassword is not null)
                EncryptedPassword = encryptedPassword;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void SetSession(string encryptedSession, DateTimeOffset? sessionExp)
        {
            EncryptedSession = encryptedSession;
            SessionExp = sessionExp;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void ClearSession()
        {
            EncryptedSession = null;
            SessionExp = null;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public SessionStatus GetSessionStatus()
        {
            if (string.IsNullOrEmpty(EncryptedSession)) return SessionStatus.Empty;
            if (SessionExp is null) return SessionStatus.Invalid;
            if (SessionExp <= DateTimeOffset.UtcNow) return SessionStatus.Expired;
            return SessionStatus.Valid;
        }
    }
}