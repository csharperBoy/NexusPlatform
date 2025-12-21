using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Extensions
{
    public static class GuidExtensions
    {
        public static Guid? TryParseGuid(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (Guid.TryParse(value, out var guid))
                return guid;

            // اگر "system" یا دیگر مقادیر بود
            return value.ToLower() switch
            {
                "system" => Guid.Empty,
                "admin" => Guid.Parse("00000000-0000-0000-0000-000000000001"),
                _ => null
            };
        }

        public static string? ToStringOrNull(this Guid? guid)
        {
            return guid?.ToString();
        }
    }
}
