using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities
{
   
    public class TraderAccount : BaseEntity
    {

        public string Name { get; private set; } = default!;
        public string Username { get; private set; } = default!;

        /// <summary>رمز عبور رمزنگاری‌شده (توسط ISecretProtector)</summary>
        public string EncryptedPassword { get; private set; } = default!;

        /// <summary>توکن Bearer رمزنگاری‌شده</summary>
        public string? EncryptedToken { get; private set; }

        /// <summary>زمان انقضای توکن (UTC)</summary>
        public DateTimeOffset? TokenExp { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        private TraderAccount() { } // EF Core

        public static TraderAccount Create(
            string name,
            string username,
            string encryptedPassword)
        {
            return new TraderAccount
            {
                Name = name,
                Username = username,
                EncryptedPassword = encryptedPassword,
                CreatedAt = DateTimeOffset.UtcNow,
            };
        }

        public void UpdateInfo(string name, string username, string? encryptedPassword)
        {
            Name = name;
            Username = username;
            if (encryptedPassword is not null)
                EncryptedPassword = encryptedPassword;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void SetToken(string encryptedToken, DateTimeOffset? tokenExp)
        {
            EncryptedToken = encryptedToken;
            TokenExp = tokenExp;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void ClearToken()
        {
            EncryptedToken = null;
            TokenExp = null;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public TokenStatus GetTokenStatus()
        {
            if (string.IsNullOrEmpty(EncryptedToken)) return TokenStatus.Empty;
            if (TokenExp is null) return TokenStatus.Invalid;
            if (TokenExp <= DateTimeOffset.UtcNow) return TokenStatus.Expired;
            return TokenStatus.Valid;
        }
    }
}
