using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Domain.ValueObjects
{
    public sealed class Email : Core.Domain.Common.ValueObject
    {
        public string Value { get; }

        private Email(string value) => Value = value;

        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty", nameof(value));

            if (!value.Contains("@") || value.Length < 5)
                throw new ArgumentException("Invalid email format", nameof(value));

            return new Email(value.Trim().ToLowerInvariant());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}