using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.ValueObjects
{
    // <summary>
    /// Value Object برای محدوده زمانی
    /// </summary>
    public class TemporalScope : ValueObject
    {
        public DateTime? StartDate { get; }
        public DateTime? EndDate { get; }
        public bool IsRecurring { get; }
        public string RecurrencePattern { get; } // مثلاً "Daily", "Weekly", "Monthly"
        public TemporalScope(DateTime? startDate = null, DateTime? endDate = null,
                   bool isRecurring = false, string recurrencePattern = null)
        {
            StartDate = startDate;
            EndDate = endDate;
            IsRecurring = isRecurring;
            RecurrencePattern = recurrencePattern;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StartDate ?? DateTime.MinValue;
            yield return EndDate ?? DateTime.MaxValue;
            yield return IsRecurring;
            yield return RecurrencePattern ?? string.Empty;
        }
    }
}
}
