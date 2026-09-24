using System.Security.Cryptography;
using System.Text;

namespace Trader.Infrastructure.EasyTrader
{
    internal static class PkceGenerator
    {
        /// <summary>Verifier تصادفی 43 کاراکتری (base64url).</summary>
        public static string GenerateVerifier()
        {
            Span<byte> bytes = stackalloc byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Base64UrlEncode(bytes);
        }

        /// <summary>challenge = base64url(SHA256(verifier))</summary>
        public static string GenerateChallenge(string verifier)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(verifier));
            return Base64UrlEncode(hash);
        }

        public static string GenerateState()
        {
            Span<byte> bytes = stackalloc byte[16];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static string Base64UrlEncode(ReadOnlySpan<byte> input)
        {
            var s = Convert.ToBase64String(input);
            return s.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}