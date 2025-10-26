using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Core.Domain.ValueObjects
{
    public sealed class FullName : Core.Domain.Common.ValueObject
    {
        public string FirstName { get; }
        public string LastName { get; }

        private Name(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public static Name Create(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty", nameof(lastName));

            return new Name(firstName.Trim(), lastName.Trim());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return FirstName.ToLowerInvariant();
            yield return LastName.ToLowerInvariant();
        }

        public override string ToString() => $"{FirstName} {LastName}";
    }
}
