using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.ValueObjects
{/// <summary>
 /// Value Object برای محدوده سلسله مراتبی
 /// </summary>
    public class HierarchicalScope : ValueObject
    {
        public int? MaxLevels { get; } // تعداد سطوح قابل دسترسی
        public bool IncludeSelf { get; }
        public bool IncludeDescendants { get; }
        public HierarchicalScope(int? maxLevels = null, bool includeSelf = true, bool includeDescendants = true)
        {
            MaxLevels = maxLevels;
            IncludeSelf = includeSelf;
            IncludeDescendants = includeDescendants;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return MaxLevels ?? 0;
            yield return IncludeSelf;
            yield return IncludeDescendants;
        }
    }
}
}
