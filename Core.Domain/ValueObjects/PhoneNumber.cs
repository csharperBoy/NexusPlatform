using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.ValueObjects
{
    public sealed class PhoneNumber : Core.Domain.Common.ValueObject
    {
        public string Value { get; }

        private PhoneNumber(string value) => Value = value;

        public static PhoneNumber Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number cannot be empty", nameof(value));

            var digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits.Length < 7 || digits.Length > 15)
                throw new ArgumentException("Invalid phone number length", nameof(value));

            return new PhoneNumber(digits);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}