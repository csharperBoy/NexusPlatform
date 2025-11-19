using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.ValueObjects
{
    /// <summary>
    /// Value Object برای محدوده جغرافیایی
    /// </summary>
    public class GeographicScope : ValueObject
    {
        public Guid? BranchId { get; }
        public Guid? RegionId { get; }
        public Guid? ProvinceId { get; }
        public GeographicScope(Guid? branchId = null, Guid? regionId = null, Guid? provinceId = null)
        {
            BranchId = branchId;
            RegionId = regionId;
            ProvinceId = provinceId;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return BranchId ?? Guid.Empty;
            yield return RegionId ?? Guid.Empty;
            yield return ProvinceId ?? Guid.Empty;
        }
    }
}
}
