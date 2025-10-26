using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.ValueObjects
{
    public sealed class NationalCode : Core.Domain.Common.ValueObject
    {
        public string Value { get; }

        private NationalCode(string value) => Value = value;

        public static NationalCode Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("National code cannot be empty", nameof(value));

            if (value.Length != 10 || !value.All(char.IsDigit))
                throw new ArgumentException("Invalid national code format", nameof(value));

            return new NationalCode(value);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
